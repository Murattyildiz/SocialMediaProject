using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using SosyalMedya_Web.Utilities.Helpers;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using System;

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
            var userId = HttpContext.Session.GetInt32("UserId");
            var responseMessage = await _httpClientFactory.CreateClient().GetAsync("https://localhost:5190/api/Users/getbyid?id=" + userId);
            if (responseMessage.IsSuccessStatusCode)
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
            if (responseMessage.IsSuccessStatusCode)
            {
                var successUpdatedUser = await GetUpdateUserResponseMessage(responseMessage);
                TempData["Message"] = successUpdatedUser.Message;
                TempData["Success"] = successUpdatedUser.Success;
                return RedirectToAction("AccountSetting", "Setting");
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
        public async Task<IActionResult> UpdateUserImage([FromForm] UserImage userImage)
        {
            try
        {
                if (userImage == null)
                {
                    return Json(new { success = false, message = "Geçersiz form verisi." });
                }

                if (userImage.UserId <= 0)
                {
                    return Json(new { success = false, message = "Geçersiz kullanıcı ID'si." });
                }

                if (userImage.ImageFile == null || userImage.ImageFile.Length == 0)
                {
                    return Json(new { success = false, message = "Lütfen bir resim seçin." });
                }

                // Dosya uzantısını kontrol et
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(userImage.ImageFile.FileName)?.ToLower();
                
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    return Json(new { success = false, message = "Sadece .jpg, .jpeg, .png ve .gif uzantılı dosyalar yüklenebilir." });
                }

                // Dosya boyutunu kontrol et (max 5MB)
                if (userImage.ImageFile.Length > 5 * 1024 * 1024)
            {
                    return Json(new { success = false, message = "Dosya boyutu 5MB'dan büyük olamaz." });
                }

                using (var formContent = new MultipartFormDataContent())
                {
                    try
                    {
                        // Benzersiz dosya adı oluştur
                        var fileName = $"{Guid.NewGuid()}{extension}";

                        // Form verilerini ekle
                        formContent.Add(new StringContent(userImage.UserId.ToString()), "userId");
                        formContent.Add(new StringContent(fileName), "ImagePath");

                        // Dosyayı ekle
                        using (var ms = new MemoryStream())
                        {
                            await userImage.ImageFile.CopyToAsync(ms);
                            var fileContent = new ByteArrayContent(ms.ToArray());
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue(userImage.ImageFile.ContentType);
                            formContent.Add(fileContent, "imageFile", fileName);
                        }

                        var token = HttpContext.Session.GetString("Token");
                        if (string.IsNullOrEmpty(token))
                        {
                            return Json(new { success = false, message = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın." });
                        }

                        var client = _httpClientFactory.CreateClient();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        // Önce mevcut görseli kontrol et
                        var checkImageUrl = $"https://localhost:5190/api/UserImages/getallbyuserid?userId={userImage.UserId}";
                        var checkResponse = await client.GetAsync(checkImageUrl);
                        
                        if (!checkResponse.IsSuccessStatusCode)
                        {
                            return Json(new { success = false, message = "Kullanıcı görsel bilgileri alınamadı." });
                        }

                        var checkContent = await checkResponse.Content.ReadAsStringAsync();
                        var existingImages = JsonConvert.DeserializeObject<ApiDataResponse<List<UserImage>>>(checkContent);

                        if (existingImages == null)
                        {
                            return Json(new { success = false, message = "Kullanıcı görsel bilgileri işlenemedi." });
                        }

                        string apiUrl;
                        if (existingImages.Data == null || !existingImages.Data.Any() || existingImages.Data.All(x => x.ImagePath == "/images/default.jpg"))
                        {
                            // Kullanıcının görseli yok, yeni ekle
                            apiUrl = "https://localhost:5190/api/UserImages/add";
                        }
                        else
                        {
                            // Mevcut görseli güncelle
                            var existingImage = existingImages.Data.FirstOrDefault();
                            if (existingImage == null)
                            {
                                return Json(new { success = false, message = "Mevcut görsel bilgisi alınamadı." });
                            }

                            apiUrl = "https://localhost:5190/api/UserImages/update";
                            formContent.Add(new StringContent(existingImage.Id.ToString()), "Id");
                        }

                        var responseMessage = await client.PostAsync(apiUrl, formContent);
                        
                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            var errorContent = await responseMessage.Content.ReadAsStringAsync();
                            var statusCode = (int)responseMessage.StatusCode;
                            
                            Console.WriteLine($"API Error - Status Code: {statusCode}");
                            Console.WriteLine($"Error Content: {errorContent}");

                            var errorResponse = JsonConvert.DeserializeObject<ApiDataResponse<object>>(errorContent);
                            var errorMessage = errorResponse?.Message ?? errorContent;
                            
                            return Json(new { success = false, message = errorMessage });
                        }

                        var responseContent = await responseMessage.Content.ReadAsStringAsync();
                        var successResponse = JsonConvert.DeserializeObject<ApiDataResponse<UserImage>>(responseContent);

                        if (successResponse?.Success == true && successResponse.Data != null)
                        {
                            // Session'daki kullanıcı resmini güncelle
                            HttpContext.Session.SetString("UserImage", successResponse.Data.ImagePath);
                            return Json(new { success = true, message = "Profil resmi başarıyla güncellendi." });
                }
                        else
                        {
                            return Json(new { 
                                success = false, 
                                message = successResponse?.Message ?? "Profil resmi güncellenirken bir hata oluştu." 
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Form işleme hatası: {ex}");
                        return Json(new { success = false, message = "Form verisi işlenirken bir hata oluştu." });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Genel hata: {ex}");
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu." });
            }
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
            var responseMessage = await httpClient.PostAsync("https://localhost:5190/api/VerificationCodes/sendcode", content);
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
            var responseMessage = await httpClient.PostAsync("https://localhost:5190/api/VerificationCodes/checkcode", content);
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

        [HttpGet("sifre-guncelle")]
        public async Task<IActionResult> ChangePassword()
        {
            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            return View();
        }
        //[Authorize(Roles = "admin,user")]
        [HttpPost("sifre-guncelle")]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var jsonInfo = JsonConvert.SerializeObject(changePassword);
            var content = new StringContent(jsonInfo, Encoding.UTF8, "application/json");
            var responseMessage = await httpClient.PostAsync("https://localhost:5190/api/Auth/changepassword", content);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var apiDataResponse = JsonConvert.DeserializeObject<ApiDataResponse<ChangePassword>>(responseContent);

                var response = new
                {
                    Success = true,
                    Message = apiDataResponse.Message,
                    Url = "/"
                };

                return Json(response);
            }
            else
            {
                var response = new
                {
                    Message = "Şifre Güncellenemedi , lütfen tekrar deneyin",
                };
                return Json(response);
            }

        }

        private async Task<ApiDataResponse<UserImage>> GetUpdateUserImageResponseMessage(HttpResponseMessage responseMessage)
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiDataResponse<UserImage>>(responseContent);
        }

        private async Task<ApiDataResponse<UserDto>> GetUpdateUserResponseMessage(HttpResponseMessage responseMessage)
        {
            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiDataResponse<UserDto>>(responseContent);
        }
    }
}