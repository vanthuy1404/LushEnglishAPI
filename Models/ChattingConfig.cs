// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.Models;

public class ChattingConfig
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required] public Guid TopicId { get; set; }

    [Required, MaxLength(255)] public string Name { get; set; }

    [Required, MaxLength(255)] public string AiRole { get; set; }

    [Required] public string Requirement { get; set; }
    
    [Required]
    public int Level { get; set; } = 1; // 1: Beginner, 2: Intermediate, 3: Advanced
    
    [Required] public string Objective { get; set; }

    [Required] public int MaxAiReplies { get; set; }

    [Required] public string OpeningQuestion { get; set; }
    
    public string LinkImage {get; set; } // src display Img


    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Topic Topic { get; set; }
}