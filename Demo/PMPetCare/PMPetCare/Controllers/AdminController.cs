using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMPetCare.Data;
using Microsoft.AspNetCore.Identity;
using PMPetCare.Models;
using PMPetCare.Models.ViewModels;

namespace PMPetCare.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Thống kê dữ liệu hệ thống
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalPets = _context.Pets.Count();
            ViewBag.TotalServices = _context.Services.Count();
            ViewBag.TotalAppointments = _context.Appointments.Count();

            ViewBag.PendingAppointments = _context.Appointments
                .Count(a => a.Status == "Pending");

            return View();
        }
        public async Task<IActionResult> Users()
        {
            var users = _context.Users.ToList();

            ViewBag.UserRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                ViewBag.UserRoles[user.Id] = roles;
            }

            return View(users);
        }
        // Hiển thị form tạo tài khoản
        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = new List<string> { "Staff", "Customer" };

            return View();
        }
        // Xử lý tạo tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            // Kiểm tra dữ liệu nhập vào
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Chỉ cho phép tạo Staff hoặc Customer
            if (model.Role != "Staff" && model.Role != "Customer")
            {
                ModelState.AddModelError("Role", "Vai trò không hợp lệ.");
                return View(model);
            }

            // Kiểm tra email đã tồn tại
            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            // Tạo tài khoản mới
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address ?? string.Empty,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Gán vai trò cho tài khoản
                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

                if (roleResult.Succeeded)
                {
                    TempData["Success"] = "Tạo tài khoản thành công!";
                    return RedirectToAction(nameof(Users));
                }

                // Nếu gán vai trò thất bại, xóa tài khoản vừa tạo
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

    }
}