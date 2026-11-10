
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using PROG6212_PART3.Models;
using PROG6212_PART3.Services;

namespace PROG6212_PART3.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApiService _apiService;

        public CategoriesController(ApiService apiService)
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

        // Display categories
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Get, "api/Categories");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to load categories.";
                return View(new List<CategoryViewModel>());
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            return View(categories ?? new List<CategoryViewModel>());
        }

        // Open create page
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            await LoadEvents();

            return View();
        }

        // Save new category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                await LoadEvents();
                return View(model);
            }

            var response = await _apiService.SendAsync(
                HttpMethod.Post,
                "api/Categories",
                new
                {
                    model.EventId,
                    model.Name,
                    model.Distance,
                    model.MaxParticipants
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create the category. Check the details and try again.");

                await LoadEvents();
                return View(model);
            }

            TempData["SuccessMessage"] = "Category created successfully!";
            return RedirectToAction("Index");
        }

        // Open edit page
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Get, $"api/Categories/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var category = await response.Content
                .ReadFromJsonAsync<CategoryViewModel>();

            if (category == null)
                return RedirectToAction("Index");

            return View(category);
        }

        // Save category changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var response = await _apiService.SendAsync(
                HttpMethod.Put,
                $"api/Categories/{id}",
                new
                {
                    model.Name,
                    model.Distance,
                    model.MaxParticipants
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update this category.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction("Index");
        }

        // Delete category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Delete, $"api/Categories/{id}");

            TempData[response.IsSuccessStatusCode
                ? "SuccessMessage"
                : "ErrorMessage"] = response.IsSuccessStatusCode
                ? "Category deleted successfully!"
                : "Unable to delete this category.";

            return RedirectToAction("Index");
        }

        // Load events for the category creation dropdown
        private async Task LoadEvents()
        {
            var response = await _apiService.SendAsync(
                HttpMethod.Get, "api/Events");

            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                ViewBag.Events = events ?? new List<EventViewModel>();
            }
            else
            {
                ViewBag.Events = new List<EventViewModel>();
            }
        }
    }
}
