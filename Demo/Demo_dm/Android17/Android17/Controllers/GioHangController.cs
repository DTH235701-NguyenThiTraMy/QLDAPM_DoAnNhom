using Android17.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Android17.Controllers;

[Authorize]
public class GioHangController : Controller
{
    private readonly Android17Context _context;
    private readonly UserManager<IdentityUser> _userManager;

    public GioHangController(Android17Context context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ===== Xem giỏ hàng =====
    public async Task<IActionResult> Index()
    {
        var nd = await LayNguoiDung();
        var ds = await _context.GioHangs
            .Include(g => g.Sp)
            .Where(g => g.Ndid == nd.Ndid)
            .OrderByDescending(g => g.GioId)
            .ToListAsync();

        // Nếu kho đã giảm so với số lượng trong giỏ thì hạ số lượng xuống bằng tồn kho
        bool doi = false;
        foreach (var g in ds)
        {
            if (g.Sp.DangBan && g.Sp.TonKho > 0 && g.Sluong > g.Sp.TonKho)
            {
                g.Sluong = g.Sp.TonKho;
                doi = true;
            }
        }
        if (doi)
        {
            await _context.SaveChangesAsync();
            ViewBag.canhbao = "Một số sản phẩm vượt quá tồn kho nên số lượng đã được điều chỉnh.";
        }

        return View(ds);
    }

    // ===== Thêm vào giỏ =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(int spid, int sluong = 1)
    {
        if (sluong < 1) sluong = 1;

        var sp = await _context.SanPhams.FindAsync(spid);
        if (sp == null || !sp.DangBan)
        {
            TempData["Loi"] = "Sản phẩm không tồn tại hoặc đã ngừng bán.";
            return RedirectToAction(nameof(Index));
        }
        if (sp.TonKho <= 0)
        {
            TempData["Loi"] = "Sản phẩm \"" + sp.TenSp + "\" đã hết hàng.";
            return RedirectToAction(nameof(Index));
        }

        var nd = await LayNguoiDung();
        var gio = await _context.GioHangs.FirstOrDefaultAsync(g => g.Ndid == nd.Ndid && g.Spid == spid);

        int moi = (gio?.Sluong ?? 0) + sluong;
        if (moi > sp.TonKho)
        {
            moi = sp.TonKho;
            TempData["Loi"] = "Kho chỉ còn " + sp.TonKho + " sản phẩm \"" + sp.TenSp + "\", đã đặt số lượng tối đa vào giỏ.";
        }
        else
        {
            TempData["ThongBao"] = "Đã thêm \"" + sp.TenSp + "\" vào giỏ hàng.";
        }

        if (gio == null)
            _context.GioHangs.Add(new GioHang { Ndid = nd.Ndid, Spid = spid, Sluong = moi });
        else
            gio.Sluong = moi;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ===== Sửa số lượng =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhat(int gioId, int sluong)
    {
        var nd = await LayNguoiDung();
        // Điều kiện Ndid == nd.Ndid: khách chỉ sửa được giỏ của chính mình
        var gio = await _context.GioHangs.Include(g => g.Sp)
            .FirstOrDefaultAsync(g => g.GioId == gioId && g.Ndid == nd.Ndid);
        if (gio == null) return RedirectToAction(nameof(Index));

        if (sluong <= 0)
        {
            _context.GioHangs.Remove(gio);
            TempData["ThongBao"] = "Đã xóa \"" + gio.Sp.TenSp + "\" khỏi giỏ hàng.";
        }
        else if (!gio.Sp.DangBan || gio.Sp.TonKho <= 0)
        {
            TempData["Loi"] = "Sản phẩm \"" + gio.Sp.TenSp + "\" đã hết hàng hoặc ngừng bán, hãy xóa khỏi giỏ.";
        }
        else if (sluong > gio.Sp.TonKho)
        {
            gio.Sluong = gio.Sp.TonKho;
            TempData["Loi"] = "Kho chỉ còn " + gio.Sp.TonKho + " sản phẩm \"" + gio.Sp.TenSp + "\".";
        }
        else
        {
            gio.Sluong = sluong;
            TempData["ThongBao"] = "Đã cập nhật số lượng.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ===== Xóa khỏi giỏ =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int gioId)
    {
        var nd = await LayNguoiDung();
        var gio = await _context.GioHangs.Include(g => g.Sp)
            .FirstOrDefaultAsync(g => g.GioId == gioId && g.Ndid == nd.Ndid);
        if (gio != null)
        {
            _context.GioHangs.Remove(gio);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã xóa \"" + gio.Sp.TenSp + "\" khỏi giỏ hàng.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ===== Lấy hồ sơ khách đang đăng nhập (chưa có thì tự tạo) =====
    private async Task<NguoiDung> LayNguoiDung()
    {
        var uid = _userManager.GetUserId(User)!;
        var nd = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.IdentityUserId == uid);
        if (nd == null)
        {
            var ten = (_userManager.GetUserName(User) ?? "Khách hàng").Split('@')[0];
            if (ten.Length > 100) ten = ten.Substring(0, 100);

            nd = new NguoiDung { IdentityUserId = uid, HoTen = ten, TgTaoTk = DateTime.Now };
            _context.NguoiDungs.Add(nd);
            await _context.SaveChangesAsync();
        }
        return nd;
    }
}