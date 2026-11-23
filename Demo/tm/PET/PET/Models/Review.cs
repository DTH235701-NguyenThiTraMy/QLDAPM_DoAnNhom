using System.ComponentModel.DataAnnotations;

namespace PET.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi")]
        [StringLength(1000)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Phản hồi lại từ phía Admin/Cửa hàng (nếu có)
        public string? AdminReply { get; set; }

        // Khóa ngoại người đánh giá
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Đánh giá cho Dịch vụ (có thể null nếu đang đánh giá sản phẩm)
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }

        // Đánh giá cho Sản phẩm (có thể null nếu đang đánh giá dịch vụ)
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
