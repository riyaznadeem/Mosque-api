using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class DonationModels
{
}
public class CreateDonationRequest
{
    [Required]
    public int? MosqueId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DonorName { get; set; } = string.Empty;
    public string? DonorNameUrdu { get; set; } = string.Empty;
    

    [MaxLength(20)]
    public string? DonorPhone { get; set; }

    [Required]
    [Range(1, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Purpose { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? PaymentMode { get; set; } = "Cash";
}

public class DonationResponse
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string MosqueName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    public string? DonorNameUrdu { get; set; } = string.Empty;
    public string? DonorPhone { get; set; }
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DonationDate { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public string? PaymentMode { get; set; }
    public bool IsPrinted { get; set; }
}

public class ReceiptPrintRequest
{
    public int DonationId { get; set; }
}
public class UpdateDonationRequest
{
    [MaxLength(100)]
    public string? DonorName { get; set; }
    public string? DonorNameUrdu { get; set; }

    [MaxLength(20)]
    public string? DonorPhone { get; set; }

    [Range(1, 10000000)]
    public decimal? Amount { get; set; }

    [MaxLength(50)]
    public string? Purpose { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? PaymentMode { get; set; }

    public bool? IsPrinted { get; set; }
}
