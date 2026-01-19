// 17/01/2026 - 18:33:07
// DANGTHUY
using LushEnglishAPI.Data;
using LushEnglishAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Middlewares;

public class DailyLoginStreakMiddleware
{
    private readonly RequestDelegate _next;

    public DailyLoginStreakMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Always continue pipeline even if anything fails here
        try
        {
            var userIdStr = context.Request.Headers["UserId"].ToString();
            var sessionId = context.Request.Headers["SessionId"].ToString();

            if (!string.IsNullOrWhiteSpace(userIdStr) &&
                !string.IsNullOrWhiteSpace(sessionId) &&
                Guid.TryParse(userIdStr, out var userId))
            {
                var db = context.RequestServices.GetRequiredService<LushEnglishDbContext>();

                // validate session (same logic as SessionCheckAttribute)
                var user = await db.Users
                    .FirstOrDefaultAsync(x => x.Id == userId && x.LoginSession == sessionId);

                if (user != null)
                {
                    var now = DateTime.UtcNow.AddHours(7);
                    var today = now.Date;

                    // Upsert daily streak row
                    var existing = await db.UserDailyLoginStreaks
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.ActivityDate == today);

                    if (existing == null)
                    {
                        db.UserDailyLoginStreaks.Add(new UserDailyLoginStreak
                        {
                            Id = Guid.CreateVersion7(),
                            UserId = userId,
                            ActivityDate = today,
                            FirstSeenAt = now,
                            LastSeenAt = now
                        });

                        // 1) Save first so today's row exists in DB
                        await db.SaveChangesAsync();

                        // 2) Now streak must be >= 1
                        var currentStreak = await CalculateCurrentStreakAsync(db, userId, today);

                        // 3) Update best streak (first login => should become 1)
                        if (currentStreak > (user.BestStreak ?? 0))
                        {
                            user.BestStreak = currentStreak;
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        // Optional: update last seen time (no need to recalc streak)
                        if (existing.LastSeenAt == null || (now - existing.LastSeenAt.Value).TotalMinutes >= 5)
                        {
                            existing.LastSeenAt = now;
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
        }
        catch
        {
            // swallow to avoid breaking requests; you can log later if needed
        }

        await _next(context);
    }

    /// <summary>
    /// Current streak: if today exists => start today, else start yesterday (but since we call this on insert-today, start today).
    /// We still keep the algorithm generic.
    /// </summary>
    private static async Task<int> CalculateCurrentStreakAsync(LushEnglishDbContext db, Guid userId, DateTime today)
    {
        // Take last 120 days to be safe
        var days = await db.UserDailyLoginStreaks
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ActivityDate)
            .Select(x => x.ActivityDate)
            .Take(120)
            .ToListAsync();

        if (days.Count == 0) return 0;

        var set = days.ToHashSet();

        // Determine start date
        DateTime startDate;
        if (set.Contains(today)) startDate = today;
        else if (set.Contains(today.AddDays(-1))) startDate = today.AddDays(-1);
        else return 0;

        int streak = 0;
        while (set.Contains(startDate))
        {
            streak++;
            startDate = startDate.AddDays(-1);
        }

        return streak;
    }
}
