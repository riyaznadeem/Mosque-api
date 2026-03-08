using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class ClassChild : BaseEntity
{
    public int ClassId { get; set; }
    public virtual Class Class { get; set; } = null!;

    public int ChildId { get; set; }
    public virtual Child Child { get; set; } = null!;

    // Enrollment Details
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletionDate { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; } = "Active"; // Active, Completed, Dropped, Transferred

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Roll Number in class
    public int? RollNumber { get; set; }
}