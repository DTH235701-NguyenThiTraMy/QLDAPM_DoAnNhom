using PMPetCare.Models;
using PMPetCare.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMPetCare.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [ForeignKey(nameof(CustomerId))]
        public User? Customer { get; set; }

        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        public string? StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public User? Staff { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string? Note { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Review? Review { get; set; }
        public ICollection<CareNote> CareNotes { get; set; } = new List<CareNote>();
    }
}
