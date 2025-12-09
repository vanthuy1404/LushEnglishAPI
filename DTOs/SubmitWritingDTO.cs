// 01/12/2025 - 20:10:51
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class SubmitWritingDTO
{
    public string Requirements { get; set; }
    public string UserParagraphs {get; set;}
    public Guid UserId { get; set; }
    public Guid WritingExerciseId { get; set; }
}