using System.ComponentModel.DataAnnotations;

namespace HatGiong701.Models
{
    public class BlogPost
    {
        [Key]
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết.")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề bài viết")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tóm tắt ngắn.")]
        [StringLength(500)]
        [Display(Name = "Tóm tắt nội dung")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung chi tiết.")]
        [Display(Name = "Nội dung chi tiết")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Ảnh bìa")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Danh mục chủ đề")]
        public string Category { get; set; } = "Kỹ thuật gieo trồng";

        [Display(Name = "Ngày đăng")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}