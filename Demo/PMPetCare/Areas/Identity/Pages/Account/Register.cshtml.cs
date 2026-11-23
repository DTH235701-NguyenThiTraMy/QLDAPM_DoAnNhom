
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PMPetCare.Models;

namespace PMPetCare.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [StringLength(100, MinimumLength = 6,
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
            [DataType(DataType.Password)]
            [Compare("Password",
                ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            [Display(Name = "Xác nhận mật khẩu")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName
            };

            // 1. Tạo tài khoản
            var createResult = await _userManager.CreateAsync(
                user, Input.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return Page();
            }

            // 2. Gán vai trò Customer
            var roleResult = await _userManager.AddToRoleAsync(
                user, "Customer");

            if (!roleResult.Succeeded)
            {
                // Nếu gán vai trò thất bại, xóa tài khoản vừa tạo
                var deleteResult = await _userManager.DeleteAsync(user);

                if (!deleteResult.Succeeded)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Tạo tài khoản thành công nhưng không thể gán vai trò Customer. " +
                        "Tài khoản chưa được đăng nhập; cần kiểm tra lại trong cơ sở dữ liệu.");
                }
                else
                {
                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError(
                            string.Empty, error.Description);
                }

                return Page();
            }

            // 3. Chỉ đăng nhập sau khi gán vai trò thành công
            await _signInManager.SignInAsync(
                user, isPersistent: false);

            return RedirectToPage("/Index");
        }
    }
}
