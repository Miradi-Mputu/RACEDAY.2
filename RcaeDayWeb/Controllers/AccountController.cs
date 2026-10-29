using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Services;
using RcaeDayWeb.ViewModels;

namespace RcaeDayWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (isSuccess, role) = await _apiService.LoginAsync(new
            {
                email = model.Email,
                password = model.Password
            });

            if (isSuccess)
            {
                // Save the role in the MVC session so we can build the menu
                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetString("UserEmail", model.Email);

                // Redirect based on role
                if (role == "Organiser")
                    return RedirectToAction("Dashboard", "Organiser");
                else
                    return RedirectToAction("Index", "Participant");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var success = await _apiService.PostAsync("api/Auth/register", new
            {
                fullName = model.FullName,
                email = model.Email,
                password = model.Password,
                role = model.Role
            });

            if (success)
            {
                TempData["SuccessMessage"] = "Account created! Please log in.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Registration failed. Email might already be in use.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _apiService.PostAsync("api/Auth/logout", new { });
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}