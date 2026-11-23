using System.Text;
using System.Text.Json;

namespace PET.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiApi:ApiKey"] ?? "";
        }

        public async Task<string> AskPetCareAssistantAsync(string userQuestion, string? petContext = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "Lỗi: Chưa cấu hình Gemini API Key trong appsettings.json.";

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Bạn là AI Care Assistant - trợ lý chuyên gia chăm sóc thú cưng thân thiện, tận tâm.");
            promptBuilder.AppendLine("Hãy trả lời ngắn gọn, dễ hiểu, có gạch đầu dòng rõ ràng bằng tiếng Việt.");

            if (!string.IsNullOrEmpty(petContext))
            {
                promptBuilder.AppendLine("\nThông tin hồ sơ thú cưng đang được hỏi:");
                promptBuilder.AppendLine(petContext);
            }

            promptBuilder.AppendLine($"\nCâu hỏi của chủ nuôi: {userQuestion}");

            var requestBody = new
            {
                contents = new[]
                {
            new { parts = new[] { new { text = promptBuilder.ToString() } } }
        }
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);

            // Danh sách các model ưu tiên (nếu model chính quá tải 503 sẽ tự chuyển sang model dự phòng)
            string[] models = { "gemini-3.5-flash-lite", "gemini-2.5-flash", "gemini-2.0-flash", "gemini-2.0-flash-lite" };

            foreach (var model in models)
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
                using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);

                    var reply = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return reply ?? "Không có phản hồi từ AI.";
                }

                // Nếu lỗi không phải do quá tải (503) hoặc giới hạn tốc độ (429) thì dừng và báo lỗi
                if ((int)response.StatusCode != 503 && (int)response.StatusCode != 429)
                {
                    return "Xin lỗi, hệ thống AI đang gặp sự cố kết nối. Bạn vui lòng thử lại sau nhé!";
                }
            }

            return "Hiện tại trợ lý AI đang có quá nhiều lượt truy cập cùng lúc. Bạn vui lòng đợi vài giây rồi gửi lại câu hỏi nhé!";
        }
    }
}