// 03/12/2025 - 23:23:33
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class ChattingResultResponse
{
    public int Score { get; set; } 
    public string OverallEvaluate { get; set; } // Nhận xét chung
    public List<string> DetailEvaluates { get; set; } // Các điểm chi tiết
    public List<string> Suggestions { get; set; } // Gợi ý cải thiện
}