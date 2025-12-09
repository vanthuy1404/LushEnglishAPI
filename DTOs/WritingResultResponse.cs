// 01/12/2025 - 20:11:21
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class WritingResultResponse
{
    public int Score { get; set; }
    public List<DetailEvaluate> DetailEvaluates { get; set; }
    public string OverallEvaluate {get; set;}
    public string CorrectVersion {get; set;}
}

public class DetailEvaluate
{
    public string Type { get; set; } // "error" or "suggestion"
    public string Original {get; set;}
    public string Corrected {get; set;}
    public string Explaination {get; set;}
}