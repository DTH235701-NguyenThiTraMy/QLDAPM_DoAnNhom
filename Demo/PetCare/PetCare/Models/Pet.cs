using PMPetCare.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMPetCare.Models
{
    public class Pet
    {
        [Key]
        public int PetId { get; set; }

        [Required]
        public string PetName { get; set; } = string.Empty;

        public string? Species { get; set; }
        public string? Breed { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Description { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [ForeignKey(nameof(CustomerId))]
        public User? Customer { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<CareNote> CareNotes { get; set; } = new List<CareNote>();
    }
}
