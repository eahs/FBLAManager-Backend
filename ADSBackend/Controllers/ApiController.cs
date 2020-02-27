using ADSBackend.Data;
using ADSBackend.Models;
using ADSBackend.Models.AccountViewModels;
using ADSBackend.Models.ApiModels;
using ADSBackend.Models.MemberViewModels;
using ADSBackend.Services;
using ADSBackend.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileTypeChecker;
using FileTypeChecker.Abstracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ADSBackend.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly Services.Configuration Configuration;
        private readonly Services.Cache _cache;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private static Random random = new Random();

        public ApiController(Services.Configuration configuration, Services.Cache cache, ApplicationDbContext context, IEmailSender emailSender)
        {
            Configuration = configuration;
            _cache = cache;
            _context = context;
            _emailSender = emailSender;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public async Task<Session> IsAuthorized()
        {
            if (Request.Headers.ContainsKey("auth"))
            {
                string key = Request.Headers["auth"];

                var session = await _context.Session.FirstOrDefaultAsync(s => s.Key == key);

                return session;
            }

            return null;
        }

        public async Task<Session> CreateSession (Member member)
        {
            var sessionKey = System.Guid.NewGuid().ToString();
            var session = await _context.Session?.FirstOrDefaultAsync(s => s.MemberId == member.MemberId);

            if (session != null)
            {
                sessionKey = session.Key;
                session.LastAccessTime = DateTime.Now;
                _context.Update(session);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Create a new session
                session = new Session
                {
                    MemberId = member.MemberId,
                    Key = sessionKey,
                    Email = member.Email,
                    LastAccessTime = DateTime.Now
                };

                _context.Add(session);
                await _context.SaveChangesAsync();
            }

            return session;
        }

        [HttpPost("ForgotPassword")]
        public async Task<object> ForgotPassword(IFormCollection forms)
        {
            var member = await _context.Member.FirstOrDefaultAsync(m => m.Email == forms["email"]);
            if (member == null)
            {
                return new
                {
                    Status = "InvalidCredentials"
                };
            }

            var code = RandomString(10);
            var callbackUrl = Url.MemberResetPasswordCallbackLink(member.MemberId, code, Request.Scheme);

            await _emailSender.SendEmailAsync(member.Email, "FBLA Navigator: Password reset link", 
                $"Someone requested to change your password. If this was you, click here to reset your password: <a href='{callbackUrl}'>link</a>");
            return new
            {
                Status = "Success"
            };
        }

        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(int userId, [Bind("Email,Password,ConfirmPassword")] ResetPasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var member = await _context.Member.FirstOrDefaultAsync(m => m.Email == vm.Email);
                if (member == null || member.MemberId != userId)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction();
                }

                PasswordHash ph = PasswordHasher.Hash(vm.Password);
                member.Password = ph.HashedPassword;
                member.Salt = ph.Salt;

                _context.Update(member);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            return View(vm);
        }

        [HttpGet("ResetPasswordConfirmation")]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet("Messageboard")]
        public async Task<List<BoardPost>> GetMessageboard()
        {
            if (await IsAuthorized() == null)
                return new List<BoardPost>();

            var messageboard = await _context.BoardPost.Where(x => x.ClubId == 5).OrderByDescending(x => x.PostTime).Take(20).ToListAsync();

            return messageboard;
        }

        [HttpGet("SingleMeeting")]
        public async Task<Meeting> GetSingleMeeting(int meetingId)
        {
            if (await IsAuthorized() == null)
                return new Meeting();

            Meeting meeting = await _context.Meeting.Include(mem => mem.MeetingAttendees)
                                                    .ThenInclude(a => a.Member)
                                                    .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            meeting.Password = "";

            foreach (var attendee in meeting.MeetingAttendees)
            {
                attendee.Meeting = null;

                Member m = attendee.Member;
                attendee.Member = new Member
                {
                    MemberId = m.MemberId,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Email = m.Email
                };

            }

            return meeting;
        }

        [HttpGet("Members")]
        public async Task<List<Member>> GetMembers()
        {
            if (await IsAuthorized() == null)
                return new List<Member>();

            var members = await _context.Member.ToListAsync();
            foreach(Member member in members)
            {
                member.Password = "";
                member.Salt = "";
            }

            return members;
        }

        // GET: api/Meetings
        [HttpGet("Meetings")]
        public async Task<List<Meeting>> GetMeetings()
        {
            if (await IsAuthorized() == null)
                return new List<Meeting>();

            var meetings = await _context.Meeting
                .Include(mem => mem.MeetingAttendees)
                .ThenInclude(a => a.Member)
                .OrderBy(x => x.Start)
                .ToListAsync();

            foreach (var meeting in meetings)
            {
                meeting.Password = "";

                foreach (var attendee in meeting.MeetingAttendees)
                {
                    attendee.Meeting = null;

                    Member m = attendee.Member;
                    attendee.Member = new Member
                    {
                        MemberId = m.MemberId,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Email = m.Email
                    };

                }
            }

            //string test = JsonConvert.SerializeObject(meetings);

            return meetings;
        }

        [HttpPost("MeetingSignup")]
        public async Task<object> MeetingSignup(IFormCollection forms)
        {
            var _session = await IsAuthorized();
            if (_session == null)
                return new
                {
                    Status = "NotLoggedIn"
                };

            int meetingid;
            Int32.TryParse(forms["meetingid"], out meetingid);
            var _member = await _context.Member.FirstOrDefaultAsync(m => m.MemberId == _session.MemberId);
            var _meeting = await _context.Meeting.FirstOrDefaultAsync(m => m.MeetingId == meetingid);
            
            if(_meeting.Type == MeetingType.Meeting && forms["password"] != _meeting.Password)
            {
                return new
                {
                    Status = "InvalidCredentials"
                };
            }
            var meetingAttendees = new MeetingAttendees
            {
                MemberId = _member.MemberId,
                MeetingId = meetingid
            };

            var attendee = await _context.MeetingAttendees.FirstOrDefaultAsync(ma => ma.MeetingId == meetingid && ma.MemberId == _session.MemberId);

            if (attendee != null)
                return new
                {
                    Status = "Success",
                    Meeting = await GetSingleMeeting(meetingid)
                };

            _context.MeetingAttendees.Add(meetingAttendees);
            await _context.SaveChangesAsync();
            
            return new
            {
                Status = "Success",
                Meeting = await GetSingleMeeting(meetingid)
            };
        }

        [HttpGet("Officers")]
        public async Task<List<Officer>> GetOfficers(int? level)
        {
            if (await IsAuthorized() == null)
                return new List<Officer>();

            if (level == null)
                return await _context.Officer.OrderBy(x => x.Order).ToListAsync();

            return await _context.Officer.Where(o => o.Level == level).OrderBy(x => x.Order).ToListAsync();

        }

        [HttpGet("Profile")]
        public async Task<Member> GetProfile()
        {
            Session session = await IsAuthorized();
            if (session == null)
                return new Member();

            Member member = await _context.Member.FirstOrDefaultAsync(m => m.MemberId == session.MemberId);
            member.Password = "";
            member.Salt = "";

            return member;
        }

        [HttpPost("Login")]
        public async Task<object> Login(IFormCollection forms)
        {
            string _email, _password;

            if (!forms.ContainsKey("email") &&
                !forms.ContainsKey("password"))
                return new
                {
                    Status = "Invalid form data"
                };

            // Get fields from forms
            _email = forms["email"];
            _password = forms["password"];

            // Look up username and password in _context.Member
            Member member = await _context.Member.FirstOrDefaultAsync(m => m.Email == _email && m.Password == PasswordHasher.Hash(_password, m.Salt).HashedPassword);

            if (member == null)
            {
                // Member email or password is incorrect
                return new
                {
                    Status = "InvalidCredentials"
                };
            }

            var session = await CreateSession(member);

            member.Password = "";
            member.Salt = "";

            return new
            {
                Status = "LoggedIn",
                Key = session.Key,
                Profile = member
            };
        }

        [HttpPost("EditProfileImage")]
        public async Task<object> EditProfileImage(IFormCollection forms)
        {
            var session = await IsAuthorized();
            if (session == null)
                return new
                {
                    Status = "NotLoggedIn"
                };

            string encoded = forms["profileImageSource"];
            byte[] bytearray = System.Convert.FromBase64String(encoded);
            MemoryStream memstream = new MemoryStream();
            memstream.Write(bytearray, 0, bytearray.Length);
            IFileType fileType = FileTypeValidator.GetFileType(memstream);

            if (new[] { "png", "jpg" }.Contains(fileType.Extension))
            {
                // Save this to disk and update profile with the image filename for their profile
                try
                {
                    string path = Path.Combine("wwwroot/images/members/", session.MemberId + "temp." + fileType.Extension);
                    string finalpath = Path.Combine("wwwroot/images/members/", session.MemberId + ".jpg");

                    await System.IO.File.WriteAllBytesAsync(path, bytearray);

                    using (Image image = Image.Load(path))
                    {
                        int cropWidth = Math.Min(image.Width, image.Height);
                        int x = image.Width / 2 - cropWidth / 2;
                        int y = image.Height / 2 - cropWidth / 2;

                        image.Mutate(a => a
                             .Crop(new SixLabors.Primitives.Rectangle(x, y, cropWidth, cropWidth))
                             .Resize(150, 150));

                        // Delete the original file
                        System.IO.File.Delete(path);

                        // Save the new one
                        image.Save(finalpath, new JpegEncoder()); // Automatic encoder selected based on extension.                       
                    }

                    // Update profile with the filename being memberId plus .jpg
                    var member = await _context.Member.FirstOrDefaultAsync(a => a.MemberId == session.MemberId);
                    member.profileImageSource = "http://fblamanager.me/images/members/" + session.MemberId + ".jpg";
                    _context.Member.Update(member);
                    await _context.SaveChangesAsync();

                }
                catch (Exception e)
                {
                    // Things crashed
                }
            }
            return new
            {
                Status = "Success"
            };
        }

        [HttpPost("EditMember")]
        public async Task<object> EditMember(IFormCollection forms)
        {
            Session session = await IsAuthorized();
            if (session == null)
                return new { Status = "NotLoggedIn" };

            int grade;
            Int32.TryParse(forms["Grade"], out grade);

            Member member = await _context.Member.FirstOrDefaultAsync(m => m.MemberId == session.MemberId);
            member.FirstName = forms["FirstName"];
            member.LastName = forms["LastName"];
            member.Description = forms["Description"];
            member.FullName = forms["FirstName"] + " " + forms["LastName"];
            member.Gender = forms["Gender"];
            member.Address = forms["Address"];
            member.City = forms["City"];
            member.State = forms["State"];
            member.ZipCode = forms["ZipCode"];
            member.Grade = grade;
            member.Email = forms["Email"];
            member.Phone = forms["Phone"];

            _context.Member.Update(member);
            await _context.SaveChangesAsync();

            return new { Status = "Success" };
        }


        [HttpPost("CreateMember")]
        public async Task<object> CreateMember(IFormCollection forms)
        {
            int grade;

            if (forms["Email"] == "" || forms["Password"] == "")
                return new { Status = "MissingFields" };

            Int32.TryParse(forms["Grade"], out grade);

            grade = Math.Max(9, Math.Min(grade, 12));

            // Check to see if the member already exists - emails must be unique
            var existingMember = await _context.Member.FirstOrDefaultAsync(m => m.Email == forms["Email"]);
            if (existingMember != null)
            {
                return new { Status = "UserExists" };
            }

            PasswordHash ph = PasswordHasher.Hash(forms["Password"]);

            var member = new Member
            {
                FirstName = forms["FirstName"],
                LastName = forms["LastName"],
                FullName = forms["FirstName"] + " " + forms["LastName"],
                profileImageSource = "http://fblamanager.me/images/members/default.png",
                Gender = forms["Gender"],
                Address = forms["Address"],
                City = forms["City"],
                State = forms["State"],
                ZipCode = forms["ZipCode"],
                Grade = grade,
                RecruitedBy = forms["RecruitedBy"],
                Email = forms["Email"],
                Phone = forms["Phone"],
                Password = ph.HashedPassword,
                Salt = ph.Salt
            };
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            var session = await CreateSession(member);

            var response = new
            {
                Status = "Success",
                Key = session.Key
            };

            return response;
        }

        // GET: api/Config
        [HttpGet("Config")]
        public ConfigResponse GetConfig()
        {
            // TODO: extend this object to include some configuration items
            return new ConfigResponse();
        }
    }
}