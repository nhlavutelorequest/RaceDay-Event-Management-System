using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class MyResultVm
    {
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public string FinishTime { get; set; } = "";
        public int FinishPosition { get; set; }
        public int TotalFinishers { get; set; }
    }

    [SessionAuthorize("Participant")]
    public class ResultsController : Controller
    {
        private readonly ApiClient _api;

        public ResultsController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Mine()
        {
            var res = await _api.GetAsync("api/results/me");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await ApiClient.ReadMessageAsync(res, "Could not load your results.");
                return RedirectToAction("Index", "Events");
            }

            var list = await res.Content.ReadFromJsonAsync<List<MyResultVm>>() ?? new();
            return View(list.OrderByDescending(r => r.EventDate).ToList());
        }
    }
}