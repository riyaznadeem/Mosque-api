using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User"; // Admin, Manager, User
    public int? AssignedMosqueId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public Mosque? AssignedMosque { get; set; }
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
