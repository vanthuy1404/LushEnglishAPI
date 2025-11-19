// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.Models;

public class WritingConfig
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required]
    public Guid PracticeId { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    [Required]
    public string Requirement { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Practice Practice { get; set; }
}