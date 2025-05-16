using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Business.Abstract;

namespace Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;

        public ChatBotController(IChatBotService chatBotService)
        {
            _chatBotService = chatBotService;
        }

        [HttpPost("message")]
        public async Task<IActionResult> ProcessMessage([FromBody] string message)
        {
            var response = await _chatBotService.ProcessMessage(message);
            return Ok(response);
        }

        [HttpGet("weather")]
        public async Task<IActionResult> GetWeather()
        {
            var response = await _chatBotService.GetWeatherInfo("Bingol");
            return Ok(response);
        }

        [HttpGet("news")]
        public async Task<IActionResult> GetNews()
        {
            var response = await _chatBotService.GetLatestNews();
            return Ok(response);
        }

        [HttpGet("university")]
        public async Task<IActionResult> GetUniversityInfo([FromQuery] string query)
        {
            var response = await _chatBotService.GetUniversityInfo(query);
            return Ok(response);
        }
    }
} 