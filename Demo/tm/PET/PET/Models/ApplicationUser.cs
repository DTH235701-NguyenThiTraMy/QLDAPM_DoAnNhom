using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PET.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }

        // Quan hệ 1-nhiều với Thú cưng và Lịch hẹn
        public ICollection<Pet>? Pets { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
