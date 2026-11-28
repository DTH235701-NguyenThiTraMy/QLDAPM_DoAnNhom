using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HatGiong701.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000)]
        public string Comment { get; set; }

        public string? ImageUrl { get; set; } // Ảnh thực tế khách chụp cây nảy mầm

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int SeedId { get; set; }
        [ForeignKey("SeedId")]
        public Seed? Seed { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
