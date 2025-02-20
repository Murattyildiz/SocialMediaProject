using Microsoft.AspNetCore.Mvc;

namespace SosyalMedya_Web.Controllers
{
    public class SettingController : Controller
    {
        [HttpGet("Hesap-bilgilerim")]
        public IActionResult AccountSetting()
        {
            return View();
        }
    }
}
