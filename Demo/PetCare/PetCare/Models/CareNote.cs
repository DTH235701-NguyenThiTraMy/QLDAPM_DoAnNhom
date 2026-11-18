using PMPetCare.Models;
using PMPetCare.Models;
using System.ComponentModel.DataAnnotations;

namespace PMPetCare.Models
{
    public class CareNote
    {
        [Key]
        public int CareNoteId { get; set; }

        public int PetId { get; set; }
        public Pet? Pet { get; set; }

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
