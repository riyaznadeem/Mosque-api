using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MosqueDonationAPI.Entities;

public class Donation
{
    public int Id { get; set; }

    [Required]
    public int MosqueId { get; set; }

    [Required]
    public int ReceivedByUserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DonorName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string? DonorNameUrdu { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? DonorPhone { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Purpose { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime DonationDate { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? ReceiptNumber { get; set; }  // <-- DEFAULT HATA DIYA

    public string? PaymentMode { get; set; }

    public bool IsPrinted { get; set; } = false;

    public DateTime? PrintedAt { get; set; }

    // Navigation
    public Mosque Mosque { get; set; } = null!;
    public User ReceivedBy { get; set; } = null!;
}