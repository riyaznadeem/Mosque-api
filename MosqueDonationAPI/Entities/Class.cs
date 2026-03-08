using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class Class : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Class 1", "Hifz Section A"

    [MaxLength(50)]
    public string? Section { get; set; } // e.g., "A", "B", "Boys", "Girls"

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? MaxCapacity { get; set; }

    // Schedule
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    // Foreign Keys
    public int MosqueId { get; set; }
    public virtual Mosque Mosque { get; set; } = null!;

    // Class Teacher (Imaam)
    public int? ClassTeacherId { get; set; }
    public virtual Imaam? ClassTeacher { get; set; }

    // Navigation
    public virtual ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
    public virtual ICollection<ClassChild> ClassChildren { get; set; } = new List<ClassChild>();
}