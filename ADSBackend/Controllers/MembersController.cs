using ADSBackend.Data;
using ADSBackend.Models;
using ADSBackend.Models.MemberViewModels;
using ADSBackend.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index(string search)
        {
            ViewData["Search"] = search;
            var members = await _context.Member
                .Include(m => m.ClubMembers)
                .ThenInclude(cm => cm.Club)
                .Include(meet => meet.MeetingAttendees)
                .ThenInclude(ma => ma.Meeting)
                .ToListAsync();

            if (!String.IsNullOrEmpty(search))
            {
                members = await _context.Member
                .Where(s => s.FullName.Contains(search))
                .Include(m => m.ClubMembers)
                .ThenInclude(cm => cm.Club)
                .Include(meet => meet.MeetingAttendees)
                .ThenInclude(ma => ma.Meeting)
                .ToListAsync();
            }

            return View(members);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .Include(m => m.ClubMembers)
                .ThenInclude(cm => cm.Club)
                .Include(m => m.MeetingAttendees)
                .ThenInclude(ma => ma.Meeting)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public async Task<IActionResult> Create()
        {
            var clubs = await _context.Club.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Clubs = new MultiSelectList(clubs, "ClubId", "Name");
            var meetings = await _context.Meeting.OrderBy(m => m.EventName).ToListAsync();
            ViewBag.Meetings = new MultiSelectList(meetings, "MeetingId", "EventName");

            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Gender,Address,City,ZipCode,Grade,RecruitedBy,profileImageSource,Description,Email,Phone,Password,ClubIds,MeetingIds")] MemberViewModel vm)
        {
            if (ModelState.IsValid)
            {
                PasswordHash ph = PasswordHasher.Hash(vm.Password);

                var member = new Member
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    FullName = vm.FirstName + " " + vm.LastName,
                    Gender = vm.Gender,
                    Address = vm.Address,
                    City = vm.City,
                    ZipCode = vm.ZipCode,
                    Grade = vm.Grade,
                    RecruitedBy = vm.RecruitedBy,
                    Email = vm.Email,
                    Phone = vm.Phone,
                    profileImageSource = vm.profileImageSource,
                    Description = vm.Description,
                    Password = ph.HashedPassword,
                    Salt = ph.Salt
                };
                _context.Member.Add(member);
                await _context.SaveChangesAsync();

                if (vm.ClubIds == null) vm.ClubIds = new System.Collections.Generic.List<int>();
                if (vm.MeetingIds == null) vm.MeetingIds = new System.Collections.Generic.List<int>();

                // add this new member to the relevant clubs
                foreach (var clubId in vm.ClubIds)
                {
                    var clubMember = new ClubMember
                    {
                        ClubId = clubId,
                        MemberId = member.MemberId
                    };

                    _context.ClubMember.Add(clubMember);
                    await _context.SaveChangesAsync();
                }
                foreach (var meetingId in vm.MeetingIds)
                {
                    var meetingAttendees = new MeetingAttendees
                    {
                        MeetingId = meetingId,
                        MemberId = member.MemberId
                    };

                    _context.MeetingAttendees.Add(meetingAttendees);
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var clubs = await _context.Club.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Clubs = new MultiSelectList(clubs, "ClubId", "Name");
            var meetings = await _context.Meeting.OrderBy(m => m.EventName).ToListAsync();
            ViewBag.Meetings = new MultiSelectList(meetings, "MeetingId", "EventName");

            return View(vm);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.Include(m => m.ClubMembers).Include(m => m.MeetingAttendees).FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            var clubs = await _context.Club.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Clubs = new MultiSelectList(clubs, "ClubId", "Name");
            var meetings = await _context.Meeting.OrderBy(m => m.EventName).ToListAsync();
            ViewBag.Meetings = new MultiSelectList(meetings, "MeetingId", "EventName");

            var vm = new MemberViewModel(member);
            return View(vm);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,FirstName,LastName,Gender,Address,City,ZipCode,Grade,Email,Phone,profileImageSource,Description,ClubIds,MeetingIds")] MemberViewModel vm)
        {
            var member = await _context.Member
                .Include(m => m.ClubMembers)
                .ThenInclude(cm => cm.Club)
                .Include(m => m.MeetingAttendees)
                .ThenInclude(ma => ma.Meeting)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (id != member.MemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    member.FirstName = vm.FirstName;
                    member.LastName = vm.LastName;
                    member.FullName = vm.FirstName + " " + vm.LastName;
                    member.Gender = vm.Gender;
                    member.Address = vm.Address;
                    member.City = vm.City;
                    member.ZipCode = vm.ZipCode;
                    member.Grade = vm.Grade;
                    member.Email = vm.Email;
                    member.Phone = vm.Phone;
                    member.profileImageSource = vm.profileImageSource;
                    member.Description = vm.Description;

                    _context.Update(member);
                    await _context.SaveChangesAsync();

                    if (vm.ClubIds == null) vm.ClubIds = new System.Collections.Generic.List<int>();

                    var oldClubIds = member.ClubMembers.Select(cm => cm.Club.ClubId).ToList();
                    foreach (var clubId in vm.ClubIds)
                    {
                        if (!oldClubIds.Contains(clubId))
                        {
                            var clubMember = new ClubMember
                            {
                                ClubId = clubId,
                                MemberId = member.MemberId
                            };

                            _context.ClubMember.Add(clubMember);
                            await _context.SaveChangesAsync();
                        }
                    }
                    foreach (var oldClubId in oldClubIds)
                    {
                        if (!vm.ClubIds.Contains(oldClubId))
                        {
                            _context.ClubMember.Remove(member.ClubMembers.FirstOrDefault(cm => cm.ClubId == oldClubId && cm.MemberId == member.MemberId));
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (vm.MeetingIds == null) vm.MeetingIds = new System.Collections.Generic.List<int>();

                    var oldMeetingIds = member.MeetingAttendees.Select(ma => ma.Meeting.MeetingId).ToList();
                    foreach (var meetingId in vm.MeetingIds)
                    {
                        if (!oldMeetingIds.Contains(meetingId))
                        {
                            var meetingAttendees = new MeetingAttendees
                            {
                                MeetingId = meetingId,
                                MemberId = member.MemberId
                            };

                            _context.MeetingAttendees.Add(meetingAttendees);
                            await _context.SaveChangesAsync();
                        }
                    }
                    foreach (var oldMeetingId in oldMeetingIds)
                    {
                        if (!vm.MeetingIds.Contains(oldMeetingId))
                        {
                            _context.MeetingAttendees.Remove(member.MeetingAttendees.FirstOrDefault(ma => ma.MeetingId == oldMeetingId && ma.MemberId == member.MemberId));
                            await _context.SaveChangesAsync();
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.MemberId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var clubs = await _context.Club.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Clubs = new MultiSelectList(clubs, "ClubId", "Name");
            var meetings = await _context.Meeting.OrderBy(m => m.EventName).ToListAsync();
            ViewBag.Meetings = new MultiSelectList(meetings, "MeetingId", "EventName");

            return View(vm);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .Include(m => m.ClubMembers)
                .ThenInclude(cm => cm.Club)
                .Include(m => m.MeetingAttendees)
                .ThenInclude(ma => ma.Meeting)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Member.FindAsync(id);
            _context.Member.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Member.Any(e => e.MemberId == id);
        }
    }
}
