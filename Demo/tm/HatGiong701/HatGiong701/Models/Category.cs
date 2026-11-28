using System.ComponentModel.DataAnnotations;

namespace HatGiong701.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        // Thuộc tính điều hướng (1 Danh mục có nhiều Hạt giống)
        public ICollection<Seed>? Seeds { get; set; }
    }
}
