// 03/12/2025 - 16:41:30
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class SubmitChattingDTO
{
    public Guid? UserId { get; set; }
    public Guid ChattingExerciseId { get; set; }
    public List<ChatMessage>?  ChatMessages { get; set; }
    public string? Requirements { get; set; }
    public string? Objective { get; set; }
    public int MaxAiReplies { get; set; }
}
public class AIResponseMessage
{
    public string? Content { get; set; }
}