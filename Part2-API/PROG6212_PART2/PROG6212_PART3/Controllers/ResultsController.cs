
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using PROG6212_PART3.Models;
using PROG6212_PART3.Services;

namespace PROG6212_PART3.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ApiService _apiService;

        public ResultsController(ApiService apiService)
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

        private bool IsParticipant()
        {
            return HttpContext.Session.GetInt32("UserId") != null &&
                string.Equals(
                    HttpContext.Session.GetString("UserRole"),
                    "Participant",
                    StringComparison.OrdinalIgnoreCase);
        }

        // Organiser results management page
        public async Task<IActionResult> Manage()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var enrolmentResponse = await _apiService.SendAsync(
                HttpMethod.Get, "api/Enrolments");

            var resultResponse = await _apiService.SendAsync(
                HttpMethod.Get, "api/Results");

            if (enrolmentResponse.StatusCode == HttpStatusCode.Unauthorized ||
                resultResponse.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!enrolmentResponse.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to load event enrolments.";
                return View(new List<EnrolmentViewModel>());
            }

            var enrolments = await enrolmentResponse.Content
                .ReadFromJsonAsync<List<EnrolmentViewModel>>();

            var results = resultResponse.IsSuccessStatusCode
                ? await resultResponse.Content.ReadFromJsonAsync<List<ResultViewModel>>()
                : new List<ResultViewModel>();

            ViewBag.Results = results ?? new List<ResultViewModel>();

            return View(enrolments ?? new List<EnrolmentViewModel>());
        }

        // Save a participant's result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int enrolmentId, string finishTime, int? position)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!TimeSpan.TryParse(finishTime, out TimeSpan parsedTime) ||
                parsedTime < TimeSpan.Zero ||
                (position.HasValue && position.Value <= 0))
            {
                TempData["ErrorMessage"] = "Please enter a valid finish time and position.";
                return RedirectToAction("Manage");
            }

            var response = await _apiService.SendAsync(
                HttpMethod.Post,
                "api/Results",
                new
                {
                    EnrolmentId = enrolmentId,
                    FinishTime = parsedTime,
                    Position = position
                });

            TempData[response.IsSuccessStatusCode
                ? "SuccessMessage"
                : "ErrorMessage"] = response.IsSuccessStatusCode
                ? "Result recorded successfully."
                : "Unable to record the result. It may already exist or the enrolment may not belong to you.";

            return RedirectToAction("Manage");
        }

        // Participant's personal results page
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.SendAsync(
                HttpMethod.Get, "api/Results");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to load your results.";
                return View(new List<ResultViewModel>());
            }

            var results = await response.Content
                .ReadFromJsonAsync<List<ResultViewModel>>();

            return View(results ?? new List<ResultViewModel>());
        }
    }
}
