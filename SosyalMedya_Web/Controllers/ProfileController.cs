using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Globalization;

namespace SosyalMedya_Web.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileController(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
            var userResponse = await httpClient.GetAsync($"https://localhost:5190/api/Users/getbyid?id={userId}");
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
                ? "https://localhost:5190/images/default.jpg" 
                : $"https://localhost:5190/{userApiResponse.Data.ImagePath}";
            ViewData["UserRegistrationDate"] = "-";

            // Get follow status if not own profile
            if (!isOwnProfile && currentUserId.HasValue)
            {
                var followResponse = await httpClient.GetAsync($"https://localhost:5190/api/UserFollow/isfollowing?followerId={currentUserId}&followedId={userId}");
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
            var articlesResponse = await httpClient.GetAsync($"https://localhost:5190/api/Articles/getarticlewithdetailsbyuserid?id={userId}");
            if (articlesResponse.IsSuccessStatusCode)
            {
                var jsonResponse = await articlesResponse.Content.ReadAsStringAsync();
                var apiDataResponse = JsonConvert.DeserializeObject<ApiListDataResponse<ArticleDetail>>(jsonResponse);
                
                if (apiDataResponse.Success && apiDataResponse.Data != null)
                {
                    // Paylaşımları en yeni tarihten eskiye doğru sırala
                    var sortedArticles = apiDataResponse.Data
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
                    
                    return View("Profile", sortedArticles);
                }
                
                return View("Profile", new List<ArticleDetail>());
            }

            return View("Profile", new List<ArticleDetail>());
        }
    }
}