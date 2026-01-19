// 17/01/2026 - 22:37:32
// DANGTHUY

using LushEnglishAPI.Data;
using LushEnglishAPI.Services;

namespace LushEnglishAPI.BackgroundServices;

using LushEnglishAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class DailyStreakReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DailyStreakReminderService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await WaitUntilNextRun(stoppingToken); // 20:00 everyday (VN time)
            await RunJob(stoppingToken);
        }
    }

    private async Task RunJob(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LushEnglishDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        var today = DateTime.UtcNow.AddHours(7).Date;
        var yesterday = today.AddDays(-1);

        // 1) Candidate userIds: logged yesterday but NOT today
        var candidateUserIds = await db.UserDailyLoginStreaks
            .AsNoTracking()
            .Where(s => s.ActivityDate == yesterday)
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync(ct);

        if (candidateUserIds.Count == 0) return;

        var loggedTodayUserIds = await db.UserDailyLoginStreaks
            .AsNoTracking()
            .Where(s => s.ActivityDate == today && candidateUserIds.Contains(s.UserId))
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync(ct);

        var finalUserIds = candidateUserIds.Except(loggedTodayUserIds).ToList();
        if (finalUserIds.Count == 0) return;

        // 2) Load users
        var users = await db.Users
            .AsNoTracking()
            .Where(u => finalUserIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToListAsync(ct);

        // 3) Send email per user, compute streak from streak table
        foreach (var u in users)
        {
            if (string.IsNullOrWhiteSpace(u.Email)) continue;

            // streak length up to yesterday
            var streakLen = await CalculateStreakUpToAsync(db, u.Id, yesterday, ct);
            if (streakLen <= 0) continue; // safety

            var subject = "🔥 Keep your learning streak alive!";
            var body = $@"
                <div style='font-family:Arial,sans-serif;line-height:1.6'>
                  <h2>Hello {System.Net.WebUtility.HtmlEncode(u.FullName ?? "there")} 👋</h2>
                  <p>You are on a <b>{streakLen}-day</b> login streak.</p>
                  <p><b>Log in today</b> to keep it going 🔥</p>
                  <br/>
                  <p style='color:#666'>Lush English</p>
                </div>
            ";

            try
            {
                await emailService.SendAsync(u.Email, subject, body);
            }
            catch
            {
                // ignore per-user failure (you can log later)
            }
        }
    }

    /// <summary>
    /// Count consecutive days ending at endDate (e.g. yesterday).
    /// Requires only UserDailyLoginStreaks.
    /// </summary>
    private static async Task<int> CalculateStreakUpToAsync(
        LushEnglishDbContext db,
        Guid userId,
        DateTime endDate,
        CancellationToken ct)
    {
        // Take last 120 days to be safe
        var dates = await db.UserDailyLoginStreaks
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ActivityDate <= endDate)
            .OrderByDescending(x => x.ActivityDate)
            .Select(x => x.ActivityDate)
            .Take(120)
            .ToListAsync(ct);

        if (dates.Count == 0) return 0;

        var set = dates.ToHashSet();

        int streak = 0;
        var d = endDate.Date;
        while (set.Contains(d))
        {
            streak++;
            d = d.AddDays(-1);
        }

        return streak;
    }

    private const bool DEBUG_RUN_EVERY_MINUTE = false;

    private static async Task WaitUntilNextRun(CancellationToken ct)
    {
        var now = DateTime.UtcNow.AddHours(7);

        DateTime nextRun;
        if (DEBUG_RUN_EVERY_MINUTE)
        {
            nextRun = now.AddMinutes(1);
        }
        else
        {
            nextRun = now.Date.AddHours(20);
            if (now >= nextRun) nextRun = nextRun.AddDays(1);
        }

        var delay = nextRun - now;
        if (delay <= TimeSpan.Zero) delay = TimeSpan.FromSeconds(1);

        await Task.Delay(delay, ct);
    }
}
