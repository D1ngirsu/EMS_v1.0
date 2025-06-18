using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string RequiredRole { get; set; }

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

        // Check role if specified
        if (!string.IsNullOrEmpty(RequiredRole))
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            if (userRole != RequiredRole && userRole != "Admin")
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}