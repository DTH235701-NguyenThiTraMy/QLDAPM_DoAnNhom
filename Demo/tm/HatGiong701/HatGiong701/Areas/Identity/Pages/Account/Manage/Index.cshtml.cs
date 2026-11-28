#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using HatGiong701.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HatGiong701.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _env;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        public string Username { get; set; }
        public string CurrentAvatarUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Địa chỉ giao hàng")]
            public string Address { get; set; }

            [Display(Name = "Ảnh đại diện")]
            public IFormFile AvatarFile { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            CurrentAvatarUrl = user.AvatarUrl;

            Input = new InputModel
            {
                FullName = user.FullName,
                PhoneNumber = phoneNumber,
                Address = user.Address
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Đã xảy ra lỗi khi cập nhật số điện thoại.";
                    return RedirectToPage();
                }
            }

            bool isChanged = false;
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                isChanged = true;
            }
            if (Input.Address != user.Address)
            {
                user.Address = Input.Address;
                isChanged = true;
            }

            // Xử lý lưu ảnh đại diện nếu có chọn file mới
            if (Input.AvatarFile != null && Input.AvatarFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string ext = Path.GetExtension(Input.AvatarFile.FileName);
                string fileName = $"{user.Id}_{Guid.NewGuid():N}{ext}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.AvatarFile.CopyToAsync(fileStream);
                }

                user.AvatarUrl = "/images/avatars/" + fileName;
                isChanged = true;
            }

            if (isChanged)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Đã xảy ra lỗi khi cập nhật thông tin hồ sơ.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Hồ sơ cá nhân của bạn đã được cập nhật thành công.";
            return RedirectToPage();
        }
    }
}