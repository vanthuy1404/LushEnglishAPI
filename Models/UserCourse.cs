// 13/01/2026 - 22:27:54
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.Models;

public class UserCourse
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    // số tiền thực trả tại thời điểm mua (để sau này Course.Price thay đổi không ảnh hưởng)
    public decimal Amount { get; set; }

    // MoMo fields tối thiểu để trace giao dịch
    public string? MomoOrderId { get; set; }     // orderId bạn tạo gửi MoMo
    public string? MomoRequestId { get; set; }   // requestId bạn tạo gửi MoMo
    public string? MomoTransId { get; set; }     // transId MoMo trả về (nếu có)
    public string Status { get; set; } = "PENDING"; // PENDING / PAID / FAILED

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
    public DateTime? PaidAt { get; set; }

    // Navigation (optional)
    public Course? Course { get; set; }
    // public User? User { get; set; } // nếu bạn có User entity
}
