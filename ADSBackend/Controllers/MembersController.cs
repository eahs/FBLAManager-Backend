﻿using ADSBackend.Data;
using ADSBackend.Models;
using ADSBackend.Models.MemberViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Controllers
{
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
            var members = await _context.Member
                .Include(m => m.ClubMembers)
                .ThenInclude(cm => cm.Club)
                .Include(meet => meet.MeetingAttendees)
                .ThenInclude(ma => ma.Meeting)
                .ToListAsync();

            return View(members);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.Include(m => m.ClubMembers).ThenInclude(mc => mc.Club)
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

            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Email,Phone,Password,ClubIds")] MemberViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var member = new Member
                {
                    Username = vm.Username,
                    Email = vm.Email,
                    Phone = vm.Phone,
                    Password = vm.Password
                };
                _context.Member.Add(member);
                await _context.SaveChangesAsync();

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
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var clubs = await _context.Club.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Clubs = new MultiSelectList(clubs, "ClubId", "Name");

            return View(vm);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,Username,Email,Phone")] Member member)
        {
            if (id != member.MemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var _member = await _context.Member.FindAsync(id);

                    _member.Username = member.Username;
                    _member.Email = member.Email;
                    _member.Phone = member.Phone;

                    _context.Update(_member);
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
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
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
