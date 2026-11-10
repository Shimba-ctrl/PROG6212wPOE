using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using PROG6212_PART3.Models;
using PROG6212_PART3.Services;

namespace PROG6212_PART3.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;

        public ParticipantController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetInt32("UserId") != null &&
                   string.Equals(
                       HttpContext.Session.GetString("UserRole"),
                       "Participant",
                       StringComparison.OrdinalIgnoreCase);
        }

        public IActionResult Index()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            ViewBag.FullName = HttpContext.Session.GetString("FullName");

            return View();
        }

        // Display events and their categories
        public async Task<IActionResult> Events()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var eventResponse = await _apiService.SendAsync(
                HttpMethod.Get, "api/Events");

            if (eventResponse.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!eventResponse.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to load events.";
                return View(new List<EventViewModel>());
            }

            var events = await eventResponse.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            var categoryResponse = await _apiService.SendAsync(
                HttpMethod.Get, "api/Categories");

            if (categoryResponse.IsSuccessStatusCode)
            {
                var categories = await categoryResponse.Content
                    .ReadFromJsonAsync<List<CategoryViewModel>>();

                ViewBag.Categories = categories ?? new List<CategoryViewModel>();
            }
            else
            {
                ViewBag.Categories = new List<CategoryViewModel>();
                ViewBag.CategoryError = "Unable to load event categories.";
            }

            return View(events ?? new List<EventViewModel>());
        }

        // Enrol in a category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Post,
                "api/Enrolments",
                new { CategoryId = categoryId });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "You have successfully enrolled!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                if (error.Contains("already enrolled",
                    StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] =
                        "You are already enrolled in this category.";
                }
                else if (error.Contains("participant limit",
                    StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] =
                        "Sorry, this category is full.";
                }
                else if (error.Contains("already taken place",
                    StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] =
                        "You cannot enrol in an event that has already taken place.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Unable to enrol. Please try again.";
                }
            }

            return RedirectToAction("Events");
        }

        // Display the participant's enrolments
        public async Task<IActionResult> Enrolments()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Get, "api/Enrolments");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to load your enrolments.";
                return View(new List<EnrolmentViewModel>());
            }

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<EnrolmentViewModel>>();

            return View(enrolments ?? new List<EnrolmentViewModel>());
        }

        // Cancel an enrolment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Put,
                $"api/Enrolments/{id}/cancel");

            TempData[response.IsSuccessStatusCode
                ? "SuccessMessage"
                : "ErrorMessage"] = response.IsSuccessStatusCode
                ? "Your enrolment has been cancelled."
                : "Unable to cancel this enrolment.";

            return RedirectToAction("Enrolments");
        }
    }
}
