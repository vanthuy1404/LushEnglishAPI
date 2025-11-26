// 26/11/2025 - 21:51:54
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.DTOs;

public class QuestionDTO
{
    public Guid? Id { get; set; }
    [Required] public Guid PracticeId { get; set; }

    [Required] public string QuestionText { get; set; }

    [Required] public string OptionA { get; set; }

    [Required] public string OptionB { get; set; }

    [Required] public string OptionC { get; set; }

    [Required] public string OptionD { get; set; }

    [Required, MaxLength(1)] public string CorrectOption { get; set; }
}