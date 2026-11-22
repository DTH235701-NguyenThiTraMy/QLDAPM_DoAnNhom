using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PET.Data;
using PET.Models;
using PET.Services;

namespace PET.Controllers
{
    [Authorize]
    public class AiAssistantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GeminiService _geminiService;

        public AiAssistantController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, GeminiService geminiService)
        {
            _context = context;
            _userManager = userManager;
            _geminiService = geminiService;
        }

        // GET: AiAssistant
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Lấy danh sách thú cưng của user để đưa vào dropdown chọn
            var userPets = await _context.Pets.Where(p => p.UserId == userId).ToListAsync();
            ViewBag.Pets = new SelectList(userPets, "Id", "Name");

            // Lấy lịch sử chat của user này
            var chatHistory = await _context.ChatLogs
                .Include(c => c.Pet)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(20)
                .ToListAsync();

            return View(chatHistory);
        }

        // POST: AiAssistant/Ask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(string question, int? petId)
        {
            if (string.IsNullOrWhiteSpace(question))
                return RedirectToAction(nameof(Index));

            var userId = _userManager.GetUserId(User)!;
            string? petContext = null;

            // Nếu người dùng có chọn 1 bé thú cưng, lấy thông tin chi tiết gửi kèm cho AI
            if (petId.HasValue)
            {
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
                if (pet != null)
                {
                    petContext = $"- Tên: {pet.Name}\n" +
                                 $"- Loài: {pet.Species}\n" +
                                 $"- Giống: {pet.Breed ?? "Không rõ"}\n" +
                                 $"- Cân nặng: {pet.Weight} kg\n" +
                                 $"- Ngày sinh: {(pet.BirthDate.HasValue ? pet.BirthDate.Value.ToString("dd/MM/yyyy") : "Không rõ")}\n" +
                                 $"- Tiền sử bệnh/Lưu ý: {pet.MedicalHistory ?? "Khỏe mạnh"}";
                }
            }

            // Gọi AI lấy câu trả lời
            string aiAnswer = await _geminiService.AskPetCareAssistantAsync(question, petContext);

            // Lưu vào Database
            var chatLog = new ChatLog
            {
                UserId = userId,
                PetId = petId,
                Question = question,
                Answer = aiAnswer,
                CreatedAt = DateTime.Now
            };

            _context.ChatLogs.Add(chatLog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // Thêm 2 phương thức này vào bên trong class AiAssistantController

        [HttpGet]
        public async Task<IActionResult> GetWidgetData()
        {
            var userId = _userManager.GetUserId(User);
            var pets = await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            var history = await _context.ChatLogs
                .Include(c => c.Pet)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.CreatedAt) // Sắp xếp cũ -> mới để hiển thị từ trên xuống dưới như khung chat thật
                .Take(15)
                .Select(c => new
                {
                    c.Question,
                    c.Answer,
                    PetName = c.Pet != null ? c.Pet.Name : null,
                    Time = c.CreatedAt.ToString("HH:mm")
                })
                .ToListAsync();

            return Json(new { pets, history });
        }

        [HttpPost]
        public async Task<IActionResult> AskAjax([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { error = "Vui lòng nhập câu hỏi." });

            var userId = _userManager.GetUserId(User)!;
            string? petContext = null;
            string? petName = null;

            if (request.PetId.HasValue)
            {
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == request.PetId && p.UserId == userId);
                if (pet != null)
                {
                    petName = pet.Name;
                    petContext = $"- Tên: {pet.Name}\n- Loài: {pet.Species}\n- Giống: {pet.Breed ?? "Không rõ"}\n- Cân nặng: {pet.Weight} kg\n- Tiền sử bệnh: {pet.MedicalHistory ?? "Khỏe mạnh"}";
                }
            }

            string aiAnswer = await _geminiService.AskPetCareAssistantAsync(request.Question, petContext);

            var chatLog = new ChatLog
            {
                UserId = userId,
                PetId = request.PetId,
                Question = request.Question,
                Answer = aiAnswer,
                CreatedAt = DateTime.Now
            };

            _context.ChatLogs.Add(chatLog);
            await _context.SaveChangesAsync();

            return Json(new
            {
                question = chatLog.Question,
                answer = chatLog.Answer,
                petName = petName,
                time = chatLog.CreatedAt.ToString("HH:mm")
            });
        }

        // Class phụ trợ nhận dữ liệu JSON (đặt ngay dưới hoặc trong cùng file AiAssistantController.cs)
        public class ChatRequest
        {
            public string Question { get; set; } = string.Empty;
            public int? PetId { get; set; }
        }
    }
}