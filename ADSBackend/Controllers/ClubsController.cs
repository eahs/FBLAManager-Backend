using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADSBackend.Data;
using ADSBackend.Models;
using Microsoft.AspNetCore.Identity;
using ADSBackend.Models.Identity;
using ADSBackend.Models.ClubViewModels;

namespace ADSBackend.Controllers
{
    public class ClubsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClubsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Clubs
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var clubs = await _context.Club
                .Include(c => c.ClubMembers)
                .ThenInclude(cm => cm.Member)
                .ToListAsync();
            if (await _userManager.IsInRoleAsync(user,"Admin"))
            {
                return View(clubs);
            }
            else
            {
                return View(clubs.Where(m => m.CreatorId == user.Id));
            }
        }

        // GET: Clubs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Club
                .Include(c => c.ClubMembers)
                .ThenInclude(cm => cm.Member)
                .FirstOrDefaultAsync(m => m.ClubId == id);
            if (club == null)
            {
                return NotFound();
            }

            return View(club);
        }

        // GET: Clubs/Create
        public async Task<IActionResult> Create()
        {
            var members = await _context.Member.OrderBy(c => c.Username).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Username");

            return View();
        }

        // POST: Clubs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClubId,Name,Description,Password,MemberIds")] ClubViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var club = new Club
                {
                    CreatorId = user.Id,
                    Creator = user.FullName,
                    Name = vm.Name,
                    Description = vm.Description,
                    Password = vm.Password
                };

                _context.Club.Add(club);
                await _context.SaveChangesAsync();

                foreach (var memberId in vm.MemberIds)
                {
                    var clubMember = new ClubMember
                    {
                        ClubId = club.ClubId,
                        MemberId = memberId
                    };

                    _context.ClubMember.Add(clubMember);
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var members = await _context.Member.OrderBy(c => c.Username).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Username");
            return View(vm);
        }

        // GET: Clubs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Club.Include(c => c.ClubMembers).FirstOrDefaultAsync(c => c.ClubId == id);
            if (club == null)
            {
                return NotFound();
            }
            var members = await _context.Member.OrderBy(c => c.Username).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Username");

            var vm = new ClubViewModel(club);
            return View(vm);
        }

        // POST: Clubs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClubId,Name,Description,Password,MemberIds")] ClubViewModel vm)
        {
            var club = await _context.Club
                .Include(c => c.ClubMembers)
                .ThenInclude(cm => cm.Member)
                .FirstOrDefaultAsync(c => c.ClubId == id);
            if (id != club.ClubId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    club.Name = club.Name;
                    club.Description = club.Description;
                    club.Password = club.Password;

                    _context.Update(club);
                    await _context.SaveChangesAsync();

                    var oldMemberIds = club.ClubMembers.Select(cm => cm.Member.MemberId).ToList();
                    foreach (var memberId in vm.MemberIds)
                    {
                        if (!oldMemberIds.Contains(memberId))
                        {
                            var clubMember = new ClubMember
                            {
                                ClubId = club.ClubId,
                                MemberId = memberId
                            };

                            _context.ClubMember.Add(clubMember);
                            await _context.SaveChangesAsync();
                        }
                    }
                    foreach (var oldMemberId in oldMemberIds)
                    {
                        if (!vm.MemberIds.Contains(oldMemberId))
                        {
                            _context.ClubMember.Remove(club.ClubMembers.FirstOrDefault(cm => cm.MemberId == oldMemberId && cm.ClubId ==club.ClubId));
                            await _context.SaveChangesAsync();
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClubExists(club.ClubId))
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

        // GET: Clubs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Club
               .Include(c => c.ClubMembers)
               .ThenInclude(cm => cm.Member)
               .FirstOrDefaultAsync(m => m.ClubId == id);
            if (club == null)
            {
                return NotFound();
            }

            return View(club);
        }

        // POST: Clubs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var club = await _context.Club.FindAsync(id);
            _context.Club.Remove(club);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClubExists(int id)
        {
            return _context.Club.Any(e => e.ClubId == id);
        }
    }
}
