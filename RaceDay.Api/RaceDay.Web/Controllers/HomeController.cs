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

        public async Task<IActionResult> Index()
        {
            var featured = new List<EventListItem>();

            try
            {
                var res = await _api.GetAsync("api/events");
                if (res.IsSuccessStatusCode)
                {
                    var events = await res.Content.ReadFromJsonAsync<List<EventListItem>>() ?? new();
                    featured = events
                        .Where(e => e.EventDate >= DateTime.Now)
                        .OrderBy(e => e.EventDate)
                        .Take(3)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load featured events.");
            }

            return View(featured);
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
    }
}