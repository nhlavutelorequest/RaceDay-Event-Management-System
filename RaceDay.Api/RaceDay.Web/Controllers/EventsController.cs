using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiClient _api;
        public EventsController(ApiClient api) => _api = api;

        public async Task<IActionResult> Index(string? search, string? type)
        {
            var vm = new EventsIndexViewModel { Search = search, Type = type };

            var res = await _api.GetAsync("api/events");
            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Error = "We couldn't load events right now. Please try again.";
                return View(vm);
            }

            var events = await res.Content.ReadFromJsonAsync<List<EventListItem>>() ?? new();

            if (!string.IsNullOrWhiteSpace(search))
                events = events.Where(e =>
                    e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(type))
                events = events.Where(e =>
                    string.Equals(e.EventType, type, StringComparison.OrdinalIgnoreCase)).ToList();

            vm.Events = events.OrderBy(e => e.EventDate).ToList();
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var res = await _api.GetAsync($"api/events/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var ev = await res.Content.ReadFromJsonAsync<EventDetails>();
            if (ev == null) return NotFound();

            return View(ev);
        }
    }
}