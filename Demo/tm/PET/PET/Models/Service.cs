using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PET.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Tắm gội, Cắt tỉa, Tiêm phòng...
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; } // Thời gian thực hiện dự kiến
        public string? ImageUrl { get; set; }
    }
}
