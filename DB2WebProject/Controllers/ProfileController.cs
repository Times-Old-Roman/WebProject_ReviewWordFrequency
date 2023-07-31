using Microsoft.AspNetCore.Mvc;

namespace DB2WebProject.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }
    }
}
