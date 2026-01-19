// 13/01/2026 - 22:23:52
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseController(LushEnglishDbContext context, IMapper mapper, IWebHostEnvironment env) : ControllerBase
{
    private readonly LushEnglishDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    // GET: api/Course
    [HttpGet]
    public async Task<ActionResult<List<CourseDTO>>> GetAllCourses()
    {
        var courses = await _context.Courses
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var results = _mapper.Map<List<CourseDTO>>(courses);

        // (Optional) kèm số topic để FE hiển thị
        var courseIds = courses.Select(c => c.Id).ToList();
        var topicCounts = await _context.Topics
            .Where(t => t.CourseId != null && courseIds.Contains(t.CourseId.Value))
            .GroupBy(t => t.CourseId!.Value)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        foreach (var r in results)
        {
            r.TopicCount = topicCounts.TryGetValue(r.Id, out var c) ? c : 0;
        }

        return Ok(results);
    }
    [HttpGet("not-registered")]
public async Task<ActionResult<List<CourseDTO>>> GetCoursesUserNotRegistered()
{
    var userIdHeader = Request.Headers["UserId"].ToString();
    Guid? userId = null;

    if (!string.IsNullOrWhiteSpace(userIdHeader) && Guid.TryParse(userIdHeader, out var parsedId))
        userId = parsedId;

    // ===== CASE 1: Guest (không có userId) → trả full course =====
    if (userId == null)
    {
        var allCourses = await _context.Courses
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var results = _mapper.Map<List<CourseDTO>>(allCourses);

        // kèm topicCount
        var courseIds = allCourses.Select(c => c.Id).ToList();
        var topicCounts = await _context.Topics
            .Where(t => t.CourseId != null && courseIds.Contains(t.CourseId.Value))
            .GroupBy(t => t.CourseId!.Value)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        foreach (var r in results)
            r.TopicCount = topicCounts.TryGetValue(r.Id, out var c) ? c : 0;

        return Ok(results);
    }

    // ===== CASE 2: Logged-in user → lọc course chưa mua =====
    var boughtCourseIds = await _context.UserCourses
        .Where(x => x.UserId == userId && x.Status == "PAID")
        .Select(x => x.CourseId)
        .Distinct()
        .ToListAsync();

    var courses = await _context.Courses
        .Where(c => !boughtCourseIds.Contains(c.Id))
        .OrderByDescending(c => c.CreatedAt)
        .ToListAsync();

    var resultDtos = _mapper.Map<List<CourseDTO>>(courses);

    // kèm topicCount
    var ids = courses.Select(c => c.Id).ToList();
    var counts = await _context.Topics
        .Where(t => t.CourseId != null && ids.Contains(t.CourseId.Value))
        .GroupBy(t => t.CourseId!.Value)
        .Select(g => new { CourseId = g.Key, Count = g.Count() })
        .ToDictionaryAsync(x => x.CourseId, x => x.Count);

    foreach (var r in resultDtos)
        r.TopicCount = counts.TryGetValue(r.Id, out var c) ? c : 0;

    return Ok(resultDtos);
}

    // GET: api/Course/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDTO>> GetCourse(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound("Course not found");

        var result = _mapper.Map<CourseDTO>(course);

        // (Optional) kèm danh sách topic thuộc course
        var topics = await _context.Topics
            .Where(t => t.CourseId == id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new CourseTopicItemDTO
            {
                Id = t.Id,
                Name = t.Name,
                Level = t.Level,
                LinkImage = t.LinkImage,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        result.Topics = topics;
        result.TopicCount = topics.Count;

        return Ok(result);
    }

    // POST: api/Course
    [HttpPost]
    [AdminCheck]
    public async Task<ActionResult<CourseDTO>> CreateCourse([FromForm] CreateCourseDTO request)
    {
        string imagePathDb = "";

        // Lưu ảnh (nếu có)
        if (request.Image != null && request.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            var folderPath = Path.Combine(env.WebRootPath, "images");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            imagePathDb = $"/images/{fileName}";
        }

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var newCourse = new Course
            {
                Id = Guid.CreateVersion7(),
                Name = request.Name,
                Price = request.Price,
                Description = request.Description,
                LinkImg = string.IsNullOrWhiteSpace(imagePathDb) ? null : imagePathDb,
                Discount = request.Discount,
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };

            await _context.Courses.AddAsync(newCourse);
            await _context.SaveChangesAsync();

            // ===== NEW: Assign topics by list (only unassigned topics) =====
            if (request.TopicIds != null && request.TopicIds.Count > 0)
            {
                var toAssign = await _context.Topics
                    .Where(t => request.TopicIds.Contains(t.Id) && t.CourseId == null)
                    .ToListAsync();

                foreach (var t in toAssign)
                    t.CourseId = newCourse.Id;

                await _context.SaveChangesAsync();
            }

            await tx.CommitAsync();

            // Build response
            var result = _mapper.Map<CourseDTO>(newCourse);

            var topics = await _context.Topics
                .Where(t => t.CourseId == newCourse.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new CourseTopicItemDTO
                {
                    Id = t.Id,
                    Name = t.Name,
                    Level = t.Level,
                    LinkImage = t.LinkImage,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            result.Topics = topics;
            result.TopicCount = topics.Count;

            return Ok(result);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }


    // PUT: api/Course/{id}
    [HttpPut("{id}")]
    [AdminCheck]
    public async Task<ActionResult<CourseDTO>> UpdateCourse(Guid id, [FromForm] CreateCourseDTO request)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound("Course not found");

        course.Name = request.Name;
        course.Price = request.Price;
        course.Description = request.Description;
        course.Discount = request.Discount;

        // Nếu có ảnh mới -> replace
        if (request.Image != null && request.Image.Length > 0)
        {
            // Xóa ảnh cũ
            if (!string.IsNullOrEmpty(course.LinkImg))
            {
                var oldPath = Path.Combine(env.WebRootPath, course.LinkImg.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            var folderPath = Path.Combine(env.WebRootPath, "images");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            course.LinkImg = $"/images/{fileName}";
        }
        // request.Image == null => giữ nguyên LinkImg

        // ===== NEW: Update topics assignment (B1 clear, B2 re-assign) =====
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            // B1: clear all topics currently in this course
            var currentTopics = await _context.Topics
                .Where(t => t.CourseId == id)
                .ToListAsync();

            foreach (var t in currentTopics)
                t.CourseId = null;

            await _context.SaveChangesAsync();

            // B2: assign again by new list
            if (request.TopicIds != null && request.TopicIds.Count > 0)
            {
                // only assign topics that are either unassigned OR already in this course
                // (để tránh “cướp topic” từ course khác)
                var toAssign = await _context.Topics
                    .Where(t => request.TopicIds.Contains(t.Id) && (t.CourseId == null || t.CourseId == id))
                    .ToListAsync();

                foreach (var t in toAssign)
                    t.CourseId = id;

                await _context.SaveChangesAsync();
            }

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        var result = _mapper.Map<CourseDTO>(course);

        // kèm số topic + topics
        var topics = await _context.Topics
            .Where(t => t.CourseId == id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new CourseTopicItemDTO
            {
                Id = t.Id,
                Name = t.Name,
                Level = t.Level,
                LinkImage = t.LinkImage,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        result.Topics = topics;
        result.TopicCount = topics.Count;

        return Ok(result);
    }

    // DELETE: api/Course/{id}
    [HttpDelete("{id}")]
    [AdminCheck]
    public async Task<ActionResult> DeleteCourse(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound("Course not found");

        // Nếu muốn "lỏng": xoá course thì set CourseId của topic = null
        var topics = await _context.Topics.Where(t => t.CourseId == id).ToListAsync();
        foreach (var t in topics) t.CourseId = null;

        // (Optional) xoá ảnh course cho đỡ rác
        if (!string.IsNullOrEmpty(course.LinkImg))
        {
            var oldPath = Path.Combine(env.WebRootPath, course.LinkImg.TrimStart('/'));
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return Ok("Delete course successfully");
    }
    // GET: api/Course/my-courses
    [HttpGet("my-courses")]
    public async Task<ActionResult<List<MyCourseItemDTO>>> GetMyCourses()
    {
        // ===== Validate session giống TopicIdsService =====
        var userIdStr = Request.Headers["UserId"].ToString();
        var sessionId = Request.Headers["SessionId"].ToString();

        if (string.IsNullOrWhiteSpace(userIdStr) || string.IsNullOrWhiteSpace(sessionId))
            return Unauthorized("Missing UserId or SessionId");

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid UserId");

        var validUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.LoginSession == sessionId);

        if (validUser == null)
            return Unauthorized("Invalid session");

        // ===== Lấy courses user đã PAID + thời gian đăng ký =====
        var courses = await _context.UserCourses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == "PAID")
            .Join(
                _context.Courses.AsNoTracking(),
                uc => uc.CourseId,
                c => c.Id,
                (uc, c) => new MyCourseItemDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegisteredAt = (uc.PaidAt ?? uc.CreatedAt)
                }
            )
            .OrderByDescending(x => x.RegisteredAt)
            .ToListAsync();

        return Ok(courses);
    }
}

// =================== DTOs ===================

public class CourseDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public decimal Price { get; set; }

    public string? Description { get; set; }
    public string? LinkImg { get; set; }

    public decimal? Discount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Optional UI helpers
    public int TopicCount { get; set; } = 0;
    public List<CourseTopicItemDTO>? Topics { get; set; }
}

public class CourseTopicItemDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public string? LinkImage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCourseDTO
{
    [Required, MaxLength(255)] public string Name { get; set; }

    public decimal Price { get; set; } = 0;

    public string? Description { get; set; }

    public decimal? Discount { get; set; }

    // React gửi file (key='image')
    public IFormFile? Image { get; set; }

    // NEW: list topic id to assign (FormData append 'topicIds' multiple times)
    public List<Guid>? TopicIds { get; set; }
}
public class MyCourseItemDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime RegisteredAt { get; set; } // thời gian đăng ký
}