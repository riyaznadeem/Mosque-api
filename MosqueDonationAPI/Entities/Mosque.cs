using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Entities;

public class Mosque
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string? UrduName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(10)]
    public string? Pincode { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
