// 19/11/2025 - 20:33:20
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.DTOs;

public class TopicDTO
{
    public Guid? Id {get; set;}
    [Required, MaxLength(255)] public string Name { get; set; }
    public string Description { get; set; }
    public string YoutubeUrl { get; set; }
    [Required]
    public int Level { get; set; } = 1; // 1: Beginner, 2: Intermediate, 3: Advanced
    
    public string LinkImage {get; set; } // src display Img
    
    public DateTime CreatedAt { get; set; }
    public List<PracticeDTO>? Practices { get; set; }
    public List<WritingExerciseDTO>? WritingExercises { get; set; }
    public List<ChattingExerciseDTO>? ChattingExercises { get; set; }
    public List<VocabularyDTO>? Vocabularies { get; set; }
}