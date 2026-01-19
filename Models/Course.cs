// 13/01/2026 - 22:14:01
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.Models;

public class Course
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required, MaxLength(255)]
    public string Name { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? LinkImg { get; set; } 

    public decimal? Discount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Topic>? Topics { get; set; }
}
