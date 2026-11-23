using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Android17.Models;

[Authorize(Roles = "Admin")]
public class LoaiHangsController : Controller
{
    private readonly Android17Context _context;

    public LoaiHangsController(Android17Context context)
    {
        _context = context;
    }

    // GET: LOAIHANGS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.LoaiHangs.ToListAsync());
    }

    // GET: LOAIHANGS/Details/5
    public async Task<IActionResult> Details(int? loaiid)
    {
        if (loaiid == null)
        {
            return NotFound();
        }

        var loaihang = await _context.LoaiHangs
            .FirstOrDefaultAsync(m => m.LoaiId == loaiid);
        if (loaihang == null)
        {
            return NotFound();
        }

        return View(loaihang);
    }

    // GET: LOAIHANGS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: LOAIHANGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LoaiId,TenLoai,MoTa,SanPhams")] LoaiHang loaihang)
    {
        if (ModelState.IsValid)
        {
            _context.Add(loaihang);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(loaihang);
    }

    // GET: LOAIHANGS/Edit/5
    public async Task<IActionResult> Edit(int? loaiid)
    {
        if (loaiid == null)
        {
            return NotFound();
        }

        var loaihang = await _context.LoaiHangs.FindAsync(loaiid);
        if (loaihang == null)
        {
            return NotFound();
        }
        return View(loaihang);
    }

    // POST: LOAIHANGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? loaiid, [Bind("LoaiId,TenLoai,MoTa,SanPhams")] LoaiHang loaihang)
    {
        if (loaiid != loaihang.LoaiId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(loaihang);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoaiHangExists(loaihang.LoaiId))
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
        return View(loaihang);
    }

    // GET: LOAIHANGS/Delete/5
    public async Task<IActionResult> Delete(int? loaiid)
    {
        if (loaiid == null)
        {
            return NotFound();
        }

        var loaihang = await _context.LoaiHangs
            .FirstOrDefaultAsync(m => m.LoaiId == loaiid);
        if (loaihang == null)
        {
            return NotFound();
        }

        return View(loaihang);
    }

    // POST: LOAIHANGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? loaiid)
    {
        var loaihang = await _context.LoaiHangs.FindAsync(loaiid);
        if (loaihang != null)
        {
            _context.LoaiHangs.Remove(loaihang);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool LoaiHangExists(int? loaiid)
    {
        return _context.LoaiHangs.Any(e => e.LoaiId == loaiid);
    }
}
