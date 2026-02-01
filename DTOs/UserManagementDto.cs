// 01/02/2026 - 13:41:52
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class UserManagementDto
{
    // Basic info
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool? IsAdmin { get; set; }
    public int? BestStreak { get; set; }

    // Purchased courses (PAID)
    public List<PurchasedCourseDto> PurchasedCourses { get; set; } = new();

    // Stats
    public int CompletedExercisesCount { get; set; }     // COUNT(Result)
    public decimal TotalExperiencePoints { get; set; }   // SUM(Result.Score)
}

public class PurchasedCourseDto
{
    public Guid CourseId { get; set; }
    public string? CourseName { get; set; }

    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
}