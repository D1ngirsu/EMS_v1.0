using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string[] RequiredRole { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                Success = false,
                Message = "Authentication required"
            });
            return;
        }

        // Check roles if specified
        if (RequiredRole != null && RequiredRole.Length > 0)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || (!RequiredRole.Contains(userRole) && userRole != "Admin"))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}