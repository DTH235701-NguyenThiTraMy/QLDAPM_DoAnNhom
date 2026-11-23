using Android17.Models;
using Android17.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Android17.Controllers;

[Authorize(Roles = "Admin")]
public class DonHangsController : Controller
{
    private readonly Android17Context _context;
    private readonly IConfiguration _config;

    public DonHangsController(Android17Context context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ===== Danh sách đơn (lọc theo trạng thái nếu có) =====
    public async Task<IActionResult> Index(int? trangThai)
    {
        var q = _context.DonHangs.AsQueryable();
        if (trangThai != null) q = q.Where(d => d.TrangThai == trangThai);

        ViewBag.trangThai = trangThai;
        var ds = await q.OrderByDescending(d => d.NgayTaoDon).ToListAsync();
        return View(ds);
    }

    // ===== Chi tiết một đơn =====
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var don = await _context.DonHangs
            .Include(d => d.DonHangChiTiets).ThenInclude(c => c.Sp)
            .FirstOrDefaultAsync(d => d.Dhid == id);
        if (don == null) return NotFound();
        return View(don);
    }

    // ===== Xác nhận đơn: bắt đầu đếm ngược giao hàng giả lập =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhan(int id)
    {
        var now = DateTime.Now;
        var giao = now.AddMinutes(_config.GetValue<int>("CuaHang:SoPhutGiaoHang", 4320));

        // Chỉ xác nhận được khi đơn còn "Chờ xác nhận"
        int dong = await _context.DonHangs
            .Where(d => d.Dhid == id && d.TrangThai == 0)
            .ExecuteUpdateAsync(s => s
                .SetProperty(d => d.TrangThai, 1)
                .SetProperty(d => d.TgXacNhan, (DateTime?)now)
                .SetProperty(d => d.NgayGiaoDuKien, (DateTime?)giao));

        if (dong > 0)
            TempData["ThongBao"] = "Đã xác nhận đơn #" + id + ". Dự kiến giao lúc " + giao.ToString("dd/MM/yyyy HH:mm") + ".";
        else
            TempData["Loi"] = "Không thể xác nhận: đơn #" + id + " không còn ở trạng thái chờ xác nhận.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // ===== Hủy đơn (hoàn kho) =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        bool ok = await DonHangHelper.HuyDonAsync(_context, id, choPhepDaXacNhan: true);
        if (ok)
            TempData["ThongBao"] = "Đã hủy đơn #" + id + " và hoàn lại số lượng vào kho.";
        else
            TempData["Loi"] = "Không thể hủy: đơn #" + id + " đã giao hoặc đã bị hủy.";

        return RedirectToAction(nameof(Details), new { id });
    }
}   