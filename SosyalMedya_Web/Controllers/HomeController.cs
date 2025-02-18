using Microsoft.AspNetCore.Mvc;
using SosyalMedya_Web.Models;
using System.Diagnostics;

namespace SosyalMedya_Web.Controllers
{
    public class HomeController : Controller
    {
       public IActionResult Index()
        {
            return View();
        }
    }
}
