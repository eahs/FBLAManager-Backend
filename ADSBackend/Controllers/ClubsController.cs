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
            var members = await _context.Member.OrderBy(c => c.LastName).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Email");

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
                if (vm.MemberIds == null) vm.MemberIds = new System.Collections.Generic.List<int>();
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
            var members = await _context.Member.OrderBy(c => c.LastName).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Email");
            return View(vm);
        }

        // GET: Clubs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Club
                .Include(c => c.ClubMembers)
                .FirstOrDefaultAsync(c => c.ClubId == id);
            if (club == null)
            {
                return NotFound();
            }
            var members = await _context.Member.OrderBy(c => c.LastName).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Email");

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

                    club.Name = vm.Name;
                    club.Description = vm.Description;
                    club.Password = vm.Password;

                    _context.Update(club);
                    await _context.SaveChangesAsync();
                    if (vm.MemberIds == null) vm.MemberIds = new System.Collections.Generic.List<int>();
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
            var members = await _context.Member.OrderBy(c => c.LastName).ToListAsync();
            ViewBag.Members = new MultiSelectList(members, "MemberId", "Email");

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

        // GET: BoardPosts
        public async Task<IActionResult> BoardIndex(int? id)
        {
            ViewBag.ClubId = id;
            ViewBag.Club = await _context.Club.FirstOrDefaultAsync(c => c.ClubId == id);

            return View(await _context.BoardPost.Where(p => p.Club.ClubId == id).ToListAsync());
        }


        // GET: BoardPosts/Create
        public IActionResult BoardCreate(int id)
        {
            BoardPost post = new BoardPost
            {
                ClubId = id
            };
            return View(post);
        }

        // POST: BoardPosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoardCreate([Bind("PostId,Title,Director,PostTime,Message,ClubId")] BoardPost boardPost)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                boardPost.Director = user.FullName;
                boardPost.PostTime = DateTime.Now;
                _context.Add(boardPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(BoardIndex), new { id = boardPost.ClubId });
            }
            return View(boardPost);
        }

        // GET: BoardPosts/Edit/5
        public async Task<IActionResult> BoardEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardPost = await _context.BoardPost.FindAsync(id);
            if (boardPost == null)
            {
                return NotFound();
            }
            return View(boardPost);
        }

        // POST: BoardPosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoardEdit(int id, [Bind("PostId,Title,Message")] BoardPost boardPost)
        {
            if (id != boardPost.PostId)
            {
                return NotFound();
            }

            var _boardPost = await _context.BoardPost.FirstOrDefaultAsync(m => m.PostId == id);

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    _boardPost.Title = boardPost.Title;
                    _boardPost.Message = boardPost.Message;
                    _boardPost.EditedTime = DateTime.Now;
                    _context.Update(_boardPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoardPostExists(boardPost.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(BoardIndex), new { id = _boardPost.ClubId });
            }
            return RedirectToAction(nameof(BoardEdit), new { id = _boardPost.ClubId });
        }

        // GET: BoardPosts/Delete/5
        public async Task<IActionResult> BoardDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardPost = await _context.BoardPost
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (boardPost == null)
            {
                return NotFound();
            }

            return View(boardPost);
        }

        // POST: BoardPosts/Delete/5
        [HttpPost, ActionName("BoardDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoardDeleteConfirmed(int id)
        {
            var boardPost = await _context.BoardPost.FindAsync(id);
            _context.BoardPost.Remove(boardPost);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(BoardIndex), new { id = boardPost.ClubId });
        }

        private bool BoardPostExists(int id)
        {
            return _context.BoardPost.Any(e => e.PostId == id);
        }

    }
}
