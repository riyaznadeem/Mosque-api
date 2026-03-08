using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class MarkChildAttendanceRequest
{
    [Required]
    public int ChildId { get; set; }

    [Required]
    public int ClassId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Present";

    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}

public class BulkMarkAttendanceRequest
{
    [Required]
    public int ClassId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public List<ChildAttendanceItem> Attendances { get; set; } = new();
}

public class ChildAttendanceItem
{
    public int ChildId { get; set; }
    public string Status { get; set; } = "Present";
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}

public class ChildAttendanceResponse
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public int? RollNumber { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
    public string? MarkedByName { get; set; }
}

public class ChildAttendanceSummaryResponse
{
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public int? RollNumber { get; set; }
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public double AttendancePercentage { get; set; }
}

public class DailyAttendanceReportResponse
{
    public DateTime Date { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public List<ChildAttendanceResponse> Details { get; set; } = new();
}