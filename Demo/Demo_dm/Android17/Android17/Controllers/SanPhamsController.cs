using Android17.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Android17.Controllers;

[Authorize(Roles = "Admin")]
public class SanPhamsController : Controller
{
    private readonly Android17Context _context;
    private readonly IWebHostEnvironment _env;

    public SanPhamsController(Android17Context context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ===== Danh sách =====
    public async Task<IActionResult> Index()
    {
        var ds = await _context.SanPhams
            .Include(s => s.Loai)
            .Include(s => s.NhanHang)
            .OrderByDescending(s => s.Spid)
            .ToListAsync();
        return View(ds);
    }

    // ===== Chi tiết =====
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var sp = await _context.SanPhams
            .Include(s => s.Loai)
            .Include(s => s.NhanHang)
            .FirstOrDefaultAsync(s => s.Spid == id);
        if (sp == null) return NotFound();
        return View(sp);
    }

    // ===== Thêm =====
    public IActionResult Create()
    {
        NapDanhSach();
        return View(new SanPham { DangBan = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("TenSp,LoaiId,NhanHangId,Gia,GiaGoc,TonKho,MoTaNgan,ThongSo,NoiBat,DangBan")] SanPham sp,
        IFormFile? anhFile)
    {
        // Form không gửi 3 thuộc tính này nên bỏ lỗi "bắt buộc" của chúng
        ModelState.Remove("Loai");
        ModelState.Remove("NhanHang");
        ModelState.Remove("Anh");

        KiemTraSanPham(sp);
        if (anhFile == null || anhFile.Length == 0)
            ModelState.AddModelError("Anh", "Vui lòng chọn ảnh sản phẩm.");
        else if (!AnhHopLe(anhFile, out var loi))
            ModelState.AddModelError("Anh", loi);

        if (ModelState.IsValid)
        {
            sp.Anh = await LuuAnh(anhFile!);
            _context.Add(sp);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        NapDanhSach(sp.LoaiId, sp.NhanHangId);
        return View(sp);
    }

    // ===== Sửa =====
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var sp = await _context.SanPhams.FindAsync(id);
        if (sp == null) return NotFound();

        NapDanhSach(sp.LoaiId, sp.NhanHangId);
        return View(sp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Spid,TenSp,LoaiId,NhanHangId,Gia,GiaGoc,TonKho,MoTaNgan,ThongSo,NoiBat,DangBan")] SanPham sp,
        IFormFile? anhFile)
    {
        if (id != sp.Spid) return NotFound();

        var goc = await _context.SanPhams.FindAsync(id);
        if (goc == null) return NotFound();

        ModelState.Remove("Loai");
        ModelState.Remove("NhanHang");
        ModelState.Remove("Anh");

        KiemTraSanPham(sp);
        bool coAnhMoi = anhFile != null && anhFile.Length > 0;
        if (coAnhMoi && !AnhHopLe(anhFile!, out var loi))
            ModelState.AddModelError("Anh", loi);

        if (ModelState.IsValid)
        {
            goc.TenSp = sp.TenSp;
            goc.LoaiId = sp.LoaiId;
            goc.NhanHangId = sp.NhanHangId;
            goc.Gia = sp.Gia;
            goc.GiaGoc = sp.GiaGoc;
            goc.TonKho = sp.TonKho;
            goc.MoTaNgan = sp.MoTaNgan;
            goc.ThongSo = sp.ThongSo;
            goc.NoiBat = sp.NoiBat;
            goc.DangBan = sp.DangBan;
            if (coAnhMoi) goc.Anh = await LuuAnh(anhFile!);   // không chọn ảnh mới thì giữ ảnh cũ

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        sp.Anh = goc.Anh;   // để form hiện lại ảnh hiện tại
        NapDanhSach(sp.LoaiId, sp.NhanHangId);
        return View(sp);
    }

    // ===== Xóa =====
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var sp = await _context.SanPhams
            .Include(s => s.Loai)
            .Include(s => s.NhanHang)
            .FirstOrDefaultAsync(s => s.Spid == id);
        if (sp == null) return NotFound();
        return View(sp);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sp = await _context.SanPhams
            .Include(s => s.DonHangChiTiets)
            .Include(s => s.GioHangs)
            .FirstOrDefaultAsync(s => s.Spid == id);
        if (sp == null) return RedirectToAction(nameof(Index));

        if (sp.DonHangChiTiets.Any())
        {
            // Đã nằm trong đơn hàng: không xóa được, chỉ ẩn đi
            sp.DangBan = false;
            TempData["ThongBao"] = "Sản phẩm \"" + sp.TenSp + "\" đã có trong đơn hàng nên không thể xóa. Hệ thống đã chuyển sang trạng thái ẩn (ngừng bán).";
        }
        else
        {
            _context.GioHangs.RemoveRange(sp.GioHangs);   // gỡ khỏi giỏ hàng của khách
            _context.SanPhams.Remove(sp);
            TempData["ThongBao"] = "Đã xóa sản phẩm \"" + sp.TenSp + "\".";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ===== Hàm phụ =====
    private void NapDanhSach(int? loaiId = null, int? nhanHangId = null)
    {
        ViewBag.dsLoai = new SelectList(_context.LoaiHangs.OrderBy(l => l.TenLoai), "LoaiId", "TenLoai", loaiId);
        ViewBag.dsHang = new SelectList(_context.NhanHangs.OrderBy(n => n.TenNhanHang), "NhanHangId", "TenNhanHang", nhanHangId);
    }

    private void KiemTraSanPham(SanPham sp)
    {
        if (sp.Gia <= 0)
            ModelState.AddModelError("Gia", "Giá bán phải lớn hơn 0.");
        if (sp.TonKho < 0)
            ModelState.AddModelError("TonKho", "Tồn kho không được âm.");
        if (sp.GiaGoc.HasValue && sp.GiaGoc.Value < sp.Gia)
            ModelState.AddModelError("GiaGoc", "Giá gốc phải lớn hơn hoặc bằng giá bán (hoặc để trống).");
    }

    private static bool AnhHopLe(IFormFile file, out string loi)
    {
        var duoi = Path.GetExtension(file.FileName).ToLowerInvariant();
        string[] chophep = { ".jpg", ".jpeg", ".png", ".webp" };
        if (!chophep.Contains(duoi))
        {
            loi = "Chỉ nhận ảnh .jpg, .jpeg, .png, .webp.";
            return false;
        }
        if (file.Length > 2 * 1024 * 1024)
        {
            loi = "Ảnh không được lớn hơn 2MB.";
            return false;
        }
        loi = "";
        return true;
    }

    private async Task<string> LuuAnh(IFormFile file)
    {
        var thuMuc = Path.Combine(_env.WebRootPath, "images");
        Directory.CreateDirectory(thuMuc);

        // Đặt tên ngẫu nhiên để không trùng và không ghi đè ảnh có sẵn
        var ten = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        using var stream = new FileStream(Path.Combine(thuMuc, ten), FileMode.Create);
        await file.CopyToAsync(stream);
        return ten;
    }
}