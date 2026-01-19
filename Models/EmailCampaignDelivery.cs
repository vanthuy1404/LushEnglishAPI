// 17/01/2026 - 23:08:32
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.Models;

public class EmailCampaignDelivery
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    [Required]
    public Guid CampaignId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required, MaxLength(255)]
    public string Email { get; set; } = "";
    public bool Status { get; set; } = true; // true: success, false: failed
    public DateTime SentAt { get; set; } = DateTime.UtcNow.AddHours(7);
    // Navigation (optional)
    public EmailCampaign? Campaign { get; set; }
}
