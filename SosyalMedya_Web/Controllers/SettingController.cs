using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;

namespace SosyalMedya_Web.Controllers
{
    public class SettingController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SettingController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("hesap-bilgilerim")]
        public async Task<IActionResult> AccountSetting()
        
        {
            var userId=HttpContext.Session.GetInt32("userId");
            var responseMessage = await _httpClientFactory.CreateClient().GetAsync("https://localhost:5190/api/Users/getbyid?id=" + userId);
            if(responseMessage.IsSuccessStatusCode)
            {
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                var apiDataResponse = JsonConvert.DeserializeObject<ApiDataResponse<UserDto>>(jsonResponse);
                return apiDataResponse.Success ? View(apiDataResponse.Data) : (IActionResult)View("Veri gelmiyor");

            }
            return View("Veri Gelmiyor");
        }
    }
}
