using HatGiong701.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HatGiong701.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // 1. Danh sách tài khoản (hỗ trợ tìm kiếm theo tên hoặc email)
        public async Task<IActionResult> Index(string? searchString)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    (u.FullName != null && u.FullName.Contains(searchString)) ||
                    (u.Email != null && u.Email.Contains(searchString)));
            }

            var users = await usersQuery.ToListAsync();

            // Lấy danh sách quyền (Role) của từng tài khoản
            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Customer";
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.SearchString = searchString;
            ViewBag.CurrentAdminId = _userManager.GetUserId(User);

            return View(users);
        }

        // 2. Chuyển đổi quyền (Admin <-> Customer)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Không cho phép Admin tự hạ quyền chính mình
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["UserError"] = "Bạn không thể tự thay đổi quyền của tài khoản đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (!currentRoles.Contains("Customer"))
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }
                TempData["UserMessage"] = $"Đã chuyển tài khoản {user.Email} về quyền Khách hàng (Customer).";
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "Customer");
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["UserMessage"] = $"Đã cấp quyền Quản trị viên (Admin) cho {user.Email}.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Khóa / Mở khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Không cho phép Admin tự khóa chính mình
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["UserError"] = "Bạn không thể tự khóa tài khoản đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            // Đảm bảo bật tính năng Lockout cho user
            await _userManager.SetLockoutEnabledAsync(user, true);

            bool isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;
            if (isLocked)
            {
                // Mở khóa
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["UserMessage"] = $"Đã mở khóa tài khoản {user.Email}.";
            }
            else
            {
                // Khóa 100 năm
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                TempData["UserMessage"] = $"Đã khóa tài khoản {user.Email}. Người dùng này sẽ không thể đăng nhập.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}