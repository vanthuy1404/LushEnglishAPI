// 26/11/2025 - 21:52:38
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.DTOs;

public class ChattingExerciseDTO
{
    public Guid? Id { get; set; }
    public Guid TopicId { get; set; }
    public string TopicName { get; set; }
    [Required, MaxLength(255)]
    public string Name { get; set; }
    [Required, MaxLength(255)]
    public string AiRole { get;set; }
    [Required]
    public string Requirement {get; set; } // List string ["req1", "req2"]
    [Required]
    public string Objective {get; set; } // The objective for evaluating
    [Required]
    public int MaxAiReplies { get; set; }
    [Required]
    public int Level { get; set; } = 1; // 1: Beginner, 2: Intermediate, 3: Advanced
    
    public string LinkImage {get; set; } // src display Img
    public IFormFile? Image { get; set; }
    [Required]
    public string OpeningQuestion { get; set; }
    public DateTime CreatedAt { get; set; }
}