using Microsoft.AspNetCore.Identity;
namespace PMPetCare.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
