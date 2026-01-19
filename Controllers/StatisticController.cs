// 17/01/2026 - 17:29:02
// DANGTHUY

using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[AdminCheck]
[Route("api/[controller]")]
public class StatisticController : ControllerBase
{
    private readonly LushEnglishDbContext _db;

    public StatisticController(LushEnglishDbContext db)
    {
        _db = db;
    }

    // GET: api/Statistic/dashboard
    [HttpGet("dashboard")]
    public async Task<ActionResult<StatisticDTO>> GetDashboard()
    {
        var now = DateTime.UtcNow.AddHours(7);

        // ===== Totals (sequential) =====
        var totalUsers = await _db.Users.AsNoTracking().CountAsync();
        var totalCourses = await _db.Courses.AsNoTracking().CountAsync();
        var totalTopics = await _db.Topics.AsNoTracking().CountAsync();

        var practicesCount = await _db.Practices.AsNoTracking().CountAsync();
        var writingCount = await _db.WritingConfigs.AsNoTracking().CountAsync();
        var chattingCount = await _db.ChattingConfigs.AsNoTracking().CountAsync();
        var totalPractices = practicesCount + writingCount + chattingCount;

        // ===== 4 calendar weeks (Mon-Sun), Week4 = current week =====
        var w4Start = StartOfWeek(now, DayOfWeek.Monday);
        var w4End = w4Start.AddDays(7).AddTicks(-1);

        var w3Start = w4Start.AddDays(-7);
        var w3End = w4Start.AddTicks(-1);

        var w2Start = w4Start.AddDays(-14);
        var w2End = w4Start.AddDays(-7).AddTicks(-1);

        var w1Start = w4Start.AddDays(-21);
        var w1End = w4Start.AddDays(-14).AddTicks(-1);

        // Query theo từng window để SQL count trực tiếp (nhanh + ít RAM)
        var week1 = await _db.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= w1Start && u.CreatedAt <= w1End);
        var week2 = await _db.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= w2Start && u.CreatedAt <= w2End);
        var week3 = await _db.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= w3Start && u.CreatedAt <= w3End);
        var week4 = await _db.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= w4Start && u.CreatedAt <= w4End);

        // ===== Revenue (PAID) =====
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonthEnd = monthStart;

        var paidQuery = _db.UserCourses.AsNoTracking()
            .Where(x => x.Status == "PAID")
            .Select(x => new
            {
                x.Amount,
                PaidTime = (x.PaidAt ?? x.CreatedAt)
            });

        var totalRevenue = (await paidQuery.SumAsync(x => (decimal?)x.Amount)) ?? 0m;
        var revenueCurrentMonth = (await paidQuery
            .Where(x => x.PaidTime >= monthStart && x.PaidTime <= now)
            .SumAsync(x => (decimal?)x.Amount)) ?? 0m;

        var revenueLastMonth = (await paidQuery
            .Where(x => x.PaidTime >= lastMonthStart && x.PaidTime < lastMonthEnd)
            .SumAsync(x => (decimal?)x.Amount)) ?? 0m;

        // ===== Top 3 courses (name + count) =====
        var topCourses = await _db.UserCourses.AsNoTracking()
            .Where(x => x.Status == "PAID")
            .GroupBy(x => x.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(3)
            .ToListAsync();

        var topCourseIds = topCourses.Select(x => x.CourseId).ToList();

        var courseNames = await _db.Courses.AsNoTracking()
            .Where(c => topCourseIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        string top1Name = "";
        int top1Count = 0;
        if (topCourses.Count > 0)
        {
            top1Count = topCourses[0].Count;
            top1Name = courseNames.TryGetValue(topCourses[0].CourseId, out var n) ? n : "";
        }

        string top2Name = "";
        int top2Count = 0;
        if (topCourses.Count > 1)
        {
            top2Count = topCourses[1].Count;
            top2Name = courseNames.TryGetValue(topCourses[1].CourseId, out var n) ? n : "";
        }

        string top3Name = "";
        int top3Count = 0;
        if (topCourses.Count > 2)
        {
            top3Count = topCourses[2].Count;
            top3Name = courseNames.TryGetValue(topCourses[2].CourseId, out var n) ? n : "";
        }

        var dto = new StatisticDTO
        {
            TotalUsers = totalUsers,
            TotalCourses = totalCourses,
            TotalTopics = totalTopics,
            TotalPractices = totalPractices,

            NumberOfUsersWeek1 = week1,
            NumberOfUsersWeek2 = week2,
            NumberOfUsersWeek3 = week3,
            NumberOfUsersWeek4 = week4,

            TotalRevenue = totalRevenue,
            RevenueCurrentMonth = revenueCurrentMonth,
            RevenueLastMonth = revenueLastMonth,

            Top1Course = top1Name,
            Top1CourseCount = top1Count,
            Top2Course = top2Name,
            Top2CourseCount = top2Count,
            Top3Course = top3Name,
            Top3CourseCount = top3Count
        };

        return Ok(dto);
    }

    private static DateTime StartOfWeek(DateTime dateTime, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
        return dateTime.Date.AddDays(-diff);
    }
}
