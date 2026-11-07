using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class ManageEventsController : Controller
    {
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly ApiClient _api;
        public ManageEventsController(ApiClient api) => _api = api;

        private async Task<List<MyEventItem>?> LoadMineAsync()
        {
            var res = await _api.GetAsync("api/events/mine");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<List<MyEventItem>>() ?? new();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await LoadMineAsync();
            if (list == null)
            {
                ViewBag.Error = "We couldn't load your events right now. Please try again.";
                return View(new List<MyEventItem>());
            }
            return View(list.OrderBy(e => e.EventDate).ToList());
        }

        [HttpGet]
        public IActionResult Create() => View("Form", new EventFormVm());

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var list = await LoadMineAsync();
            var ev = list?.FirstOrDefault(e => e.EventId == id);
            if (ev == null) return NotFound();

            return View("Form", new EventFormVm
            {
                EventId = ev.EventId,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                DistanceKm = ev.DistanceKm,
                EventType = ev.EventType,
                BannerImageUrl = ev.BannerImageUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(EventFormVm vm, IFormFile? banner)
        {
            if (!ModelState.IsValid) return View("Form", vm);

            var hasBanner = banner != null && banner.Length > 0;
            if (hasBanner && banner!.Length > MaxImageBytes)
            {
                ModelState.AddModelError("", "The banner image is too large. The maximum size is 5 MB.");
                return View("Form", vm);
            }

            var body = new
            {
                vm.Name,
                vm.Description,
                vm.EventDate,
                vm.Location,
                vm.DistanceKm,
                vm.EventType,
                vm.BannerImageUrl
            };

            var res = vm.EventId == 0
                ? await _api.PostAsync("api/events", body)
                : await _api.PutAsync($"api/events/{vm.EventId}", body);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await ApiClient.ReadMessageAsync(res, "Could not save the event. Please check the details and try again."));
                return View("Form", vm);
            }

            var eventId = vm.EventId;
            if (eventId == 0)
            {
                var created = await res.Content.ReadFromJsonAsync<JsonElement>();
                eventId = created.GetProperty("eventId").GetInt32();
            }

            if (hasBanner)
            {
                var upload = await _api.PostFileAsync($"api/events/{eventId}/banner", banner!);
                if (!upload.IsSuccessStatusCode)
                {
                    var reason = await ApiClient.ReadMessageAsync(upload, "Please try again.");
                    TempData["Error"] = $"The event was saved, but the banner could not be uploaded. {reason} You can add it by editing the event.";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Success"] = vm.EventId == 0 ? "Event created." : "Event updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _api.DeleteAsync($"api/events/{id}");
            if (res.IsSuccessStatusCode)
                TempData["Success"] = "Event deleted.";
            else
                TempData["Error"] = await ApiClient.ReadMessageAsync(res, "Could not delete this event. It may still have enrolments.");

            return RedirectToAction(nameof(Index));
        }
    }
}