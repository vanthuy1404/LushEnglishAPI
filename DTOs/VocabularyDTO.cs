// 19/11/2025 - 20:53:13
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class VocabularyDTO
{
    public Guid?  Id { get; set; }
    public string Word { get; set; }
    public string Phonetic { get; set; }
    public string Meaning { get; set; }
    public DateTime? CreatedAt { get; set; }
}