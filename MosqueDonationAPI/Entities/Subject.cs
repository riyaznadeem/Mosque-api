using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class Subject : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., Quran, Hadith, Fiqh, Arabic

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; } // Subject code like QRN-101

    // Foreign Key
    public int MosqueId { get; set; }
    public virtual Mosque Mosque { get; set; } = null!;

    // Optional: Main teacher for this subject
    public int? DefaultImaamId { get; set; }
    public virtual Imaam? DefaultImaam { get; set; }

    // Navigation
    public virtual ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
}