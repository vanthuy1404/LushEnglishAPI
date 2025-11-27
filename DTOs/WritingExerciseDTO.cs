// 26/11/2025 - 21:52:50
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.DTOs;

public class WritingExerciseDTO
{
    public Guid? Id { get; set; }

    [Required]
    public Guid TopicId { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    [Required]
    public string Requirement { get; set; }
    
    [Required]
    public int Level { get; set; } = 1; // 1: Beginner, 2: Intermediate, 3: Advanced
    
    public string LinkImage {get; set; } // src display Img

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}