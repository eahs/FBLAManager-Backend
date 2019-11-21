using ADSBackend.Data;
using ADSBackend.Models;
using ADSBackend.Models.ApiModels;
using ADSBackend.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly Services.Configuration Configuration;
        private readonly Services.Cache _cache;
        private readonly ApplicationDbContext _context;

        public ApiController(Services.Configuration configuration, Services.Cache cache, ApplicationDbContext context)
        {
            Configuration = configuration;
            _cache = cache;
            _context = context;
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

        // GET: api/Meetings
        [HttpGet("Meetings")]
        public async Task<List<Meeting>> GetMeetings()
        {
            if (await IsAuthorized() == null)
                return new List<Meeting>();

            var meetings = await _context.Meeting.Include(mem => mem.MeetingAttendees).ThenInclude(a => a.Member).OrderByDescending(x => x.Start).ToListAsync();

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

            var response = new
            {
                Status = "LoggedIn",
                Key = sessionKey
            };

            return response;
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

            var response = new
            {
                Status = "Success"
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