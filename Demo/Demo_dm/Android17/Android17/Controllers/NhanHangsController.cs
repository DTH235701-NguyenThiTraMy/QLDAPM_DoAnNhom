using Android17.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Android17.Controllers;

[Authorize(Roles = "Admin")]
public class NhanHangsController : Controller
{
    private readonly Android17Context _context;

    public NhanHangsController(Android17Context context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var ds = await _context.NhanHangs
            .Include(n => n.SanPhams)
            .OrderBy(n => n.TenNhanHang)
            .ToListAsync();
        return View(ds);
    }

    public IActionResult Create() => View(new NhanHang());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenNhanHang")] NhanHang nhan)
    {
        await KiemTra(nhan, 0);
        if (!ModelState.IsValid) return View(nhan);

        _context.Add(nhan);
        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã thêm nhãn hàng \"" + nhan.TenNhanHang + "\".";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var nhan = await _context.NhanHangs.FindAsync(id);
        if (nhan == null) return NotFound();
        return View(nhan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("NhanHangId,TenNhanHang")] NhanHang nhan)
    {
        if (id != nhan.NhanHangId) return NotFound();
        var goc = await _context.NhanHangs.FindAsync(id);
        if (goc == null) return NotFound();

        await KiemTra(nhan, id);
        if (!ModelState.IsValid) return View(nhan);

        goc.TenNhanHang = nhan.TenNhanHang;
        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã cập nhật nhãn hàng \"" + goc.TenNhanHang + "\".";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var nhan = await _context.NhanHangs
            .Include(n => n.SanPhams)
            .FirstOrDefaultAsync(n => n.NhanHangId == id);
        if (nhan == null) return NotFound();
        return View(nhan);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var nhan = await _context.NhanHangs
            .Include(n => n.SanPhams)
            .FirstOrDefaultAsync(n => n.NhanHangId == id);
        if (nhan == null) return RedirectToAction(nameof(Index));

        if (nhan.SanPhams.Count > 0)
        {
            TempData["Loi"] = "Không thể xóa nhãn hàng \"" + nhan.TenNhanHang + "\" vì còn " + nhan.SanPhams.Count + " sản phẩm thuộc nhãn này.";
            return RedirectToAction(nameof(Index));
        }

        _context.NhanHangs.Remove(nhan);
        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã xóa nhãn hàng \"" + nhan.TenNhanHang + "\".";
        return RedirectToAction(nameof(Index));
    }

    private async Task KiemTra(NhanHang n, int idHienTai)
    {
        ModelState.Remove(nameof(NhanHang.TenNhanHang));

        var ten = (n.TenNhanHang ?? "").Trim();
        n.TenNhanHang = ten;

        if (ten.Length == 0)
            ModelState.AddModelError(nameof(NhanHang.TenNhanHang), "Vui lòng nhập tên nhãn hàng.");
        else if (ten.Length > 100)
            ModelState.AddModelError(nameof(NhanHang.TenNhanHang), "Tên nhãn hàng tối đa 100 ký tự.");
        else if (await _context.NhanHangs.AnyAsync(x => x.TenNhanHang == ten && x.NhanHangId != idHienTai))
            ModelState.AddModelError(nameof(NhanHang.TenNhanHang), "Đã có nhãn hàng tên \"" + ten + "\".");
    }
}