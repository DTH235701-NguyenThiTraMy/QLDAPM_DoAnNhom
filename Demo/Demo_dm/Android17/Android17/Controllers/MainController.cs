using Android17.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Android17.Controllers;

public class MainController : Controller
{
    private readonly Android17Context _context;

    public MainController(Android17Context context)
    {
        _context = context;
    }

    // Chạy trước MỌI action: nạp dữ liệu dùng chung cho _LayoutMain
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.loaihang = _context.LoaiHangs.OrderBy(l => l.LoaiId).ToList();
        ViewBag.nhanhang = _context.NhanHangs.OrderBy(n => n.TenNhanHang).ToList();
        ViewBag.spmoi = _context.SanPhams
            .Where(s => s.DangBan)
            .OrderByDescending(s => s.Spid)
            .Take(5)
            .ToList();
        base.OnActionExecuting(context);
    }

    // Trang chủ: tất cả sản phẩm đang bán + danh sách nổi bật
    public IActionResult Index()
    {
        var ds = _context.SanPhams
            .Include(s => s.Loai)
            .Include(s => s.NhanHang)
            .Where(s => s.DangBan)
            .OrderByDescending(s => s.Spid)
            .ToList();

        ViewBag.noibat = ds.Where(s => s.NoiBat).ToList();
        return View(ds);
    }

    // Chi tiết một sản phẩm
    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var sp = _context.SanPhams
            .Include(s => s.Loai)
            .Include(s => s.NhanHang)
            .FirstOrDefault(s => s.Spid == id && s.DangBan);
        if (sp == null) return NotFound();

        // Sản phẩm khác cùng loại
        ViewBag.spkhac = _context.SanPhams
            .Where(s => s.LoaiId == sp.LoaiId && s.Spid != sp.Spid && s.DangBan)
            .Take(5)
            .ToList();

        return View(sp);
    }

    // Danh sách sản phẩm theo loại hàng
    public IActionResult Loai(int? id)
    {
        if (id == null) return NotFound();
        var loai = _context.LoaiHangs.Find(id);
        if (loai == null) return NotFound();

        var ds = _context.SanPhams
            .Include(s => s.Loai)
            .Include(s => s.NhanHang)
            .Where(s => s.LoaiId == id && s.DangBan)
            .OrderByDescending(s => s.Spid)
            .ToList();

        ViewBag.tieude = "Loại hàng: " + loai.TenLoai;
        return View("DanhSach", ds);
    }

    // Danh sách sản phẩm theo nhãn hàng
    public IActionResult Hang(int? id)
    {
        if (id == null) return NotFound();
        var nhan = _context.NhanHangs.Find(id);
        if (nhan == null) return NotFound();

        var ds = _context.SanPhams
            .Include(s => s.Loai)
            .Include(s => s.NhanHang)
            .Where(s => s.NhanHangId == id && s.DangBan)
            .OrderByDescending(s => s.Spid)
            .ToList();

        ViewBag.tieude = "Nhãn hàng: " + nhan.TenNhanHang;
        return View("DanhSach", ds);
    }

    // Tìm kiếm theo từ khóa
    public IActionResult Tim(string? tukhoa)
    {
        var ds = _context.SanPhams
            .Include(s => s.Loai)
            .Include(s => s.NhanHang)
            .Where(s => s.DangBan);

        if (!string.IsNullOrWhiteSpace(tukhoa))
        {
            tukhoa = tukhoa.Trim();
            ds = ds.Where(s => s.TenSp.Contains(tukhoa) ||
                               (s.MoTaNgan != null && s.MoTaNgan.Contains(tukhoa)));
        }

        ViewBag.tukhoa = tukhoa;
        ViewBag.tieude = "Kết quả tìm kiếm: " + tukhoa;
        return View("DanhSach", ds.OrderByDescending(s => s.Spid).ToList());
    }
}