using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Smart_Store_For_Clothes.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AIChatController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AIChatController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new
                {
                    success = false,
                    reply = "Please write a message first."
                });
            }

            var apiKey = _configuration["Groq:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Json(new
                {
                    success = false,
                    reply = "Groq API key is missing."
                });
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a helpful AI assistant for a clothes store. Answer simply and clearly. Help customers with clothes, colors, sizes, style, prices, and shopping questions. If the customer writes Arabic, answer in Arabic."
                    },
                    new
                    {
                        role = "user",
                        content = request.Message
                    }
                },
                temperature = 0.7,
                max_tokens = 300
            };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                content
            );

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Json(new
                {
                    success = false,
                    reply = "AI service error. Please try again later."
                });
            }

            string reply = "Sorry, I could not understand.";

            using var doc = JsonDocument.Parse(responseText);

            if (doc.RootElement.TryGetProperty("choices", out var choicesArray))
            {
                var firstChoice = choicesArray.EnumerateArray().FirstOrDefault();

                if (firstChoice.TryGetProperty("message", out var messageElement))
                {
                    if (messageElement.TryGetProperty("content", out var contentElement))
                    {
                        reply = contentElement.GetString() ?? reply;
                    }
                }
            }

            return Json(new
            {
                success = true,
                reply
            });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}