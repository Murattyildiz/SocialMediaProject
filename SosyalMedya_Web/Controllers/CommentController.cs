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
        public async Task<IActionResult> Comment([FromBody] Comment comment)
        {
            try
            {
                // Kullanıcı bilgilerini session'dan al
                var userId = HttpContext.Session.GetInt32("UserId");
                var userName = HttpContext.Session.GetString("UserName");
                
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın." });
                }

                if (comment == null || comment.ArticleId <= 0 || string.IsNullOrEmpty(comment.CommentText))
                {
                    return Json(new { success = false, message = "Geçersiz yorum verisi." });
                }

                // Yorum nesnesini oluştur
                var commentToAdd = new
                {
                    ArticleId = comment.ArticleId,
                    UserId = userId.Value,
                    CommentText = comment.CommentText.Trim(),
                    CommentDate = DateTime.Now,
                    Status = true
                };

                var httpClient = _httpClientFactory.CreateClient();
                var jsonComment = JsonConvert.SerializeObject(commentToAdd);
                var content = new StringContent(jsonComment, Encoding.UTF8, "application/json");
                var token = HttpContext.Session.GetString("Token");

                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın." });
                }

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var responseMessage = await httpClient.PostAsync("https://localhost:5190/api/Comments/add", content);
                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.IsSuccessStatusCode)
                {
                    var userImage = HttpContext.Session.GetString("UserImage") ?? "images/default.jpg";
                    
                    return Json(new { 
                        success = true, 
                        userName = userName,
                        userImage = userImage,
                        commentDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                        message = "Yorum başarıyla eklendi."
                    });
                }
                
                return Json(new { 
                    success = false, 
                    message = "Yorum eklenirken bir hata oluştu: " + responseContent 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Yorum eklenirken bir hata oluştu: " + ex.Message 
                });
            }
        }

        [Authorize(Roles = "admin,user")]
        [HttpPost("delete-comment")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "Geçersiz yorum ID'si." });
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın." });
                }

                var httpClient = _httpClientFactory.CreateClient();
                var token = HttpContext.Session.GetString("Token");
                
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın." });
                }

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                // Yorumu silme işlemini gerçekleştir
                var responseMessage = await httpClient.DeleteAsync($"https://localhost:5190/api/Comments/delete?id={id}");
                
                if (responseMessage.IsSuccessStatusCode)
                {
                    // Silme işlemi başarılı olduktan sonra cache'i temizle
                    var clearCacheResponse = await httpClient.PostAsync("https://localhost:5190/api/Comments/clearcache", null);
                    
                    return Json(new { 
                        success = true, 
                        message = "Yorum başarıyla silindi.",
                        requiresRefresh = true 
                    });
                }

                var errorContent = await responseMessage.Content.ReadAsStringAsync();
                return Json(new { success = false, message = $"Yorum silinirken bir hata oluştu: {errorContent}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Yorum silinirken bir hata oluştu: {ex.Message}" });
            }
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