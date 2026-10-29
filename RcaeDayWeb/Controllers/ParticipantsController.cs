using Microsoft.AspNetCore.Mvc;

namespace RcaeDayWeb.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index()
        {
            // Check if the user is logged in and is a Participant
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}