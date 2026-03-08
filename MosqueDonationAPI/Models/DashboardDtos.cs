namespace MosqueDonationAPI.Models;

public class DashboardDtos
{
}
public class DashboardSummaryDto
{
    public int TotalImaams { get; set; }
    public int TotalChildren { get; set; }
    public int TotalClasses { get; set; }
    public int TotalSubjects { get; set; }
    public decimal MonthlySalaryExpense { get; set; }
    public decimal MonthlyFeeCollection { get; set; }
    public decimal PendingFees { get; set; }
    public int TodayPresentImaams { get; set; }
    public int TodayPresentChildren { get; set; }
}

public class ChartDataDto
{
    public string[] Labels { get; set; } = Array.Empty<string>();
    public decimal[] Data { get; set; } = Array.Empty<decimal>();
}

public class MonthlyFinancialDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetAmount { get; set; }
}

public class AttendanceTrendDto
{
    public DateTime Date { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
}

public class RecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty; // "FeePayment", "SalaryPayment", "Enrollment", "Attendance"
    public string Description { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string? PerformedBy { get; set; }
}

public class ClassEnrollmentDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int EnrolledStudents { get; set; }
    public int? MaxCapacity { get; set; }
    public decimal OccupancyPercentage => MaxCapacity.HasValue && MaxCapacity > 0
        ? (decimal)EnrolledStudents / MaxCapacity.Value * 100
        : 0;
}

public class TopPerformerDto
{
    public int ImaamId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int ClassesCount { get; set; }
    public int SubjectsCount { get; set; }
    public decimal? AverageAttendance { get; set; }
}

public class DashboardResponseDto
{
    public DashboardSummaryDto Summary { get; set; } = new();
    public List<MonthlyFinancialDto> MonthlyFinancials { get; set; } = new();
    public List<ClassEnrollmentDto> ClassEnrollments { get; set; } = new();
    public List<TopPerformerDto> TopPerformers { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public ChartDataDto FeeCollectionByClass { get; set; } = new();
    public ChartDataDto AttendanceByStatus { get; set; } = new();
}
