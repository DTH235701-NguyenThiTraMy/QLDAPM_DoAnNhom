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

        [Display(Name = "Tên thú cưng")]
        public string PetName { get; set; } = string.Empty;

        [Display (Name = "Loài")]
        public string? Species { get; set; }

        [Display(Name = "Giống")]
        public string? Breed { get; set; }

        [Display(Name = "Tuổi")]
        public int? Age { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh thú cưng")]
        public string? ImagePath { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [ForeignKey(nameof(CustomerId))]
        public User? Customer { get; set; }

        public bool IsDeleted { get; set; } = false;

        [Display(Name = "Mã QR")]
        [StringLength(64)]
        public string QrToken { get; set; } = Guid.NewGuid().ToString("N");

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<CareNote> CareNotes { get; set; } = new List<CareNote>();
    }
}
