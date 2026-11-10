
using Microsoft.AspNetCore.Mvc;

namespace PROG6212_PART3.Controllers
{
    public class OrganiserController : Controller
    {
        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null ||
                !string.Equals(role, "Organiser",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.FullName =
                HttpContext.Session.GetString("FullName");

            return View();
        }
    }
}

