using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;
        public OrganiserController(ApiClient api) => _api = api;

        // ---------- Dashboard ----------
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var res = await _api.GetAsync("api/events/mine");
            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Error = await ApiClient.ReadMessageAsync(res, "Could not load your dashboard. Please try again.");
                return View(new List<MyEventItem>());
            }

            var list = await res.Content.ReadFromJsonAsync<List<MyEventItem>>() ?? new();
            return View(list.OrderBy(e => e.EventDate).ToList());
        }

        // ---------- Categories ----------
        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            var res = await _api.GetAsync($"api/events/{eventId}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var ev = await res.Content.ReadFromJsonAsync<EventDetails>();
            if (ev == null) return NotFound();

            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int eventId, string? categoryName, string? description)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["Error"] = "Please enter a category name.";
                return RedirectToAction(nameof(Categories), new { eventId });
            }
            if (categoryName.Trim().Length > 50)
            {
                TempData["Error"] = "Category name must be 50 characters or fewer.";
                return RedirectToAction(nameof(Categories), new { eventId });
            }

            var res = await _api.PostAsync($"api/events/{eventId}/categories", new
            {
                categoryName = categoryName.Trim(),
                description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
            });

            if (res.IsSuccessStatusCode)
                TempData["Success"] = "Category added.";
            else
                TempData["Error"] = await ApiClient.ReadMessageAsync(res, "Could not add the category. Please try again.");

            return RedirectToAction(nameof(Categories), new { eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int eventId, int id)
        {
            var res = await _api.DeleteAsync($"api/categories/{id}");
            if (res.IsSuccessStatusCode)
                TempData["Success"] = "Category removed.";
            else
                TempData["Error"] = await ApiClient.ReadMessageAsync(res, "Could not remove this category. It may already have enrolments.");

            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // ---------- Enrolments ----------
        [HttpGet]
        public async Task<IActionResult> Enrolments(int eventId)
        {
            var (vm, error) = await LoadEnrolmentsAsync(eventId);
            if (vm == null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Index", "ManageEvents");
            }
            return View(vm);
        }

        // ---------- Results entry ----------
        [HttpGet]
        public async Task<IActionResult> Results(int eventId)
        {
            var (vm, error) = await LoadEnrolmentsAsync(eventId);
            if (vm == null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Index", "ManageEvents");
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(int eventId, int enrolmentId, string? finishTime, int? finishPosition)
        {
            if (!TimeSpan.TryParse(finishTime, out var time) || time <= TimeSpan.Zero)
            {
                TempData["Error"] = "Please enter a valid finish time.";
                return RedirectToAction(nameof(Results), new { eventId });
            }
            if (finishPosition is null or < 1)
            {
                TempData["Error"] = "Please enter a finishing position of 1 or more.";
                return RedirectToAction(nameof(Results), new { eventId });
            }

            var res = await _api.PostAsync($"api/enrolments/{enrolmentId}/result", new
            {
                finishTime = time,
                finishPosition = finishPosition.Value
            });

            if (res.IsSuccessStatusCode)
                TempData["Success"] = "Result captured.";
            else
                TempData["Error"] = await ApiClient.ReadMessageAsync(res, "Could not save the result. Please check the details and try again.");

            return RedirectToAction(nameof(Results), new { eventId });
        }

        // ---------- Shared loader ----------
        private async Task<(OrgEnrolmentsVm? Vm, string? Error)> LoadEnrolmentsAsync(int eventId)
        {
            var evRes = await _api.GetAsync($"api/events/{eventId}");
            if (!evRes.IsSuccessStatusCode) return (null, "We couldn't find that event.");

            var ev = await evRes.Content.ReadFromJsonAsync<EventDetails>();
            if (ev == null) return (null, "We couldn't find that event.");

            var enRes = await _api.GetAsync($"api/events/{eventId}/enrolments");
            if (!enRes.IsSuccessStatusCode)
                return (null, await ApiClient.ReadMessageAsync(enRes, "You can only view enrolments for your own events."));

            var rows = await enRes.Content.ReadFromJsonAsync<List<EnrolmentRow>>() ?? new();

            return (new OrgEnrolmentsVm
            {
                EventId = eventId,
                EventName = ev.Name,
                Rows = rows.OrderBy(r => r.ParticipantName).ToList()
            }, null);
        }
    }
}