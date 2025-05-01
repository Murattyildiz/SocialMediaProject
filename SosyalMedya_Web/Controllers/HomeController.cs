using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Diagnostics;
using System.Linq;
using System.Globalization;

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
                ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
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
                    
                    return View(sortedArticles);
                }
                return View("Veri gelmiyor");
            }
            return View("Veri Gelmiyor");
        }
    }
}