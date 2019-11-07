using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADSBackend.Data;
using ADSBackend.Models;
using ADSBackend.Models.Identity;
using Microsoft.AspNetCore.Identity;
using ADSBackend.Models.MeetingViewModels;

namespace ADSBackend.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MeetingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Meetings
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var meetings = await _context.Meeting
                .Include(mem => mem.MeetingAttendees)
                .ThenInclude(ma => ma.Member)
                .ToListAsync();
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return View(meetings);
            }
            else
            {
                return View(meetings.Where(m => m.OrganizerId == user.Id));
            }
        }

        // GET: Meetings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .Include(mem => mem.MeetingAttendees)
                .ThenInclude(ma => ma.Member)
                .FirstOrDefaultAsync(m => m.MeetingId == id);
            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // GET: Meetings/Create
        public async Task<IActionResult> Create()
        {
            var members = await _context.Member.OrderBy(c => c.Username).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Username");
            return View();
        }

        // POST: Meetings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MeetingId,ContactId,EventName,Capacity,Start,End,Password,Color,AllDay,MemberIds")] MeetingViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var meeting = new Meeting
                {
                    OrganizerId = user.Id,
                    Organizer = user.FullName,
                    ContactId = vm.ContactId,
                    EventName = vm.EventName,
                    Capacity = vm.Capacity,
                    Start = vm.Start,
                    End = vm.End,
                    Password = vm.Password,
                    Color = vm.Color,
                    AllDay = vm.AllDay
                };
                _context.Add(meeting);
                await _context.SaveChangesAsync();
                if (vm.MemberIds == null) vm.MemberIds = new System.Collections.Generic.List<int>();
                foreach (var memberId in vm.MemberIds)
                {
                    var meetingAttendees = new MeetingAttendees
                    {
                        MeetingId = meeting.MeetingId,
                        MemberId = memberId
                    };

                    _context.MeetingAttendees.Add(meetingAttendees);
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var members = await _context.Member.OrderBy(c => c.Username).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Username");
            return View(vm);
        }

        // GET: Meetings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .Include(m => m.MeetingAttendees)
                .FirstOrDefaultAsync(m => m.MeetingId == id);
            if (meeting == null)
            {
                return NotFound();
            }
            var members = await _context.Member.OrderBy(c => c.Username).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Username");
            var vm = new MeetingViewModel(meeting);
            return View(vm);
        }

        // POST: Meetings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MeetingId,ContactId,EventName,Capacity,Start,End,Password,Color,AllDay,MemberIds")] MeetingViewModel vm)
        {
            var meeting = await _context.Meeting
                .Include(mem => mem.MeetingAttendees)
                .ThenInclude(ma => ma.Member)
                .FirstOrDefaultAsync(m => m.MeetingId == id);
            if (id != meeting.MeetingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    meeting.ContactId = vm.ContactId;
                    meeting.EventName = vm.EventName;
                    meeting.Capacity = vm.Capacity;
                    meeting.Start = vm.Start;
                    meeting.End = vm.End;
                    meeting.Password = vm.Password;
                    meeting.Color = vm.Color;
                    meeting.AllDay = vm.AllDay;
                    _context.Update(meeting);
                    await _context.SaveChangesAsync();
                    if (vm.MemberIds == null) vm.MemberIds = new System.Collections.Generic.List<int>();
                    var oldMemberIds = meeting.MeetingAttendees.Select(ma => ma.Member.MemberId).ToList();
                    foreach (var memberId in vm.MemberIds)
                    {
                        if (!oldMemberIds.Contains(memberId))
                        {
                            var meetingAttendees = new MeetingAttendees
                            {
                                MeetingId = meeting.MeetingId,
                                MemberId = memberId
                            };

                            _context.MeetingAttendees.Add(meetingAttendees);
                            await _context.SaveChangesAsync();
                        }
                    }
                    foreach (var oldMemberId in oldMemberIds)
                    {
                        if (!vm.MemberIds.Contains(oldMemberId))
                        {
                            _context.MeetingAttendees.Remove(meeting.MeetingAttendees.FirstOrDefault(ma => ma.MemberId == oldMemberId && ma.MeetingId == meeting.MeetingId));
                            await _context.SaveChangesAsync();
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingExists(meeting.MeetingId))
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
            var members = await _context.Member.OrderBy(c => c.Username).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Username");

            return View(vm);
        }

        // GET: Meetings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .Include(mem => mem.MeetingAttendees)
                .ThenInclude(ma => ma.Member)
                .FirstOrDefaultAsync(m => m.MeetingId == id);
            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // POST: Meetings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meeting = await _context.Meeting.FindAsync(id);
            _context.Meeting.Remove(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MeetingExists(int id)
        {
            return _context.Meeting.Any(e => e.MeetingId == id);
        }
    }
}
