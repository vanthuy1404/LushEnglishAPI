// 17/01/2026 - 23:15:06
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.DTOs;

public class CreateEmailCampaignDTO
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Required, MaxLength(255)]
    public string Subject { get; set; } = "";

    [Required]
    public string HtmlBody { get; set; } = "";

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public TimeSpan SendTimeOfDay { get; set; }

    public bool IsEnabled { get; set; } = true;
}

public class UpdateEmailCampaignDTO : CreateEmailCampaignDTO
{
    // y chang Create để dễ dùng
}