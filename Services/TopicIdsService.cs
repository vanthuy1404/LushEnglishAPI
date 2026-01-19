// 14/01/2026 - 21:31:34
// DANGTHUY

using LushEnglishAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Services;

public class TopicIdsService
{
    private readonly LushEnglishDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TopicIdsService(LushEnglishDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Validate session giống SessionCheckAttribute và trả về userId
    /// </summary>
    private async Task<Guid> RequireValidUserAsync()
    {
        var http = _httpContextAccessor.HttpContext;
        if (http == null) throw new UnauthorizedAccessException("HttpContext is null");

        var userIdStr = http.Request.Headers["UserId"].ToString();
        var sessionId = http.Request.Headers["SessionId"].ToString();

        if (string.IsNullOrWhiteSpace(userIdStr))
            throw new UnauthorizedAccessException("Missing or invalid UserId header");

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new UnauthorizedAccessException("Missing or invalid SessionId header");

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("UserId must be a valid GUID");

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.LoginSession == sessionId);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid session");

        return userId;
    }

    /// <summary>
    /// Lấy list topicIds mà user được phép học:
    /// UserCourses(PAID) -> courseIds -> topics(courseId in courseIds) -> topicIds
    /// </summary>
    public async Task<List<Guid>> GetMyTopicIdsAsync()
    {
        var userId = await RequireValidUserAsync();

        // 1) Lấy list courseId mà user đã PAID
        var courseIds = await _db.UserCourses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == "PAID")
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync();

        if (courseIds.Count == 0)
            return new List<Guid>();

        // 2) Lấy list topicId thuộc các course đó
        var topicIds = await _db.Topics
            .AsNoTracking()
            .Where(t => t.CourseId != null && courseIds.Contains(t.CourseId.Value))
            .Select(t => t.Id)
            .Distinct()
            .ToListAsync();

        return topicIds;
    }

    /// <summary>
    /// Nếu bạn cần cả courseIds (tùy use-case)
    /// </summary>
    public async Task<List<Guid>> GetMyCourseIdsAsync()
    {
        var userId = await RequireValidUserAsync();

        var courseIds = await _db.UserCourses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == "PAID")
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync();

        return courseIds;
    }
}