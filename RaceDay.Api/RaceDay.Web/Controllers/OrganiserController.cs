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
    }
}