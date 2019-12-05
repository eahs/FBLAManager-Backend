using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADSBackend.Data;
using ADSBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace ADSBackend.Controllers
{
    [Authorize]
    public class OfficersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfficersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Officers
        public async Task<IActionResult> Index(string search)
        {
            ViewData["Search"] = search;
            var officers = await _context.Officer.ToListAsync();
            if (!String.IsNullOrEmpty(search))
            {
                officers = await _context.Officer
                    .Where(s => s.Name.Contains(search))
                    .ToListAsync();
            }
            return View(officers);
        }

        // GET: Officers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officer = await _context.Officer
                .FirstOrDefaultAsync(m => m.OfficerId == id);
            if (officer == null)
            {
                return NotFound();
            }

            return View(officer);
        }

        // GET: Officers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Officers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OfficerId,Name,Position,Image,WebsiteLink,Level,Order")] Officer officer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(officer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(officer);
        }

        // GET: Officers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officer = await _context.Officer.FindAsync(id);
            if (officer == null)
            {
                return NotFound();
            }
            return View(officer);
        }

        // POST: Officers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OfficerId,Name,Position,Image,WebsiteLink,Level,Order")] Officer officer)
        {
            if (id != officer.OfficerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(officer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfficerExists(officer.OfficerId))
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
            return View(officer);
        }

        // GET: Officers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officer = await _context.Officer
                .FirstOrDefaultAsync(m => m.OfficerId == id);
            if (officer == null)
            {
                return NotFound();
            }

            return View(officer);
        }

        // POST: Officers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var officer = await _context.Officer.FindAsync(id);
            _context.Officer.Remove(officer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OfficerExists(int id)
        {
            return _context.Officer.Any(e => e.OfficerId == id);
        }
    }
}
