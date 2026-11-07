using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiClient _api;

        public HomeController(ILogger<HomeController> logger, ApiClient api)
        {
            _logger = logger;
            _api = api;
        }

        public async Task<IActionResult> Index(string? search, string? type)
        {
            var upcoming = new List<EventListItem>();

            try
            {
                var res = await _api.GetAsync("api/events");
                if (res.IsSuccessStatusCode)
                {
                    var events = await res.Content.ReadFromJsonAsync<List<EventListItem>>() ?? new();
                    upcoming = events.Where(e => e.EventDate >= DateTime.Now).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load upcoming events.");
            }

            if (!string.IsNullOrWhiteSpace(search))
                upcoming = upcoming.Where(e =>
                    e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(type))
                upcoming = upcoming.Where(e =>
                    string.Equals(e.EventType, type, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.Search = search;
            ViewBag.Type = type;

            return View(upcoming.OrderBy(e => e.EventDate).ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult HttpStatus(int code)
        {
            ViewBag.Code = code;
            ViewBag.Heading = code == 404 ? "Page not found" : "Something went wrong";
            ViewBag.Message = code == 404
                ? "We couldn't find the page you were looking for."
                : "We couldn't complete that request. Please try again.";
            return View();
        }
    }
}