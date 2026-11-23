using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PET.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(150)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [StringLength(50)]
        public string Category { get; set; } // Ví dụ: Thức ăn, Phụ kiện, Thuốc & Vitamin, Đồ chơi

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không hợp lệ")]
        public int StockQuantity { get; set; } // Số lượng tồn kho

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true; // Trạng thái còn kinh doanh hay ngừng bán
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Quan hệ 1-nhiều với Đánh giá
        public ICollection<Review>? Reviews { get; set; }
    }
}
