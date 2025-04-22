using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using SosyalMedya_Web.Utilities.Helpers;
using System.Net.Http.Headers;
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
            var userId=HttpContext.Session.GetInt32("UserId");
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
        [HttpPost("/photo-update")]
        public async Task<IActionResult> UpdateUserImage(UserImage userImage)
        {
            if(userImage.ImagePath != null)
            {
                using (var formContent = new MultipartFormDataContent())
                {
                    formContent.Add(new StringContent(userImage.Id.ToString()), "Id");
                    formContent.Add(new StringContent(userImage.UserId.ToString()), "UserId");
                    formContent.Add(new StringContent(userImage.ImagePath.FileName), "ImagePath");
                    formContent.Add(new StreamContent(userImage.ImagePath.OpenReadStream())
                    {
                        Headers =
                        {
                            ContentLength=userImage.ImagePath.Length,
                            ContentType=new MediaTypeHeaderValue(userImage.ImagePath.ContentType)
                        }
                    },
                    "ImagePath", userImage.ImagePath.FileName);

                    var responseMessage= await _httpClientFactory.CreateClient().PostAsync("https://localhost:5190/api/UserImages/update", formContent);
                    var successUpdatedUserImage = await GetUpdateUserImageResponseMessage(responseMessage);
                    TempData["Message"] = successUpdatedUserImage.Message;
                    TempData["Success"] = successUpdatedUserImage.Success;

                    return RedirectToAction("AccountSetting", "Setting");
                }
            }
            return RedirectToAction("AccountSetting", "Setting");
        }

        [HttpGet("kod-dogrulama")]
        public async Task<IActionResult> GetVerifyCode()
        {
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            return View();
        }


        [HttpPost("kod")]
        public async Task<IActionResult> GetVerifyCode(VerificationCodeDto verificationCodeDto)
        {

            var httpClient = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var jsonInfo = JsonConvert.SerializeObject(verificationCodeDto);
            var content = new StringContent(jsonInfo, Encoding.UTF8, "application/json");
            var responseMessage = await httpClient.PostAsync("https://localhost:44347/api/VerificationCodes/sendcode", content);
            if (responseMessage.IsSuccessStatusCode)
            {
                var response = new
                {
                    Success = true,
                    Url = "kod-dogrulama"
                };

                return Json(response);
            }
            return RedirectToAction("AccountSetting", "Settings");
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode(VerificationCodeDto verificationCodeDto)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var jsonInfo = JsonConvert.SerializeObject(verificationCodeDto);
            var content = new StringContent(jsonInfo, Encoding.UTF8, "application/json");
            var responseMessage = await httpClient.PostAsync("https://localhost:44347/api/VerificationCodes/checkcode", content);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var apiDataResponse = JsonConvert.DeserializeObject<ApiDataResponse<VerificationCodeDto>>(responseContent);

                var response = new
                {
                    Success = true,
                    Message = apiDataResponse.Message,
                    Url = "sifre-guncelle"
                };

                return Json(response);
            }
            else
            {
                var response = new
                {
                    Message = "Kod doğrulanamadı ! . Lütfen tekrar deneyin",
                };
                return Json(response);
            }

        }

        private async Task<ApiDataResponse<UserImage>> GetUpdateUserImageResponseMessage(HttpResponseMessage responseMessage)
        {
            var responseContent=await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiDataResponse<UserImage>>(responseContent);
        }

        private async Task<ApiDataResponse<UserDto>> GetUpdateUserResponseMessage(HttpResponseMessage responseMessage)
        {
            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiDataResponse<UserDto>>(responseContent);
        }
    }
}
