using HatGiong701.Data;
using HatGiong701.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HatGiong701.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SeedsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeedsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Seeds
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Seeds.Include(s => s.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/Seeds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seed = await _context.Seeds
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.SeedId == id);
            if (seed == null)
            {
                return NotFound();
            }

            return View(seed);
        }

        // GET: Admin/Seeds/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Admin/Seeds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile? imageFile, [Bind("SeedId,Name,Price,StockQuantity,ImageUrl,Description,PlantingSeason,SuitableSpace,CareLevel,GerminationRate,HarvestDays,PlantingGuide,CategoryId")] Seed seed)
        {
            if (ModelState.IsValid)
            {
                // Xử lý lưu file ảnh vào wwwroot/images/seeds
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/seeds", fileName);
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    seed.ImageUrl = "/images/seeds/" + fileName;
                }
                _context.Add(seed);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", seed.CategoryId);
            return View(seed);
        }

        // GET: Admin/Seeds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seed = await _context.Seeds.FindAsync(id);
            if (seed == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", seed.CategoryId);
            return View(seed);
        }

        // POST: Admin/Seeds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SeedId,Name,Price,StockQuantity,ImageUrl,Description,PlantingSeason,SuitableSpace,CareLevel,GerminationRate,HarvestDays,PlantingGuide,CategoryId")] Seed seed)
        {
            if (id != seed.SeedId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seed);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeedExists(seed.SeedId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", seed.CategoryId);
            return View(seed);
        }

        // GET: Admin/Seeds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seed = await _context.Seeds
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.SeedId == id);
            if (seed == null)
            {
                return NotFound();
            }

            return View(seed);
        }

        // POST: Admin/Seeds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seed = await _context.Seeds.FindAsync(id);
            if (seed != null)
            {
                _context.Seeds.Remove(seed);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeedExists(int id)
        {
            return _context.Seeds.Any(e => e.SeedId == id);
        }
    }
}
