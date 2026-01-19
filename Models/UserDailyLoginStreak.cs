// 17/01/2026 - 18:26:16
// DANGTHUY
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LushEnglishAPI.Models;

public class UserDailyLoginStreak
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Activity date in local time (+07), DATE only
    /// </summary>
    [Required]
    [Column(TypeName = "date")]
    public DateTime ActivityDate { get; set; }

    /// <summary>
    /// First request detected in this day
    /// </summary>
    [Required]
    public DateTime FirstSeenAt { get; set; }

    /// <summary>
    /// Optional: last request time in the same day
    /// </summary>
    public DateTime? LastSeenAt { get; set; }

    // Navigation (optional)
    public User? User { get; set; }
}
