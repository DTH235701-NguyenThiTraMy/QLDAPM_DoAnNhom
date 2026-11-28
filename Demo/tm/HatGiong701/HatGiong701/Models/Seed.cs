using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HatGiong701.Models
{
    public class Seed
    {
        [Key]
        public int SeedId { get; set; }

        [Required(ErrorMessage = "Tên hạt giống không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên hạt giống")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Số lượng tồn")]
        public int StockQuantity { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        // --- CÁC THUỘC TÍNH ĐẶC THÙ CHO CHỨC NĂNG GỢI Ý ---
        [StringLength(50)]
        [Display(Name = "Mùa vụ thích hợp")] // VD: Xuân, Hè, Thu, Đông, Quanh năm
        public string PlantingSeason { get; set; }

        [StringLength(50)]
        [Display(Name = "Không gian trồng")] // VD: Ban công, Sân vườn, Trong nhà
        public string SuitableSpace { get; set; }

        [StringLength(30)]
        [Display(Name = "Độ khó chăm sóc")] // VD: Dễ, Trung bình, Khó
        public string CareLevel { get; set; }

        [Display(Name = "Tỷ lệ nảy mầm (%)")]
        public int GerminationRate { get; set; }

        [Display(Name = "Thời gian thu hoạch (ngày)")]
        public int HarvestDays { get; set; }

        [Display(Name = "Hướng dẫn gieo trồng")]
        public string? PlantingGuide { get; set; }

        // Khóa ngoại liên kết với Category
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        // Liên kết với Đánh giá
        public ICollection<Review>? Reviews { get; set; }
    }
}
