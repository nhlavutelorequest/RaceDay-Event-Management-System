using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters
{
    // Usage: [SessionAuthorize] - must be logged in (any role)
    //        [SessionAuthorize("Organiser")] - must be logged in AND have this specific role
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string? _requiredRole;

        public SessionAuthorizeAttribute(string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in to access this resource." });
                return;
            }

            if (_requiredRole != null)
            {
                var role = context.HttpContext.Session.GetString("Role");
                if (!string.Equals(role, _requiredRole, StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new ObjectResult(new { message = $"This action requires the {_requiredRole} role." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
        }
    }
}