using Android17.Models;
using Android17.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Android17.Services;

namespace Android17.Controllers;

[Authorize]
public class DatHangController : Controller
{
    private readonly Android17Context _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;

    public DatHangController(Android17Context context, UserManager<IdentityUser> userManager, IConfiguration config)
    {
        _context = context;
        _userManager = userManager;
        _config = config;
    }

    // ===== Lịch sử đơn hàng của khách đang đăng nhập =====
    public async Task<IActionResult> Index()
    {
        var uid = _userManager.GetUserId(User);
        var ds = await _context.DonHangs
            .Where(d => d.Nd.IdentityUserId == uid)
            .OrderByDescending(d => d.NgayTaoDon)
            .ToListAsync();
        return View(ds);
    }

    // ===== Hiện form nhập thông tin nhận hàng =====
    public async Task<IActionResult> ThanhToan()
    {
        var nd = await LayNguoiDung();
        var (ds, loi) = await LayGioDeDat(nd.Ndid);
        if (loi != null)
        {
            TempData["Loi"] = loi;
            return RedirectToAction("Index", "GioHang");
        }

        NapTomTat(ds);
        // Điền sẵn từ hồ sơ khách (nếu có)
        var vm = new ThanhToanVM { NguoiNhan = nd.HoTen, Sdt = nd.Sdt ?? "", DiaChi = nd.Dchi ?? "" };
        return View(vm);
    }

    // ===== Bấm "Xác nhận đặt hàng" =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThanhToan(ThanhToanVM vm)
    {
        var nd = await LayNguoiDung();
        var (ds, loi) = await LayGioDeDat(nd.Ndid);
        if (loi != null)
        {
            TempData["Loi"] = loi;
            return RedirectToAction("Index", "GioHang");
        }

        string[] cachTra = { "COD", "Chuyển khoản" };
        if (!cachTra.Contains(vm.PhuongThuc))
            ModelState.AddModelError("PhuongThuc", "Phương thức thanh toán không hợp lệ.");

        if (!ModelState.IsValid)
        {
            NapTomTat(ds);
            return View(vm);
        }

        var (tong, phi, thanhTien) = TinhTien(ds);

        // Giao dịch: hoặc làm xong tất cả, hoặc không thay đổi gì.
        // Thoát hàm mà chưa Commit thì tự hoàn tác.
        await using var tran = await _context.Database.BeginTransactionAsync();

        // Trừ kho từng sản phẩm bằng MỘT câu lệnh duy nhất, chỉ trừ khi còn đủ hàng.
        // Hai khách cùng mua món cuối cùng thì chỉ một người trừ được.
        foreach (var g in ds)
        {
            int spid = g.Spid;
            int sl = g.Sluong;
            int dong = await _context.SanPhams
                .Where(s => s.Spid == spid && s.DangBan && s.TonKho >= sl)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.TonKho, x => x.TonKho - sl));
            if (dong == 0)
            {
                TempData["Loi"] = "Sản phẩm \"" + g.Sp.TenSp + "\" vừa hết hàng hoặc không đủ số lượng. Đơn chưa được tạo.";
                return RedirectToAction("Index", "GioHang");
            }
        }

        var don = new DonHang
        {
            Ndid = nd.Ndid,
            NgayTaoDon = DateTime.Now,
            NguoiNhan = vm.NguoiNhan.Trim(),
            SdtnguoiNhan = vm.Sdt.Trim(),
            Dchi = vm.DiaChi.Trim(),
            GhiChu = string.IsNullOrWhiteSpace(vm.GhiChu) ? null : vm.GhiChu.Trim(),
            PthucThanhToan = vm.PhuongThuc,
            TongTienSp = tong,
            PhiVanChuyen = phi,
            ThanhTien = thanhTien,
            TrangThai = 0   // Chờ xác nhận
        };
        foreach (var g in ds)
        {
            // Giá lấy từ database tại thời điểm đặt, khách không gửi giá lên
            don.DonHangChiTiets.Add(new DonHangChiTiet { Spid = g.Spid, Sluong = g.Sluong, GiaLucDat = g.Sp.Gia });
        }

        // Hồ sơ còn trống số điện thoại/địa chỉ thì lưu luôn để lần sau điền sẵn
        if (string.IsNullOrEmpty(nd.Sdt)) nd.Sdt = don.SdtnguoiNhan;
        if (string.IsNullOrEmpty(nd.Dchi)) nd.Dchi = don.Dchi;

        _context.DonHangs.Add(don);
        _context.GioHangs.RemoveRange(ds);   // chỉ xóa các dòng đã đặt
        await _context.SaveChangesAsync();
        await tran.CommitAsync();

        TempData["ThongBao"] = "Đặt hàng thành công! Mã đơn của bạn là #" + don.Dhid + ".";
        return RedirectToAction(nameof(HoaDon), new { id = don.Dhid });
    }

    // ===== Hóa đơn (khách chỉ xem được đơn của mình) =====
    public async Task<IActionResult> HoaDon(int? id)
    {
        if (id == null) return NotFound();
        var uid = _userManager.GetUserId(User);
        var don = await _context.DonHangs
            .Include(d => d.DonHangChiTiets).ThenInclude(c => c.Sp)
            .FirstOrDefaultAsync(d => d.Dhid == id && d.Nd.IdentityUserId == uid);
        if (don == null) return NotFound();
        return View(don);
    }

    // ===== Khách hủy đơn (chỉ khi đơn còn "Chờ xác nhận") =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var uid = _userManager.GetUserId(User);
        bool cuaToi = await _context.DonHangs.AnyAsync(d => d.Dhid == id && d.Nd.IdentityUserId == uid);
        if (!cuaToi) return NotFound();

        bool ok = await DonHangHelper.HuyDonAsync(_context, id, choPhepDaXacNhan: false);
        if (ok)
            TempData["ThongBao"] = "Đã hủy đơn #" + id + ". Số lượng hàng đã được hoàn lại kho.";
        else
            TempData["Loi"] = "Không thể hủy đơn #" + id + " vì đơn đã được xác nhận hoặc đã hủy trước đó.";

        return RedirectToAction(nameof(HoaDon), new { id });
    }
    // Lấy các dòng giỏ có thể đặt (còn bán, còn hàng). Có vấn đề thì trả về thông báo lỗi.
    private async Task<(List<GioHang> ds, string? loi)> LayGioDeDat(int ndid)
    {
        var ds = await _context.GioHangs
            .Include(g => g.Sp)
            .Where(g => g.Ndid == ndid && g.Sp.DangBan && g.Sp.TonKho > 0)
            .OrderBy(g => g.GioId)
            .ToListAsync();

        if (ds.Count == 0)
            return (ds, "Giỏ hàng chưa có sản phẩm nào có thể đặt.");

        var thieu = ds.FirstOrDefault(g => g.Sluong > g.Sp.TonKho);
        if (thieu != null)
            return (ds, "Kho chỉ còn " + thieu.Sp.TonKho + " sản phẩm \"" + thieu.Sp.TenSp + "\". Vui lòng kiểm tra lại giỏ hàng.");

        return (ds, null);
    }

    private (decimal tong, decimal phi, decimal thanhTien) TinhTien(List<GioHang> ds)
    {
        decimal tong = ds.Sum(g => g.Sluong * g.Sp.Gia);
        decimal mienPhiTu = _config.GetValue<decimal>("CuaHang:MienPhiTu", 500000);
        decimal phi = tong >= mienPhiTu ? 0 : _config.GetValue<decimal>("CuaHang:PhiVanChuyen", 30000);
        return (tong, phi, tong + phi);
    }

    private void NapTomTat(List<GioHang> ds)
    {
        var (tong, phi, thanhTien) = TinhTien(ds);
        ViewBag.gio = ds;
        ViewBag.tong = tong;
        ViewBag.phi = phi;
        ViewBag.thanhtien = thanhTien;
        ViewBag.mienphitu = _config.GetValue<decimal>("CuaHang:MienPhiTu", 500000);
    }

    // Lấy hồ sơ khách đang đăng nhập (chưa có thì tự tạo), giống GioHangController
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