using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MosqueDonationAPI.Entities;

public class ImaamSalary : BaseEntity
{
    [Required]
    public int ImaamId { get; set; }
    public virtual Imaam Imaam { get; set; } = null!;

    [Required]
    public int MosqueId { get; set; }
    public virtual Mosque Mosque { get; set; } = null!;

    // Salary Month & Year
    [Required]
    public int Year { get; set; }

    [Required]
    public int Month { get; set; } // 1-12

    // Basic Salary
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    // Allowances
    [Column(TypeName = "decimal(18,2)")]
    public decimal? HousingAllowance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TransportAllowance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OtherAllowances { get; set; }

    // Deductions
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AbsenceDeduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LateDeduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OtherDeductions { get; set; }

    [MaxLength(500)]
    public string? DeductionRemarks { get; set; }

    // Calculated Properties
    public decimal TotalAllowances => (HousingAllowance ?? 0) + (TransportAllowance ?? 0) + (OtherAllowances ?? 0);
    public decimal TotalDeductions => (AbsenceDeduction ?? 0) + (LateDeduction ?? 0) + (OtherDeductions ?? 0);
    public decimal GrossSalary => BasicSalary + TotalAllowances;
    public decimal NetSalary => GrossSalary - TotalDeductions;

    // Payment Status: Pending, Paid, PartiallyPaid
    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending";

    public DateTime? PaymentDate { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; } // Cash, BankTransfer, Check

    [MaxLength(500)]
    public string? PaymentRemarks { get; set; }

    // Who processed the salary
    public int? ProcessedById { get; set; }
    public virtual User? ProcessedBy { get; set; }
}
