using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SosyalMedya_Web.Controllers
{
    public class ChatBotController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatBotController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            try
            {
                // API'ye doğrudan controller üzerinden istek yapalım
                var client = _httpClientFactory.CreateClient();
                
                // LocalHost üzerinden API'ye erişim - gerçek ortamda API URL'ini ayarlayın
                var apiUrl = "https://localhost:5190/api/chatbot/message"; // Web_Api projesi portu
                
                var content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return Content(responseString, "text/plain");
                }
                
                return BadRequest("API yanıt vermedi. Durum kodu: " + response.StatusCode);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }
    }
} 