using Android17.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Android17.Controllers;

[Authorize(Roles = "Admin")]
public class LoaiHangsController : Controller
{
    private readonly Android17Context _context;

    public LoaiHangsController(Android17Context context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var ds = await _context.LoaiHangs
            .Include(l => l.SanPhams)
            .OrderBy(l => l.LoaiId)
            .ToListAsync();
        return View(ds);
    }

    public IActionResult Create() => View(new LoaiHang());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenLoai,MoTa")] LoaiHang loai)
    {
        await KiemTra(loai, 0);
        if (!ModelState.IsValid) return View(loai);

        _context.Add(loai);
        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã thêm loại hàng \"" + loai.TenLoai + "\".";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var loai = await _context.LoaiHangs.FindAsync(id);
        if (loai == null) return NotFound();
        return View(loai);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("LoaiId,TenLoai,MoTa")] LoaiHang loai)
    {
        if (id != loai.LoaiId) return NotFound();
        var goc = await _context.LoaiHangs.FindAsync(id);
        if (goc == null) return NotFound();

        await KiemTra(loai, id);
        if (!ModelState.IsValid) return View(loai);

        goc.TenLoai = loai.TenLoai;
        goc.MoTa = loai.MoTa;
        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã cập nhật loại hàng \"" + goc.TenLoai + "\".";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var loai = await _context.LoaiHangs
            .Include(l => l.SanPhams)
            .FirstOrDefaultAsync(l => l.LoaiId == id);
        if (loai == null) return NotFound();
        return View(loai);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var loai = await _context.LoaiHangs
            .Include(l => l.SanPhams)
            .FirstOrDefaultAsync(l => l.LoaiId == id);
        if (loai == null) return RedirectToAction(nameof(Index));

        if (loai.SanPhams.Count > 0)
        {
            TempData["Loi"] = "Không thể xóa loại hàng \"" + loai.TenLoai + "\" vì còn " + loai.SanPhams.Count + " sản phẩm thuộc loại này.";
            return RedirectToAction(nameof(Index));
        }

        _context.LoaiHangs.Remove(loai);
        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã xóa loại hàng \"" + loai.TenLoai + "\".";
        return RedirectToAction(nameof(Index));
    }

    // Kiểm tra dữ liệu nhập, báo lỗi bằng tiếng Việt
    private async Task KiemTra(LoaiHang l, int idHienTai)
    {
        // Bỏ thông báo lỗi mặc định (tiếng Anh) để dùng thông báo của mình
        ModelState.Remove(nameof(LoaiHang.TenLoai));
        ModelState.Remove(nameof(LoaiHang.MoTa));

        var ten = (l.TenLoai ?? "").Trim();
        l.TenLoai = ten;
        l.MoTa = string.IsNullOrWhiteSpace(l.MoTa) ? null : l.MoTa.Trim();

        if (ten.Length == 0)
            ModelState.AddModelError(nameof(LoaiHang.TenLoai), "Vui lòng nhập tên loại hàng.");
        else if (ten.Length > 100)
            ModelState.AddModelError(nameof(LoaiHang.TenLoai), "Tên loại hàng tối đa 100 ký tự.");
        else if (await _context.LoaiHangs.AnyAsync(x => x.TenLoai == ten && x.LoaiId != idHienTai))
            ModelState.AddModelError(nameof(LoaiHang.TenLoai), "Đã có loại hàng tên \"" + ten + "\".");

        if (l.MoTa != null && l.MoTa.Length > 255)
            ModelState.AddModelError(nameof(LoaiHang.MoTa), "Mô tả tối đa 255 ký tự.");
    }
}