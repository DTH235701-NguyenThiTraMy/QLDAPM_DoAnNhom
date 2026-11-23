using System.ComponentModel.DataAnnotations;

namespace PMPetCare.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [Display(Name = "Tên dịch vụ")]
        public string ServiceName { get; set; } = string.Empty;

        [Display(Name = "Mô tả dịch vụ")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]

        [Display(Name = "Giá dịch vụ")]
        public decimal Price { get; set; }

        [Display(Name = "Thời lượng dịch vụ (phút)")]
        public int Duration { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
