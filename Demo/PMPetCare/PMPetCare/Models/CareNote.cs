using PMPetCare.Models;
using System.ComponentModel.DataAnnotations;

namespace PMPetCare.Models
{
    public class CareNote
    {
        [Key]

        [Display(Name = "Mã ghi chú chăm sóc")]
        public int CareNoteId { get; set; }

        [Display(Name = "Mã thú cưng")]
        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        [Display(Name = "Mã lịch hẹn")]
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Required]
        public string StaffId { get; set; } = string.Empty;
        public User? Staff { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReminderDate { get; set; }
    }
}
