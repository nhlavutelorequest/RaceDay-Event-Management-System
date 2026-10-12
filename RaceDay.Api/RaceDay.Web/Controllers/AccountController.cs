using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;
        public AccountController(ApiClient api) => _api = api;

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var res = await _api.PostAsync("api/auth/register", new
            {
                vm.FullName,
                vm.Email,
                vm.Password,
                vm.Role,
                vm.PhoneNumber
            });

            if (res.IsSuccessStatusCode)
            {
                TempData["Success"] = "Account created. Please log in.";
                return RedirectToAction(nameof(Login));
            }

            var body = await res.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Registration failed. API returned {(int)res.StatusCode}: {body}");
            return View(vm);
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var res = await _api.PostAsync("api/auth/login", new { vm.Email, vm.Password });

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Login failed. API returned {(int)res.StatusCode}: {body}");
                return View(vm);
            }

            var json = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            HttpContext.Session.SetString("UserName", json.GetProperty("fullName").GetString() ?? "");
            HttpContext.Session.SetString("Role", json.GetProperty("role").GetString() ?? "");
            HttpContext.Session.SetInt32("UserId", json.GetProperty("userId").GetInt32());

            return RedirectToAction("Index", "Events");
        }

        public async Task<IActionResult> Logout()
        {
            await _api.PostAsync("api/auth/logout");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}