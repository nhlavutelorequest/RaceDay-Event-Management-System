using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api.Controllers
{
    // Other controllers inherit from this to easily access the logged-in user's session info.
    public abstract class BaseApiController : ControllerBase
    {
        protected int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
        protected string? CurrentRole => HttpContext.Session.GetString("Role");
    }
}