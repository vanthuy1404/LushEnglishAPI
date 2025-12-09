// 30/11/2025 - 19:26:02
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class SubmitPracticeDTO
{
    public Guid UserId { get; set; } //Fe truyen di
    public Guid PracticeId { get; set; } // Fe truyen di
    public int TotalQuestions { get; set; } // Fe truyen di
    public List<SubmitQuestionDTO> submits { get; set; } // Fe truyen di
    public decimal? Score { get; set; }
    public int? NumberOfCorrects { get; set; }
}
public class SubmitQuestionDTO
{
    public Guid Id { get; set; }
    public string UserAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
}