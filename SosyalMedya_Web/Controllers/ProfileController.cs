using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Globalization;
using System.Net.Http.Headers;

namespace SosyalMedya_Web.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;

        public ProfileController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiUrl = _configuration["ApiUrl"] ?? "https://localhost:5190"; // Default API URL if not found in configuration
        }

        [Authorize(Roles = "admin,user")]
        [HttpGet("profilim")]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return await GetProfileView(userId.Value, true);
        }

        [Authorize(Roles = "admin,user")]
        [HttpGet("profil/{id}")]
        public async Task<IActionResult> UserProfile(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (id == currentUserId)
            {
                return RedirectToAction("Profile");
            }
            return await GetProfileView(id, false);
        }

        private async Task<IActionResult> GetProfileView(int userId, bool isOwnProfile)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            ViewData["CurrentUserId"] = currentUserId;
            ViewData["UserId"] = userId;

            var httpClient = _httpClientFactory.CreateClient();

            // Get user details
            var userResponse = await httpClient.GetAsync($"{_apiUrl}/api/Users/getbyid?id={userId}");
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<UserDto>>(userJson);
            if (!userApiResponse.Success)
            {
                return NotFound();
            }

            // Profil sayfasında görüntülenecek kullanıcı bilgileri
            ViewData["UserName"] = $"{userApiResponse.Data.FirstName} {userApiResponse.Data.LastName}";
            ViewData["UserImage"] = string.IsNullOrEmpty(userApiResponse.Data.ImagePath)
                ? "/frontend/assets/images/testLogo.jpg"
                : $"{_apiUrl}/{userApiResponse.Data.ImagePath}";
            ViewData["UserRegistrationDate"] = "-";

            // Get follow status if not own profile
            if (!isOwnProfile && currentUserId.HasValue)
            {
                var followResponse = await httpClient.GetAsync($"{_apiUrl}/api/UserFollow/isfollowing?followerId={currentUserId}&followedId={userId}");
                if (followResponse.IsSuccessStatusCode)
                {
                    var followJson = await followResponse.Content.ReadAsStringAsync();
                    var followApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<bool>>(followJson);
                    ViewBag.IsFollowing = followApiResponse.Success && followApiResponse.Data;
                }
                else
                {
                    ViewBag.IsFollowing = false;
                }
            }
            else
            {
                ViewBag.IsFollowing = false;
            }

            // Get user's articles
            var articlesResponse = await httpClient.GetAsync($"{_apiUrl}/api/Articles/getarticlewithdetailsbyuserid?id={userId}");
            List<ArticleDetail> articles = new List<ArticleDetail>();
            
            if (articlesResponse.IsSuccessStatusCode)
            {
                var jsonResponse = await articlesResponse.Content.ReadAsStringAsync();
                var apiDataResponse = JsonConvert.DeserializeObject<ApiListDataResponse<ArticleDetail>>(jsonResponse);

                if (apiDataResponse.Success && apiDataResponse.Data != null)
                {
                    // Paylaşımları en yeni tarihten eskiye doğru sırala
                    articles = apiDataResponse.Data
                        .OrderByDescending(a => {
                            // Esnek bir şekilde tarihi parse etmeye çalış
                            if (DateTime.TryParse(a.SharingDate, CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out DateTime parsedDate))
                            {
                                return parsedDate;
                            }
                            // Parse edilemezse varsayılan değer döndür
                            return DateTime.MinValue;
                        })
                        .ToList();
                }
            }
            
            // Get user's code shares
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            var codeSharesResponse = await httpClient.GetAsync($"{_apiUrl}/api/CodeShares/getbyuserid?userId={userId}");
            List<CodeShareViewModel> codeShares = new List<CodeShareViewModel>();
            
            if (codeSharesResponse.IsSuccessStatusCode)
            {
                var jsonResponse = await codeSharesResponse.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
                
                if (apiResponse.Success)
                {
                    var jsonSettings = new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Local,
                        DateParseHandling = DateParseHandling.DateTime
                    };
                    
                    codeShares = JsonConvert.DeserializeObject<List<CodeShareViewModel>>(
                        apiResponse.Data.ToString(), jsonSettings);
                }
            }
            
            // Create the profile view model
            var profileViewModel = new ProfileViewModel
            {
                Articles = articles,
                CodeShares = codeShares
            };

            return View("Profile", profileViewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetUserCodeShares(int userId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var response = await httpClient.GetAsync($"{_apiUrl}/api/CodeShares/getbyuserid?userId={userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
                    
                    if (apiResponse.Success)
                    {
                        var jsonSettings = new JsonSerializerSettings
                        {
                            DateFormatHandling = DateFormatHandling.IsoDateFormat,
                            DateTimeZoneHandling = DateTimeZoneHandling.Local,
                            DateParseHandling = DateParseHandling.DateTime
                        };
                        
                        var codeShares = JsonConvert.DeserializeObject<List<CodeShareViewModel>>(
                            apiResponse.Data.ToString(), jsonSettings);
                        
                        return Json(new { success = true, data = codeShares });
                    }
                }
                
                return Json(new { success = false, message = "Kod paylaşımları yüklenirken bir hata oluştu." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
    }
}