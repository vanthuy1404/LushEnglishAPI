// 17/01/2026 - 17:16:23
// DANGTHUY

namespace LushEnglishAPI.DTOs;

public class StatisticDTO
{
    public int TotalUsers { get; set; }
    public int TotalCourses { get; set; }
    public int TotalTopics { get; set; }
    public int TotalPractices { get; set; }

    public int NumberOfUsersWeek1 { get; set; }
    public int NumberOfUsersWeek2 { get; set; }
    public int NumberOfUsersWeek3 { get; set; }
    public int NumberOfUsersWeek4 { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal RevenueCurrentMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }

    public string Top1Course { get; set; }
    public int Top1CourseCount { get; set; }

    public string Top2Course { get; set; }
    public int Top2CourseCount { get; set; }

    public string Top3Course { get; set; }
    public int Top3CourseCount { get; set; }
}
