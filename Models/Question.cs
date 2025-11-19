// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.Models;

public class Question
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required] public Guid PracticeId { get; set; }

    [Required] public string QuestionText { get; set; }

    [Required] public string OptionA { get; set; }

    [Required] public string OptionB { get; set; }

    [Required] public string OptionC { get; set; }

    [Required] public string OptionD { get; set; }

    [Required, MaxLength(1)] public string CorrectOption { get; set; }

    // Navigation
    public Practice Practice { get; set; }
}