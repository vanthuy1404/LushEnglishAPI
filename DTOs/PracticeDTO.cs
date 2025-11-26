// 19/11/2025 - 20:53:01
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.DTOs;

public class PracticeDTO
{
    public Guid? Id { get; set; }
    [Required] public Guid TopicId { get; set; }
    [Required, MaxLength(255)] public string Name { get; set; }
    [Required] public int PracticeType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuestionDTO>? Questions { get; set; }
    public List<ChattingExerciseDTO>? Exercises { get; set; }
    public List<WritingExerciseDTO>? WritingExercises { get; set; }
}