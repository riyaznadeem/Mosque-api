using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MosqueDonationAPI.Entities;

public class ChildFee : BaseEntity
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

    // Fee Month & Year
    [Required]
    public int Year { get; set; }

    [Required]
    public int Month { get; set; } // 1-12

    // Fee Types
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TuitionFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AdmissionFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExaminationFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BooksFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? UniformFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OtherFees { get; set; }

    [MaxLength(500)]
    public string? OtherFeesDescription { get; set; }

    // Discounts
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ScholarshipDiscount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SiblingDiscount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OtherDiscount { get; set; }

    [MaxLength(500)]
    public string? DiscountRemarks { get; set; }

    // Calculated Properties
    public decimal TotalFees => TuitionFee + (AdmissionFee ?? 0) + (ExaminationFee ?? 0) +
                                (BooksFee ?? 0) + (UniformFee ?? 0) + (OtherFees ?? 0);
    public decimal TotalDiscounts => (ScholarshipDiscount ?? 0) + (SiblingDiscount ?? 0) + (OtherDiscount ?? 0);
    public decimal NetPayable => TotalFees - TotalDiscounts;

    // Payment Tracking
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; } = 0;

    public decimal Balance => NetPayable - AmountPaid;

    // Payment Status: Pending, Paid, PartiallyPaid, Overdue, Waived
    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending";

    public DateTime? LastPaymentDate { get; set; }

    [MaxLength(100)]
    public string? LastPaymentMethod { get; set; }

    // Due Date for payment
    public DateTime? DueDate { get; set; }

    // Late Fee
    [Column(TypeName = "decimal(18,2)")]
    public decimal? LateFee { get; set; }

    // Who processed the fee
    public int? ProcessedById { get; set; }
    public virtual User? ProcessedBy { get; set; }

    // Navigation
    public virtual ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();
}