using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class ProfileVm
    {
        public string? Email { get; set; }
        public string? Role { get; set; }

        [Required(ErrorMessage = "Full name is required."), MaxLength(100, ErrorMessage = "Full name is too long.")]
        public string FullName { get; set; } = "";

        [MaxLength(20, ErrorMessage = "Phone number is too long.")]
        public string? PhoneNumber { get; set; }
    }

    [SessionAuthorize]
    public class ProfileController : Controller
    {
        private readonly ApiClient _api;

        public ProfileController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var res = await _api.GetAsync("api/users/me");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await ApiClient.ReadMessageAsync(res, "Could not load your profile.");
                return RedirectToAction("Index", "Home");
            }

            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            var vm = new ProfileVm
            {
                FullName = json.GetProperty("fullName").GetString() ?? "",
                Email = json.GetProperty("email").GetString(),
                Role = json.GetProperty("role").GetString(),
                PhoneNumber = json.TryGetProperty("phoneNumber", out var p) && p.ValueKind == JsonValueKind.String
                    ? p.GetString() : null
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var res = await _api.PutAsync("api/users/me", new { vm.FullName, vm.PhoneNumber });
            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await ApiClient.ReadMessageAsync(res, "Could not update your profile. Please try again."));
                return View(vm);
            }

            HttpContext.Session.SetString("UserName", vm.FullName.Trim());
            TempData["Success"] = "Profile updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}