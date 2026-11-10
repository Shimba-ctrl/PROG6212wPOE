

using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using PROG6212_PART3.Models;
using PROG6212_PART3.Services;

namespace PROG6212_PART3.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiService _apiService;

        public EventsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserId") != null &&
                   string.Equals(
                       HttpContext.Session.GetString("UserRole"),
                       "Organiser",
                       StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Get, "api/Events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to load events.";
                return View(new List<EventViewModel>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            return View(events ?? new List<EventViewModel>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var response = await _apiService.SendAsync(
                HttpMethod.Post, "api/Events", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty, "Could not create the event.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Event created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Get, $"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var eventItem = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
                return RedirectToAction("Index");

            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, EventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var response = await _apiService.SendAsync(
                HttpMethod.Put, $"api/Events/{id}", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Could not update the event. You can only edit your own events.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Event updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Delete, $"api/Events/{id}");

            TempData["SuccessMessage"] =
                response.IsSuccessStatusCode
                ? "Event deleted successfully!"
                : "Unable to delete this event.";

            return RedirectToAction("Index");
        }
    }
}
