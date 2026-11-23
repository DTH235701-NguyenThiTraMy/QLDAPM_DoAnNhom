using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PET.Data;
using PET.Models;
using QRCoder; // Thêm thư viện QR

namespace PET.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được quản lý thú cưng
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: Pets (Chỉ hiện thú cưng của user đang đăng nhập, Admin thì xem được hết)
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Pets.Include(p => p.User).ToListAsync());
            }
            return View(await _context.Pets.Where(p => p.UserId == currentUserId).ToListAsync());
        }

        // GET: Pets/Details/5 (Cho phép quét QR xem công khai không cần đăng nhập)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pet == null) return NotFound();

            return View(pet);
        }

        // POST: Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Species,Breed,BirthDate,Weight,MedicalHistory")] Pet pet)
        {
            // Gán tự động chủ nhân là user hiện tại
            pet.UserId = _userManager.GetUserId(User)!;
            ModelState.Remove("UserId"); // Bỏ qua validate UserId từ form

            if (ModelState.IsValid)
            {
                // 1. Lưu Pet vào DB trước để lấy được Id tự tăng
                _context.Add(pet);
                await _context.SaveChangesAsync();

                // 2. Tạo đường link dẫn tới trang chi tiết thú cưng
                string petUrl = Url.Action("Details", "Pets", new { id = pet.Id }, Request.Scheme)!;

                // 3. Sinh mã QR và lưu thành file ảnh .png trong wwwroot/qrcodes
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(petUrl, QRCodeGenerator.ECCLevel.Q);
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);

                    string fileName = $"pet_{pet.Id}.png";
                    string folderPath = Path.Combine(_env.WebRootPath, "qrcodes");
                    Directory.CreateDirectory(folderPath); // Đảm bảo thư mục tồn tại
                    string filePath = Path.Combine(folderPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, qrCodeBytes);

                    // 4. Cập nhật đường dẫn ảnh QR vào DB
                    pet.QRCodePath = $"/qrcodes/{fileName}";
                    _context.Update(pet);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(pet);
        }
        // Các hàm Edit, Delete giữ nguyên bên dưới...
    }
}