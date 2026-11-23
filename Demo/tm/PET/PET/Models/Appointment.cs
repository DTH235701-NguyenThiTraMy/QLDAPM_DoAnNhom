using System.ComponentModel.DataAnnotations;

namespace PET.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }

        // Trạng thái: Pending (Chờ duyệt), Confirmed (Đã xác nhận), Completed (Hoàn thành), Cancelled (Đã hủy)
        public string Status { get; set; } = "Pending";

        // Phục vụ Module nhắc lịch hẹn
        public bool IsReminded { get; set; } = false;

        // Khóa ngoại
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        public int ServiceId { get; set; }
        public Service? Service { get; set; }
    }
}
