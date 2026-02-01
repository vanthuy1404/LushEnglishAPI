// 01/02/2026 - 14:13:52
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class PaymentManagementListItemDto
{
    public Guid Id { get; set; }          // UserCourse.Id
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }

    // User (basic)
    public string UserFullName { get; set; } = default!;
    public string UserEmail { get; set; } = default!;
    public string? UserAvatarUrl { get; set; }

    // Course (basic)
    public string CourseName { get; set; } = default!;
    public string? CourseImageUrl { get; set; }

    // Payment
    public decimal Amount { get; set; }           // số tiền thực trả
    public string Status { get; set; } = "PENDING"; // PENDING/PAID/FAILED
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    // MoMo trace
    public string? MomoOrderId { get; set; }
    public string? MomoRequestId { get; set; }
    public string? MomoTransId { get; set; }
}
public class PaymentManagementDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }

    public string UserFullName { get; set; } = default!;
    public string UserEmail { get; set; } = default!;
    public string? UserAvatarUrl { get; set; }

    public string CourseName { get; set; } = default!;
    public string? CourseImageUrl { get; set; }
    public string? CourseDescription { get; set; }

    public decimal Amount { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public string? MomoOrderId { get; set; }
    public string? MomoRequestId { get; set; }
    public string? MomoTransId { get; set; }
}