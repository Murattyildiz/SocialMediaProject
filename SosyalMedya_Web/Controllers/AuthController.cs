using Microsoft.AspNetCore.Mvc;

namespace SosyalMedya_Web.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet("giris-yap")]
        public IActionResult Login()
        {
            return View();
        }
    }
}
