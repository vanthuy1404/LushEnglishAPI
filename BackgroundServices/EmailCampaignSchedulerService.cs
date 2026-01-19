// 18/01/2026 - 20:34:29
// DANGTHUY

using LushEnglishAPI.Data;
using LushEnglishAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LushEnglishAPI.BackgroundServices;

public class EmailCampaignSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    // VN time: UTC+7
    private static DateTime NowVn() => DateTime.UtcNow.AddHours(7);

    public EmailCampaignSchedulerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // loop forever
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnce(stoppingToken);
            }
            catch
            {
                // swallow to keep host alive; add logging later if you want
            }

            await DelayToNextCheck(stoppingToken);
        }
    }

    private async Task RunOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LushEnglishDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        var now = NowVn();
        var today = now.Date;

        // Active campaigns that are within date window
        var campaigns = await db.EmailCampaigns
            .AsNoTracking()
            .Where(c => c.IsEnabled
                        && c.StartDate.Date <= today
                        && c.EndDate.Date >= today)
            .ToListAsync(ct);

        if (campaigns.Count == 0) return;

        // Load all users once (only those with email)
        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.Email != null && u.Email != "")
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToListAsync(ct);

        if (users.Count == 0) return;

        foreach (var c in campaigns)
        {
            // Today's scheduled time (VN)
            var scheduledToday = today.Add(c.SendTimeOfDay);

            // Not yet time => skip
            if (now < scheduledToday) continue;

            // We send 1 time per user per campaign.
            // Fetch already-sent users for this campaign
            var sentUserIds = await db.EmailCampaignDeliveries
                .AsNoTracking()
                .Where(d => d.CampaignId == c.Id)
                .Select(d => d.UserId)
                .ToListAsync(ct);

            var sentSet = sentUserIds.ToHashSet();

            // Eligible users = not yet sent
            var targets = users.Where(u => !sentSet.Contains(u.Id)).ToList();
            if (targets.Count == 0) continue;

            // Optional: batch insert deliveries for speed (but we need per-user success/fail)
            foreach (var u in targets)
            {
                if (string.IsNullOrWhiteSpace(u.Email)) continue;

                // IMPORTANT: prevent duplicates in race conditions
                // (If you add UNIQUE index (CampaignId, UserId) it becomes rock-solid)
                var exists = await db.EmailCampaignDeliveries
                    .AsNoTracking()
                    .AnyAsync(x => x.CampaignId == c.Id && x.UserId == u.Id, ct);

                if (exists) continue;

                var subject = c.Subject;
                var body = PersonalizeBody(c.HtmlBody, u.FullName);

                var ok = true;
                try
                {
                    await emailService.SendAsync(u.Email, subject, body);
                }
                catch
                {
                    ok = false;
                }

                db.EmailCampaignDeliveries.Add(new Models.EmailCampaignDelivery
                {
                    Id = Guid.CreateVersion7(),
                    CampaignId = c.Id,
                    UserId = u.Id,
                    Email = u.Email,
                    Status = ok,
                    SentAt = now
                });

                // Save per user to avoid losing the whole batch when one fails
                // (If you want faster: save every 50 items)
                await db.SaveChangesAsync(ct);
            }
        }
    }

    private static string PersonalizeBody(string htmlBody, string? fullName)
    {
        // Simple personalization: replace placeholders if you want
        // Example: use {{name}} in HTML
        var safeName = System.Net.WebUtility.HtmlEncode(fullName ?? "there");
        return (htmlBody ?? "").Replace("{{name}}", safeName);
    }

    private static async Task DelayToNextCheck(CancellationToken ct)
    {
        // Check every 30 seconds (safe + simple)
        var delay = TimeSpan.FromSeconds(30);
        if (delay <= TimeSpan.Zero) delay = TimeSpan.FromSeconds(1);
        await Task.Delay(delay, ct);
    }
}
