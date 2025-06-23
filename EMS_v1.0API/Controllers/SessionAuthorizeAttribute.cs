using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string[] RequiredRole { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<SessionAuthorizeAttribute>>();
        var cookies = context.HttpContext.Request.Cookies;
        foreach (var cookie in cookies)
        {
            logger.LogInformation($"[SessionAuthorize] Cookie: {cookie.Key} = {cookie.Value}");
        }

        var userId = context.HttpContext.Session.GetString("UserId");
        logger.LogInformation($"[SessionAuthorize] UserId: {userId}");

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                Success = false,
                Message = "Authentication required"
            });
            return;
        }

        if (RequiredRole != null && RequiredRole.Length > 0)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            logger.LogInformation($"[SessionAuthorize] UserRole: {userRole}, RequiredRoles: {string.Join(", ", RequiredRole)}");
            if (string.IsNullOrEmpty(userRole) || (!RequiredRole.Contains(userRole) && userRole != "Admin"))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}