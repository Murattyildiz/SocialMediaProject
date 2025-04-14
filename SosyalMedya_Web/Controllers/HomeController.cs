using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Diagnostics;

namespace SosyalMedya_Web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "admin,user")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:5190/api/Articles/getarticlewithdetails");
            if (responseMessage.IsSuccessStatusCode)
            {
                ViewData["UserId"] = HttpContext.Session.GetInt32("userId");
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                var apiDataResponse = JsonConvert.DeserializeObject<ApiListDataResponse<ArticleDetail>>(jsonResponse);

                return apiDataResponse.Success ? View(apiDataResponse.Data) : (IActionResult)View("Veri gelmiyor");
            }
            return View("Veri Gelmiyor");
        }
    }
}