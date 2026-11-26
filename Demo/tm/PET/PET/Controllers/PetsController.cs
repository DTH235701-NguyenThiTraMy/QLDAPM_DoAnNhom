using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PET.Data;
using PET.Models;
using QRCoder;

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

        // GET: Pets/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Species,Breed,BirthDate,Weight,MedicalHistory")] Pet pet, IFormFile? imageFile)
        {
            pet.UserId = _userManager.GetUserId(User)!;
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                // 1. Xử lý lưu file ảnh đại diện nếu người dùng có chọn ảnh
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadFolder = Path.Combine(_env.WebRootPath, "images", "pets");
                    Directory.CreateDirectory(uploadFolder);

                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    pet.ImageUrl = $"/images/pets/{uniqueFileName}";
                }

                // 2. Lưu Pet vào DB để lấy Id tự tăng
                _context.Add(pet);
                await _context.SaveChangesAsync();

                // 3. Sinh mã QR dẫn tới trang chi tiết thú cưng
                string petUrl = Url.Action("Details", "Pets", new { id = pet.Id }, Request.Scheme)!;
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(petUrl, QRCodeGenerator.ECCLevel.Q);
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);

                    string qrFileName = $"pet_{pet.Id}.png";
                    string qrFolderPath = Path.Combine(_env.WebRootPath, "qrcodes");
                    Directory.CreateDirectory(qrFolderPath);
                    await System.IO.File.WriteAllBytesAsync(Path.Combine(qrFolderPath, qrFileName), qrCodeBytes);

                    pet.QRCodePath = $"/qrcodes/{qrFileName}";
                    _context.Update(pet);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(pet);
        }

        // GET: Pets/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();

            // Kiểm tra quyền sở hữu (chỉ chủ nhân hoặc Admin mới được sửa)
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && pet.UserId != currentUserId)
            {
                return Forbid();
            }

            return View(pet);
        }

        // POST: Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Species,Breed,BirthDate,Weight,MedicalHistory,ImageUrl,QRCodePath,UserId")] Pet pet, IFormFile? imageFile)
        {
            if (id != pet.Id) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && pet.UserId != currentUserId)
            {
                return Forbid();
            }

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Nếu người dùng chọn file ảnh mới thì lưu ảnh mới & xóa ảnh cũ (nếu có)
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(pet.ImageUrl) && pet.ImageUrl.StartsWith("/images/pets/"))
                        {
                            var oldImagePath = Path.Combine(_env.WebRootPath, pet.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string uploadFolder = Path.Combine(_env.WebRootPath, "images", "pets");
                        Directory.CreateDirectory(uploadFolder);

                        string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        pet.ImageUrl = $"/images/pets/{uniqueFileName}";
                    }

                    // 2. Nếu hồ sơ cũ chưa có QR thì tạo bổ sung luôn
                    if (string.IsNullOrEmpty(pet.QRCodePath))
                    {
                        string petUrl = Url.Action("Details", "Pets", new { id = pet.Id }, Request.Scheme)!;
                        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                        {
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(petUrl, QRCodeGenerator.ECCLevel.Q);
                            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                            byte[] qrCodeBytes = qrCode.GetGraphic(20);

                            string fileName = $"pet_{pet.Id}.png";
                            string folderPath = Path.Combine(_env.WebRootPath, "qrcodes");
                            Directory.CreateDirectory(folderPath);
                            await System.IO.File.WriteAllBytesAsync(Path.Combine(folderPath, fileName), qrCodeBytes);

                            pet.QRCodePath = $"/qrcodes/{fileName}";
                        }
                    }

                    _context.Update(pet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetExists(pet.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pet);
        }

        // GET: Pets/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pet == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && pet.UserId != currentUserId)
            {
                return Forbid();
            }

            return View(pet);
        }

        // POST: Pets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                var currentUserId = _userManager.GetUserId(User);
                if (!User.IsInRole("Admin") && pet.UserId != currentUserId)
                {
                    return Forbid();
                }

                // Xóa file ảnh QR vật lý trong wwwroot/qrcodes nếu tồn tại
                if (!string.IsNullOrEmpty(pet.QRCodePath))
                {
                    var physicalPath = Path.Combine(_env.WebRootPath, pet.QRCodePath.TrimStart('/'));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }

                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.Id == id);
        }
    }
}