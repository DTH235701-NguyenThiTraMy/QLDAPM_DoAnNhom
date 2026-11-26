using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PET.Data;
using PET.Models;

namespace PET.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // (Giữ nguyên Action Dashboard() hiện có của bạn ở đây...)

        // GET: /Admin/Users (Danh sách tài khoản & quyền hạn)
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var adminIds = new HashSet<string>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    adminIds.Add(user.Id);
                }
            }

            ViewBag.AdminIds = adminIds;
            ViewBag.CurrentUserId = _userManager.GetUserId(User);
            return View(users);
        }

        // POST: Cấp hoặc gỡ quyền Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["WarningMessage"] = "Bạn không thể tự gỡ quyền Admin của chính mình.";
                return RedirectToAction(nameof(Users));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                TempData["SuccessMessage"] = $"Đã gỡ quyền Admin của {user.UserName}.";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["SuccessMessage"] = $"Đã cấp quyền Admin cho {user.UserName}.";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Khóa hoặc mở khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["WarningMessage"] = "Bạn không thể tự khóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Users));
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["SuccessMessage"] = $"Đã mở khóa tài khoản {user.UserName}.";
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100));
                TempData["SuccessMessage"] = $"Đã khóa tài khoản {user.UserName}.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}