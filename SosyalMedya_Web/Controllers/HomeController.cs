using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Diagnostics;
using System.Linq;
using System.Globalization;

namespace SosyalMedya_Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Roles = "admin,user")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync("https://localhost:5190/api/Articles/getarticlewithdetails");
            if (responseMessage.IsSuccessStatusCode)
            {
                ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                var apiDataResponse = JsonConvert.DeserializeObject<ApiListDataResponse<ArticleDetail>>(jsonResponse);

                if (apiDataResponse.Success && apiDataResponse.Data != null)
                {
               
                    var sortedArticles = apiDataResponse.Data
                        .OrderByDescending(a => {
                         
                            if (DateTime.TryParse(a.SharingDate, CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out DateTime parsedDate))
                            {
                                return parsedDate;
                            }
                       
                            return DateTime.MinValue;
                        })
                        .ToList();
                    
                    return View(sortedArticles);
                }
                return View("Veri gelmiyor");
            }
            return View("Veri Gelmiyor");
        }
    }
}