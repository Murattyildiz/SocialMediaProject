using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using SosyalMedya_Web.Utilities.Helpers;
using System.Text;

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
        [HttpPost("bilgileri-guncelle")]
        public async Task<IActionResult> UpdateAccountSetting(UserDto userDto)
        {
            var jsonUserDto = JsonConvert.SerializeObject(userDto);
            var content = new StringContent(jsonUserDto, Encoding.UTF8, "application/json");
            var responseMessage = await _httpClientFactory.CreateClient().PostAsync("https://localhost:5190/api/Users/update", content);
            if(responseMessage.IsSuccessStatusCode)
            {
                var successUpdatedUser = await GetUpdateUserResponseMessage(responseMessage);
                TempData["Message"] = successUpdatedUser.Message;
                TempData["Success"] = successUpdatedUser.Success;
                return RedirectToAction("AccountSetting","Setting");
            }
            else
            {
                var successUpdatedUser = await GetUpdateUserResponseMessage(responseMessage);
                TempData["Message"] = successUpdatedUser.Message;
                TempData["Success"] = successUpdatedUser.Success;
                return View();
            }

        }

        private async Task<ApiDataResponse<UserDto>> GetUpdateUserResponseMessage(HttpResponseMessage responseMessage)
        {
            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiDataResponse<UserDto>>(responseContent);
        }
    }
}
