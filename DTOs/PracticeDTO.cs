// 19/11/2025 - 20:53:01
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.DTOs;

public class PracticeDTO
{
    public Guid? Id { get; set; }
    [Required] public Guid TopicId { get; set; }
    [Required, MaxLength(255)] public string Name { get; set; }
    [Required]
    public int Level { get; set; } = 1; // 1: Beginner, 2: Intermediate, 3: Advanced
    
    public string LinkImage {get; set; } // src display Img
    public DateTime CreatedAt { get; set; }
    public List<QuestionDTO>? Questions { get; set; }
}