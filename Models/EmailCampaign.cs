// 17/01/2026 - 23:06:05
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.Models;

public class EmailCampaign
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required, MaxLength(200)]
    public string Name { get; set; } = ""; // tên chiến dịch: "Promo Jan 2026"
    [Required, MaxLength(255)]
    public string Subject { get; set; } = ""; // tiêu đề email
    [Required]
    public string HtmlBody { get; set; } = ""; // nội dung html (khuyến mại)
    /// <summary>
    /// Ngày bắt đầu cho phép gửi (theo VN time). Ví dụ: 2026-01-18
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }
    /// <summary>
    /// Ngày kết thúc cho phép gửi (theo VN time). Ví dụ: 2026-01-25
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Giờ/phút trong ngày để gửi (VN). Ví dụ: 20:00:00
    /// Dùng TimeSpan để lưu "time-of-day".
    /// </summary>
    [Required]
    public TimeSpan SendTimeOfDay { get; set; } = new TimeSpan(20, 0, 0);
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
    public DateTime? UpdatedAt { get; set; }
}