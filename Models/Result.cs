// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.Models;

public class Result
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required]
    public Guid UserId { get; set; }

    // Polymorphic "loose foreign key"
    [Required]
    public Guid TargetId { get; set; }   // PracticeId or WritingConfigId or ChattingConfigId

    [Required]
    public int PracticeType { get; set; } // 1 = Practice, 2 = Writing, 3 = Chatting

    public decimal? Score { get; set; }
    public int? TotalQuestions { get; set; }
    public int? CorrectAnswers { get; set; }

    public string WritingText { get; set; }
    public string WritingFeedback { get; set; } // Json => parse sang WritingResultResponse

    public string ChatHistoryJson { get; set; } //json
    public string ChatEvaluation { get; set; } //json => parse sang ChattingResultResponse

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public User User { get; set; }
}
