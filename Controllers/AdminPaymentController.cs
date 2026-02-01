// 01/02/2026 - 14:14:42
// DANGTHUY

using LushEnglishAPI.Attributes;
using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminPaymentController : ControllerBase
{
    private readonly LushEnglishDbContext _context;
    public AdminPaymentController(LushEnglishDbContext context) => _context = context;

    // GET: /api/AdminPayment/payments?status=PAID&q=thuy&from=2026-01-01&to=2026-02-01
    [HttpGet("payments")]
    [SessionCheck]
    [AdminCheck]
    public async Task<ActionResult<List<PaymentManagementListItemDto>>> GetPayments(
        [FromQuery] string? status,
        [FromQuery] string? q,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to
    )
    {
        var query = _context.UserCourses.AsNoTracking();

        // Status filter: ALL / null => bỏ qua
        if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
            query = query.Where(x => x.Status == status);

        // Date filter theo CreatedAt (bạn có thể đổi sang PaidAt nếu muốn)
        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.CreatedAt <= to.Value);

        // Search: user name/email, course name, momo ids
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();

            query = query.Where(x =>
                (x.MomoOrderId != null && x.MomoOrderId.Contains(q)) ||
                (x.MomoRequestId != null && x.MomoRequestId.Contains(q)) ||
                (x.MomoTransId != null && x.MomoTransId.Contains(q)) ||
                _context.Users.Any(u => u.Id == x.UserId && (u.Email.Contains(q) || u.FullName.Contains(q))) ||
                _context.Courses.Any(c => c.Id == x.CourseId && c.Name.Contains(q))
            );
        }

        // Projection (read-only)
        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentManagementListItemDto
            {
                Id = x.Id,
                UserId = x.UserId,
                CourseId = x.CourseId,

                UserFullName = _context.Users
                    .Where(u => u.Id == x.UserId)
                    .Select(u => u.FullName)
                    .FirstOrDefault() ?? "",

                UserEmail = _context.Users
                    .Where(u => u.Id == x.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefault() ?? "",

                UserAvatarUrl = _context.Users
                    .Where(u => u.Id == x.UserId)
                    .Select(u => u.AvatarUrl)
                    .FirstOrDefault(),

                CourseName = _context.Courses
                    .Where(c => c.Id == x.CourseId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? "",

                CourseImageUrl = _context.Courses
                    .Where(c => c.Id == x.CourseId)
                    .Select(c => c.LinkImg)
                    .FirstOrDefault(),

                Amount = x.Amount,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                PaidAt = x.PaidAt,

                MomoOrderId = x.MomoOrderId,
                MomoRequestId = x.MomoRequestId,
                MomoTransId = x.MomoTransId
            })
            .ToListAsync();

        return Ok(data);
    }

    // GET: /api/AdminPayment/payments/{id}
    [HttpGet("payments/{id:guid}")]
    [SessionCheck]
    [AdminCheck]
    public async Task<ActionResult<PaymentManagementDetailDto>> GetPaymentDetail(Guid id)
    {
        var data = await _context.UserCourses
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PaymentManagementDetailDto
            {
                Id = x.Id,
                UserId = x.UserId,
                CourseId = x.CourseId,

                UserFullName = _context.Users
                    .Where(u => u.Id == x.UserId)
                    .Select(u => u.FullName)
                    .FirstOrDefault() ?? "",

                UserEmail = _context.Users
                    .Where(u => u.Id == x.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefault() ?? "",

                UserAvatarUrl = _context.Users
                    .Where(u => u.Id == x.UserId)
                    .Select(u => u.AvatarUrl)
                    .FirstOrDefault(),

                CourseName = _context.Courses
                    .Where(c => c.Id == x.CourseId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? "",

                CourseImageUrl = _context.Courses
                    .Where(c => c.Id == x.CourseId)
                    .Select(c => c.LinkImg)
                    .FirstOrDefault(),

                CourseDescription = _context.Courses
                    .Where(c => c.Id == x.CourseId)
                    .Select(c => c.Description)
                    .FirstOrDefault(),

                Amount = x.Amount,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                PaidAt = x.PaidAt,

                MomoOrderId = x.MomoOrderId,
                MomoRequestId = x.MomoRequestId,
                MomoTransId = x.MomoTransId
            })
            .FirstOrDefaultAsync();

        if (data == null) return NotFound();
        return Ok(data);
    }
}