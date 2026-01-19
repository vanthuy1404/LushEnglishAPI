// 19/11/2025 - updated
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LushEnglishAPI.DTOs;

public class TopicDTO
{
    public Guid? Id { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    public string? Description { get; set; }
    public string? YoutubeUrl { get; set; }

    [Required]
    public int Level { get; set; } = 1; // 1: Beginner, 2: Intermediate, 3: Advanced

    public string? LinkImage { get; set; } // src display Img
    public DateTime CreatedAt { get; set; }

    // ===== NEW: Course info (optional) =====
    public Guid? CourseId { get; set; }
    public string? CourseName { get; set; }

    // Đây là property nhận file binary từ React gửi lên (key là 'image')
    public IFormFile? Image { get; set; }

    public List<PracticeDTO>? Practices { get; set; }
    public List<WritingExerciseDTO>? WritingExercises { get; set; }
    public List<ChattingExerciseDTO>? ChattingExercises { get; set; }
    public List<VocabularyDTO>? Vocabularies { get; set; }
}