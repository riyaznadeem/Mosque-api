using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class ImaamAttendance : BaseEntity
{
    [Required]
    public int ImaamId { get; set; }
    public virtual Imaam Imaam { get; set; } = null!;

    [Required]
    public int MosqueId { get; set; }
    public virtual Mosque Mosque { get; set; } = null!;

    [Required]
    public DateTime Date { get; set; }

    // Status: Present, Absent, Late, OnLeave, HalfDay
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Present";

    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // Who marked the attendance
    public int? MarkedById { get; set; }
    public virtual User? MarkedBy { get; set; }
}