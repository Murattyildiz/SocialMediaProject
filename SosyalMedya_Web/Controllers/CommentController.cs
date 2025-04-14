using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SosyalMedya_Web.Controllers
{
    public class CommentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CommentController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Roles = "admin,user")]
        [HttpPost("post-comment")]
        public async Task<IActionResult> Comment(Comment comment)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonComment = JsonConvert.SerializeObject(comment);
            var content = new StringContent(jsonComment, Encoding.UTF8, "application/json");
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responseMessage = await httpClient.PostAsync("https://localhost:5190/api/Comments/add", content);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "admin,user")]
        [HttpPost("delete-comment")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var httpClient = _httpClientFactory.CreateClient();
            //var jsonArticle = JsonConvert.SerializeObject(article);
            //var content = new StringContent(jsonArticle, Encoding.UTF8, "application/json");
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responseMessage = await httpClient.DeleteAsync("https://localhost:5190/api/Comments/delete?id=" + id);
            if (responseMessage.IsSuccessStatusCode)
            {

                TempData["Message"] = "Yorum Silindi";
                TempData["Success"] = true;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [Authorize(Roles = "admin,user")]
        [HttpGet("notification")]
        public async Task<IActionResult> Notification()
        {

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            ViewData["MyArticle"] = HttpContext.Session.GetInt32("MyArticle");
            ViewData["UserId"] = userId;
            var responseMessage = await _httpClientFactory.CreateClient().GetAsync("https://localhost:5190/api/Articles/getarticlewithdetailsbyuserid?id=" + userId);
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();

                var apiDataResponse = JsonConvert.DeserializeObject<ApiListDataResponse<ArticleDetail>>(jsonResponse);
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");

                return apiDataResponse.Success ? View(apiDataResponse.Data) : RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home");
        }
    }

}