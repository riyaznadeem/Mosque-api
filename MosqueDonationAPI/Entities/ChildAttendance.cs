using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class ChildAttendance : BaseEntity
{
    [Required]
    public int ChildId { get; set; }
    public virtual Child Child { get; set; } = null!;

    [Required]
    public int ClassId { get; set; }
    public virtual Class Class { get; set; } = null!;

    [Required]
    public int MosqueId { get; set; }
    public virtual Mosque Mosque { get; set; } = null!;

    [Required]
    public DateTime Date { get; set; }

    // Status: Present, Absent, Late, OnLeave, HalfDay, Excused
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Present";

    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // Who marked the attendance (Imaam/Teacher)
    public int? MarkedById { get; set; }
    public virtual Imaam? MarkedBy { get; set; }
}