// 2025
// DANGTHUY

using LushEnglishAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionCheckAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var db = http.RequestServices.GetRequiredService<LushEnglishDbContext>();

        // Read headers
        var userIdStr = http.Request.Headers["UserId"].ToString();
        var sessionId = http.Request.Headers["SessionId"].ToString();

        if (string.IsNullOrWhiteSpace(userIdStr))
        {
            context.Result = new UnauthorizedObjectResult("Missing or invalid UserId header");
            return;
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            context.Result = new UnauthorizedObjectResult("Missing or invalid SessionId header");
            return;
        }

        // Validate GUID
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            context.Result = new UnauthorizedObjectResult("UserId must be a valid GUID");
            return;
        }

        // Validate in database
        var user = await db.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.LoginSession == sessionId);

        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult("Invalid session");
            return;
        }

        // Session valid — Continue
        await next();
    }
}