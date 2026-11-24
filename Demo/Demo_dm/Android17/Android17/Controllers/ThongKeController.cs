using Android17.Models;
using Android17.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Android17.Controllers;

[Authorize(Roles = "Admin")]
public class ThongKeController : Controller
{
    private const int NguongSapHet = 10;   // tồn kho từ 10 trở xuống thì coi là sắp hết
    private readonly Android17Context _context;

    public ThongKeController(Android17Context context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
    {
        // Gõ ngược ngày thì tự đảo lại
        if (tuNgay.HasValue && denNgay.HasValue && tuNgay > denNgay)
            (tuNgay, denNgay) = (denNgay, tuNgay);

        var dh = _context.DonHangs.AsQueryable();
        var ct = _context.DonHangChiTiets.AsQueryable();

        if (tuNgay.HasValue)
        {
            var tu = tuNgay.Value.Date;
            dh = dh.Where(d => d.NgayTaoDon >= tu);
            ct = ct.Where(c => c.Dh.NgayTaoDon >= tu);
        }
        if (denNgay.HasValue)
        {
            var den = denNgay.Value.Date.AddDays(1);   // tính hết ngày "đến ngày"
            dh = dh.Where(d => d.NgayTaoDon < den);
            ct = ct.Where(c => c.Dh.NgayTaoDon < den);
        }

        var vm = new ThongKeVM { TuNgay = tuNgay, DenNgay = denNgay, NguongSapHet = NguongSapHet };

        // Doanh thu và giá trị đơn đang xử lý
        vm.DoanhThu = await dh.Where(d => d.TrangThai == 3)
            .SumAsync(d => (decimal?)d.ThanhTien) ?? 0;
        vm.DangXuLy = await dh.Where(d => d.TrangThai < 3)
            .SumAsync(d => (decimal?)d.ThanhTien) ?? 0;

        // Số đơn theo từng trạng thái
        var nhom = await dh.GroupBy(d => d.TrangThai)
            .Select(g => new { TrangThai = g.Key, Dem = g.Count() })
            .ToListAsync();
        foreach (var n in nhom)
        {
            if (n.TrangThai >= 0 && n.TrangThai <= 4)
                vm.SoDonTheoTrangThai[n.TrangThai] = n.Dem;
        }
        vm.TongDon = vm.SoDonTheoTrangThai.Sum();

        vm.SoKhachDatHang = await dh.Select(d => d.Ndid).Distinct().CountAsync();
        vm.SoSanPhamDangBan = await _context.SanPhams.CountAsync(s => s.DangBan);

        // Doanh thu theo ngày (10 ngày gần nhất có đơn hoàn thành)
        var theoNgay = await dh.Where(d => d.TrangThai == 3)
            .GroupBy(d => d.NgayTaoDon.Date)
            .Select(g => new DoanhThuNgayItem
            {
                Ngay = g.Key,
                SoDon = g.Count(),
                DoanhThu = g.Sum(x => x.ThanhTien)
            })
            .OrderByDescending(x => x.Ngay)
            .Take(10)
            .ToListAsync();
        theoNgay.Reverse();   // hiện từ ngày cũ đến ngày mới
        vm.DoanhThuNgay = theoNgay;

        // 5 sản phẩm bán chạy (không tính đơn đã hủy)
        vm.BanChay = await ct.Where(c => c.Dh.TrangThai != 4)
            .GroupBy(c => new { c.Spid, c.Sp.TenSp, c.Sp.Anh })
            .Select(g => new BanChayItem
            {
                Spid = g.Key.Spid,
                TenSp = g.Key.TenSp,
                Anh = g.Key.Anh,
                SoLuong = g.Sum(x => x.Sluong),
                DoanhThu = g.Sum(x => x.Sluong * x.GiaLucDat)
            })
            .OrderByDescending(x => x.SoLuong)
            .ThenByDescending(x => x.DoanhThu)
            .Take(5)
            .ToListAsync();

        // Sản phẩm đang bán nhưng sắp hết hàng (không phụ thuộc bộ lọc ngày)
        vm.SapHet = await _context.SanPhams
            .Where(s => s.DangBan && s.TonKho <= NguongSapHet)
            .OrderBy(s => s.TonKho)
            .Take(10)
            .ToListAsync();

        return View(vm);
    }
}