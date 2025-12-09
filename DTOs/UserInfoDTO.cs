// 04/12/2025 - 16:22:45
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class UserInfoDTO
{
    public Guid? Id {get; set;}
    public string FullName {get; set;}
    public string Email {get; set;}
    public string AvatarUrl {get; set;}
    public DateTime? CreatedAt {get; set;}
    public List<UserResultDTO>? Results {get; set;}
    public decimal? TotalExp {get; set;} // Tổng tất cả score
    public decimal? AverageScore {get; set;} // Điểm trung bình tất cả bài tập
    public decimal? AverageScoreMultipleChoice {get; set;} // Điểm trung bình tất cả bài tập type 1
    public decimal? AverageScoreWriting {get; set;} // Điểm trung bình tất cả bài tập type 2
    public decimal? AverageScoreChatting {get; set;} // Điểm trung bình tất cả bài tập type 3
}
public class UserResultDTO
{
    public Guid Id {get; set;}
    public Guid TargetId {get; set;} // Link to practices, writingConfigs or chattingConfigs
    public int PracticeType   {get; set;} // 1: Multiple choice 2: Writing 3: Chatting
    public string PracticeName {get; set;}
    public decimal? Score {get; set;}
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}
}
public class UpdateProfileDTO
{
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public IFormFile? Avatar { get; set; } // Cho phép null nếu user chỉ muốn sửa tên
}