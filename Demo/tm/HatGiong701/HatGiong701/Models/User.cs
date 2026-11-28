using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HatGiong701.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ mặc định")]
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
