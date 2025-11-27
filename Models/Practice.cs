// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.Models;

public class Practice
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required] public Guid TopicId { get; set; }

    [Required, MaxLength(255)] public string Name { get; set; }
    
    [Required]
    public int Level { get; set; } = 1; // 1: Beginner, 2: Intermediate, 3: Advanced
    
    public string LinkImage {get; set; } // src display Img

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    // Navigation
    public Topic Topic { get; set; }
    public ICollection<Question> Questions { get; set; }
    public ICollection<Result> Results { get; set; }
}