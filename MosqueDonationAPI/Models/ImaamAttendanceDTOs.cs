using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class MarkImaamAttendanceRequest
{
    [Required]
    public int ImaamId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Present"; // Present, Absent, Late, OnLeave, HalfDay

    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateImaamAttendanceRequest
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Present";

    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}

public class ImaamAttendanceResponse
{
    public int Id { get; set; }
    public int ImaamId { get; set; }
    public string ImaamName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
    public string? MarkedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ImaamAttendanceSummaryResponse
{
    public int ImaamId { get; set; }
    public string ImaamName { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int OnLeaveDays { get; set; }
    public int HalfDayDays { get; set; }
    public double AttendancePercentage { get; set; }
}
