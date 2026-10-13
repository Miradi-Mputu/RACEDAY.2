using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayAPI.Attributes
{
    // This attribute checks if the logged-in user has the required role.
    // Use it like: [RoleAuthorize("Organiser")] on a controller method.
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredRole;

        public RoleAuthorizeAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Get the user's role from the session
            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");

            // If no role in session, the user is not logged in
            if (string.IsNullOrEmpty(userRole))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Not logged in." });
                return;
            }

            // If the user's role doesn't match what's required, deny access
            if (userRole != _requiredRole)
            {
                context.Result = new ObjectResult(new { message = $"Access denied. Requires role: {_requiredRole}." })
                {
                    StatusCode = 403
                };
            }
        }
    }
}