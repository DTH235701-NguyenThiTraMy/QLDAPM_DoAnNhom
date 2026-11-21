using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Android17.Models;

public class NhanHangsController : Controller
{
    private readonly Android17Context _context;

    public NhanHangsController(Android17Context context)
    {
        _context = context;
    }

    // GET: NHANHANGS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.NhanHangs.ToListAsync());
    }

    // GET: NHANHANGS/Details/5
    public async Task<IActionResult> Details(int? nhanhangid)
    {
        if (nhanhangid == null)
        {
            return NotFound();
        }

        var nhanhang = await _context.NhanHangs
            .FirstOrDefaultAsync(m => m.NhanHangId == nhanhangid);
        if (nhanhang == null)
        {
            return NotFound();
        }

        return View(nhanhang);
    }

    // GET: NHANHANGS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: NHANHANGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NhanHangId,TenNhanHang,SanPhams")] NhanHang nhanhang)
    {
        if (ModelState.IsValid)
        {
            _context.Add(nhanhang);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(nhanhang);
    }

    // GET: NHANHANGS/Edit/5
    public async Task<IActionResult> Edit(int? nhanhangid)
    {
        if (nhanhangid == null)
        {
            return NotFound();
        }

        var nhanhang = await _context.NhanHangs.FindAsync(nhanhangid);
        if (nhanhang == null)
        {
            return NotFound();
        }
        return View(nhanhang);
    }

    // POST: NHANHANGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? nhanhangid, [Bind("NhanHangId,TenNhanHang,SanPhams")] NhanHang nhanhang)
    {
        if (nhanhangid != nhanhang.NhanHangId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(nhanhang);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhanHangExists(nhanhang.NhanHangId))
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
        return View(nhanhang);
    }

    // GET: NHANHANGS/Delete/5
    public async Task<IActionResult> Delete(int? nhanhangid)
    {
        if (nhanhangid == null)
        {
            return NotFound();
        }

        var nhanhang = await _context.NhanHangs
            .FirstOrDefaultAsync(m => m.NhanHangId == nhanhangid);
        if (nhanhang == null)
        {
            return NotFound();
        }

        return View(nhanhang);
    }

    // POST: NHANHANGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? nhanhangid)
    {
        var nhanhang = await _context.NhanHangs.FindAsync(nhanhangid);
        if (nhanhang != null)
        {
            _context.NhanHangs.Remove(nhanhang);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool NhanHangExists(int? nhanhangid)
    {
        return _context.NhanHangs.Any(e => e.NhanHangId == nhanhangid);
    }
}
