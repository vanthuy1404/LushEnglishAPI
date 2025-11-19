// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.Models;

public class Result
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required] public Guid UserId { get; set; }

    [Required] public Guid PracticeId { get; set; }

    [Required] public int PracticeType { get; set; } // match Practice.PracticeType

    public decimal? Score { get; set; }
    public int? TotalQuestions { get; set; }
    public int? CorrectAnswers { get; set; }

    public string WritingText { get; set; }
    public string WritingFeedback { get; set; }
    public decimal? WritingScore { get; set; }

    public string ChatHistoryJson { get; set; }
    public string ChatEvaluation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User User { get; set; }
    public Practice Practice { get; set; }
}