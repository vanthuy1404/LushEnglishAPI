// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.Models;

public class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required, MaxLength(255)] public string FullName { get; set; }

    [Required, MaxLength(255)] public string Email { get; set; }

    [Required, MaxLength(255)] public string Password { get; set; }

    [MaxLength(500)] public string AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    // Streak
    public int? BestStreak { get; set; } 
    // Navigation
    public ICollection<Result> Results { get; set; }

    public string LoginSession { get; set; }
    public bool? IsAdmin { get; set; }
}