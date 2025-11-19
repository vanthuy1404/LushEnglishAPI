// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.Models;

public class Topic
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required, MaxLength(255)] public string Name { get; set; }

    public string Description { get; set; }
    public string YoutubeUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Practice>? Practices { get; set; }
    public ICollection<Vocabulary>? Vocabularies { get; set; }
}