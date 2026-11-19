using System.ComponentModel.DataAnnotations;

namespace PET.Models
{
    public class Pet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thú cưng")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public string Species { get; set; } // Chó, Mèo, Khác...
        public string? Breed { get; set; }   // Giống (Poodle, Corgi, Mèo Anh...)
        public DateTime? BirthDate { get; set; }
        public double Weight { get; set; }   // Cân nặng (kg)
        public string? MedicalHistory { get; set; } // Tiền sử bệnh / dị ứng (rất hữu ích cho AI Care Assistant)
        public string? ImageUrl { get; set; }
        public string? QRCodePath { get; set; } // Lưu đường dẫn ảnh QR đã sinh ra

        // Khóa ngoại liên kết chủ nuôi
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
