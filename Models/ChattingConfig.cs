// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.Models;

public class ChattingConfig
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required] public Guid PracticeId { get; set; }

    [Required, MaxLength(255)] public string Name { get; set; }

    [Required, MaxLength(255)] public string AiRole { get; set; }

    [Required] public string Requirement { get; set; }

    [Required] public string Objective { get; set; }

    [Required] public int MaxAiReplies { get; set; }

    [Required] public string OpeningQuestion { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Practice Practice { get; set; }
}