using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Services;
using RcaeDayWeb.Services;

namespace RcaeDayWeb.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Dashboard()
        {
            // Check if the user is logged in and is an Organiser
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}