using System.ComponentModel.DataAnnotations;

namespace PET.Models
{
    public class ChatLog
    {
        public int Id { get; set; }

        [Required]
        public string Question { get; set; } // Câu hỏi của người dùng

        [Required]
        public string Answer { get; set; }   // Câu trả lời từ AI

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Khóa ngoại người hỏi
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Liên kết thú cưng đang được tư vấn (có thể null nếu hỏi chung chung)
        public int? PetId { get; set; }
        public Pet? Pet { get; set; }
    }
}
