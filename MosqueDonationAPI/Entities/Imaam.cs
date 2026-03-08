using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class Imaam : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(200)]
    public string? Qualification { get; set; }

    public DateTime? JoiningDate { get; set; }

    // Foreign Key
    public int MosqueId { get; set; }
    public virtual Mosque Mosque { get; set; } = null!;

    // Navigation
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}