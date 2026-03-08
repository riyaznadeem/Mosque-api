using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class Child : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; } // Male, Female

    // Guardian Information
    [MaxLength(100)]
    public string? GuardianName { get; set; }

    [MaxLength(20)]
    public string? GuardianPhone { get; set; }

    [MaxLength(200)]
    public string? GuardianEmail { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    // Enrollment Date
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    // Foreign Key
    public int MosqueId { get; set; }
    public virtual Mosque Mosque { get; set; } = null!;

    // Navigation
    public virtual ICollection<ClassChild> ClassEnrollments { get; set; } = new List<ClassChild>();
}