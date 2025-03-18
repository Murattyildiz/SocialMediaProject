using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Net.Http.Headers;
using System.Text;

namespace SosyalMedya_Web.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ArticleController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [Authorize(Roles = "admin,user")]
        [HttpPost("share-content")]
        public async Task<IActionResult> SharingContent(Article article)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonArticle = JsonConvert.SerializeObject(article);
            var content = new StringContent(jsonArticle, Encoding.UTF8, "application/json");
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responseMessage = await httpClient.PostAsync("https://localhost:5190/api/Articles/add", content);
            if (responseMessage.IsSuccessStatusCode)
            {
                var sharedResponse = await GetSharedResponse(responseMessage);
                TempData["Message"] = sharedResponse.Message;
                TempData["Success"] = sharedResponse.Success;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [Authorize(Roles = "admin,user")]
        [HttpPost("delete-article")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var httpClient = _httpClientFactory.CreateClient();
            //var jsonArticle = JsonConvert.SerializeObject(article);
            //var content = new StringContent(jsonArticle, Encoding.UTF8, "application/json");
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responseMessage = await httpClient. DeleteAsync("https://localhost:5190/api/Articles/delete?id="+id);
            if (responseMessage.IsSuccessStatusCode)
            {
                var sharedResponse = await GetSharedResponse(responseMessage);
                TempData["Message"] = sharedResponse.Message;
                TempData["Success"] = sharedResponse.Success;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [Authorize(Roles = "admin,user")]
        [HttpPost("update-content")]
        public async Task<IActionResult> UpdateContent(Article article)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonArticle = JsonConvert.SerializeObject(article);
            var content = new StringContent(jsonArticle, Encoding.UTF8, "application/json");
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responseMessage = await httpClient.PostAsync("https://localhost:5190/api/Articles/update", content);
            if (responseMessage.IsSuccessStatusCode)
            {
                var sharedResponse = await GetSharedResponse(responseMessage);
                TempData["Message"] = sharedResponse.Message;
                TempData["Success"] = sharedResponse.Success;
                return RedirectToAction("Index", "Home");

            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "admin,user")]
        [HttpGet("getarticlebyid")]
        public async Task<IActionResult> GetUpdateArticle(int id)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:5190/api/Articles/getbyid?id=" + id);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ApiDataResponse<Article>>(responseContent);

                var responseMessage1 = await httpClient.GetAsync("https://localhost:5190/api/Topics/getall");
                if (responseMessage1.IsSuccessStatusCode)
                {
                    var jsonResponse1 = await responseMessage1.Content.ReadAsStringAsync();
                    var apiDataResponse = JsonConvert.DeserializeObject<ApiListDataResponse<Topics>>(jsonResponse1);

                    var response = new ArticleTopicsResponse
                    {
                        Article = data.Data,
                        Topics = apiDataResponse.Data.Where(x => x.Status == true).ToList()
                    };

                    return Json(response);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        private async Task<ApiDataResponse<Article>> GetSharedResponse(HttpResponseMessage responseMessage)
        {
           var resonsecontent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiDataResponse<Article>>(resonsecontent);
        }
    }
}
