using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;

namespace SosyalMedya_Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Default constructor for controllers that don't inject IHttpClientFactory
        public BaseController()
        {
        }

        public BaseController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    // Get user details for the layout
                    await SetUserViewData(userId.Value);
                }
            }

            // Execute the action
            await next();
        }

        // Helper method to get the JWT token from the session
        protected string GetToken()
        {
            return HttpContext.Session.GetString("Token");
        }

        // Helper method to get the current user's ID
        protected int GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return userId.HasValue ? userId.Value : 0;
        }

        protected async Task SetUserViewData(int userId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Get user details
                var userResponse = await httpClient.GetAsync($"https://localhost:5190/api/Users/getbyid?id={userId}");
                if (userResponse.IsSuccessStatusCode)
                {
                    var userJson = await userResponse.Content.ReadAsStringAsync();
                    var userApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<UserDto>>(userJson);

                    if (userApiResponse.Success && userApiResponse.Data != null)
                    {
                        // Hem orijinal anahtarları hem de yeni anahtarları doldur
                        // Orijinal anahtarlar
                        ViewData["UserName"] = $"{userApiResponse.Data.FirstName} {userApiResponse.Data.LastName}";
                        ViewData["UserImage"] = string.IsNullOrEmpty(userApiResponse.Data.ImagePath)
                            ? "/frontend/assets/images/testLogo.jpg"
                            : $"https://localhost:5190/{userApiResponse.Data.ImagePath}";

                        // Yeni anahtarlar
                        ViewData["CurrentUserName"] = $"{userApiResponse.Data.FirstName} {userApiResponse.Data.LastName}";
                        ViewData["CurrentUserImage"] = string.IsNullOrEmpty(userApiResponse.Data.ImagePath)
                            ? "/frontend/assets/images/testLogo.jpg"
                            : $"https://localhost:5190/{userApiResponse.Data.ImagePath}";

                        // Get article count
                        var articlesResponse = await httpClient.GetAsync($"https://localhost:5190/api/Articles/getarticlewithdetailsbyuserid?id={userId}");
                        if (articlesResponse.IsSuccessStatusCode)
                        {
                            var articlesJson = await articlesResponse.Content.ReadAsStringAsync();
                            var articlesApiResponse = JsonConvert.DeserializeObject<ApiListDataResponse<ArticleDetail>>(articlesJson);

                            if (articlesApiResponse.Success && articlesApiResponse.Data != null)
                            {
                                ViewData["MyArticle"] = articlesApiResponse.Data.Count;
                                ViewData["CurrentUserArticleCount"] = articlesApiResponse.Data.Count;
                            }
                            else
                            {
                                ViewData["MyArticle"] = 0;
                                ViewData["CurrentUserArticleCount"] = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Set default values if error occurs
                ViewData["UserName"] = "Kullanıcı";
                ViewData["UserImage"] = "/frontend/assets/images/testLogo.jpg";
                ViewData["MyArticle"] = 0;

                ViewData["CurrentUserName"] = "Kullanıcı";
                ViewData["CurrentUserImage"] = "/frontend/assets/images/testLogo.jpg";
                ViewData["CurrentUserArticleCount"] = 0;

                // Log the error
                Console.WriteLine($"Error setting user ViewData: {ex.Message}");
            }
        }
    }
}