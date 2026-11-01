using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
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

    [SessionAuthorize("Participant")]
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