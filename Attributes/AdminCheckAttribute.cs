// 2025
// DANGTHUY

using LushEnglishAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminCheckAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var db = http.RequestServices.GetRequiredService<LushEnglishDbContext>();

        // Lấy UserId & SessionId từ headers (giống SessionCheck)
        var userIdStr = http.Request.Headers["UserId"].ToString();
        var sessionId = http.Request.Headers["SessionId"].ToString();

        if (!Guid.TryParse(userIdStr, out var userId))
        {
            context.Result = new UnauthorizedObjectResult("Invalid or missing UserId");
            return;
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            context.Result = new UnauthorizedObjectResult("Missing SessionId");
            return;
        }

        // Lấy user từ DB
        var user = await db.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.LoginSession == sessionId);

        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult("Invalid session");
            return;
        }

        // Kiểm tra quyền admin
        if (user.IsAdmin != true)
        {
            context.Result = new ForbidResult("Admin privileges required");
            return;
        }

        await next();
    }
}