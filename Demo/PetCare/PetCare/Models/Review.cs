using PMPetCare.Models;
using System.ComponentModel.DataAnnotations;

namespace PMPetCare.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        public User? Customer { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
