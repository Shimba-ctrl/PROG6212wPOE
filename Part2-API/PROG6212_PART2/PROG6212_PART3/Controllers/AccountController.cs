
using Microsoft.AspNetCore.Mvc;

using System.Net.Http.Json;
using PROG6212_PART3.Models;
using PROG6212_PART3.Services;

namespace PROG6212_PART3.Controllers
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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var registrationData = new
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role
            };

            var response = await _apiService.SendAsync(
                HttpMethod.Post,
                "api/Auth/register",
                registrationData);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. This email may already be registered.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Your account has been created successfully. Please log in.";

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.SendAsync(
                HttpMethod.Post,
                "api/Auth/login",
                model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password. Please try again.");

                return View(model);
            }

            var loginResult = await response.Content
                .ReadFromJsonAsync<LoginResponse>();

            if (loginResult?.User == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to process the login response.");

                return View(model);
            }

            HttpContext.Session.SetInt32(
                "UserId", loginResult.User.UserId);

            HttpContext.Session.SetString(
                "UserRole", loginResult.User.Role);

            HttpContext.Session.SetString(
                "FullName", loginResult.User.FullName);

            if (loginResult.User.Role.Equals(
                "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Organiser");
            }

            return RedirectToAction("Index", "Participant");
        }
    }

    public class LoginResponse
    {
        public LoginUser? User { get; set; }
    }

    public class LoginUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
