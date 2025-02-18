using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Diagnostics;

namespace SosyalMedya_Web.Controllers
{
    public class HomeController : Controller
    {
       public async Task<IActionResult> Index()
        {
            var httpClient= new HttpClient();
            var responseMessage=await httpClient.GetAsync("https://localhost:5190/api/Articles/getarticlewithdetails");
            if(responseMessage.IsSuccessStatusCode)
            {
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                var apiDataResponse = JsonConvert.DeserializeObject<ApiDataResponse<ArticleDetail>>(jsonResponse);

                return apiDataResponse.Success ? View(apiDataResponse.Data) : (IActionResult)View("Veri gelmiyor");
            }
            return View("Veri Gelmiyor");
        }
    }
}
