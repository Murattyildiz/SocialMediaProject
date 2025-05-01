using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;

namespace SosyalMedya_Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Users(string searchTerm)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.CurrentUserId = currentUserId;
            var httpClient = _httpClientFactory.CreateClient();
            var users = new List<UserListViewModel>();

            // Get all users from API
            var response = await httpClient.GetAsync("https://localhost:5190/api/Users/getalldto");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiDataResponse<List<UserDto>>>(jsonResponse);
                
                if (apiResponse.Success && apiResponse.Data != null)
                {
                    var filteredUsers = apiResponse.Data.Where(u => u.Id != currentUserId);
                    
                    // Apply search filter if searchTerm is provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        searchTerm = searchTerm.ToLower().Trim();
                        filteredUsers = filteredUsers.Where(u =>
                            u.FirstName.ToLower().Contains(searchTerm) ||
                            u.LastName.ToLower().Contains(searchTerm) ||
                            $"{u.FirstName} {u.LastName}".ToLower().Contains(searchTerm)
                        );
                    }

                    // Get follow status for each user
                    var followList = new Dictionary<int, bool>();
                    foreach (var user in filteredUsers)
                    {
                        var followResponse = await httpClient.GetAsync($"https://localhost:5190/api/UserFollow/isfollowing?followerId={currentUserId}&followedId={user.Id}");
                        bool isFollowing = false;
                        
                        if (followResponse.IsSuccessStatusCode)
                        {
                            var followJson = await followResponse.Content.ReadAsStringAsync();
                            var followApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<bool>>(followJson);
                            isFollowing = followApiResponse.Success && followApiResponse.Data;
                        }

                        users.Add(new UserListViewModel
                        {
                            Id = user.Id,
                            FullName = $"{user.FirstName} {user.LastName}",
                            ImagePath = string.IsNullOrEmpty(user.ImagePath) ? "default.jpg" : user.ImagePath,
                            IsFollowing = isFollowing
                        });
                    }
                }
            }

            ViewBag.SearchTerm = searchTerm;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Follow(int followedId)
        {
            var followerId = HttpContext.Session.GetInt32("UserId");
            if (followerId == null)
            {
                return Json(new { success = false, message = "Oturum bulunamadı" });
            }

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync($"https://localhost:5190/api/UserFollow/follow?followerId={followerId}&followedId={followedId}", null);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
                return Json(new { success = result.Success });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> Unfollow(int followedId)
        {
            var followerId = HttpContext.Session.GetInt32("UserId");
            if (followerId == null)
            {
                return Json(new { success = false, message = "Oturum bulunamadı" });
            }

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync($"https://localhost:5190/api/UserFollow/unfollow?followerId={followerId}&followedId={followedId}", null);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
                return Json(new { success = result.Success });
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowCounts(int userId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var followersResponse = await httpClient.GetAsync($"https://localhost:5190/api/UserFollow/getfollowercount?userId={userId}");
            var followingResponse = await httpClient.GetAsync($"https://localhost:5190/api/UserFollow/getfollowingcount?userId={userId}");

            if (followersResponse.IsSuccessStatusCode && followingResponse.IsSuccessStatusCode)
            {
                var followerCount = JsonConvert.DeserializeObject<ApiDataResponse<int>>(await followersResponse.Content.ReadAsStringAsync()).Data;
                var followingCount = JsonConvert.DeserializeObject<ApiDataResponse<int>>(await followingResponse.Content.ReadAsStringAsync()).Data;

                return Json(new { success = true, followerCount, followingCount });
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://localhost:5190/api/UserFollow/getfollowers?userId={userId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiListDataResponse<UserFollowDto>>(jsonResponse);
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowing(int userId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://localhost:5190/api/UserFollow/getfollowing?userId={userId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiListDataResponse<UserFollowDto>>(jsonResponse);
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false });
        }
    }
} 