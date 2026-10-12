using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class MyEnrolmentVm
    {
        public int EnrolmentId { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime EnrolledAt { get; set; }
    }

    public class EnrolmentsController : Controller
    {
        private readonly ApiClient _api;

        public EnrolmentsController(ApiClient api)
        {
            _api = api;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
                return RedirectToAction("Login", "Account");

            var res = await _api.PostAsync($"api/events/{eventId}/enrolments", new { categoryId });

            if (res.IsSuccessStatusCode)
            {
                TempData["Success"] = "You're enrolled! Your enrolment is pending confirmation.";
                return RedirectToAction(nameof(Mine));
            }

            TempData["Error"] = await ApiClient.ReadMessageAsync(res, "Could not enrol. Please try again.");
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        [HttpGet]
        public IActionResult Index() => RedirectToAction(nameof(Mine));

        [HttpGet]
        public async Task<IActionResult> Mine()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
                return RedirectToAction("Login", "Account");

            var res = await _api.GetAsync("api/enrolments/me");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await ApiClient.ReadMessageAsync(res, "Could not load your enrolments.");
                return RedirectToAction("Index", "Events");
            }

            var list = await res.Content.ReadFromJsonAsync<List<MyEnrolmentVm>>() ?? new();
            return View(list);
        }
    }
}