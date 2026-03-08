using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class CreateFeeRequest
{
    [Required]
    public int ChildId { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    public decimal TuitionFee { get; set; }

    public decimal? AdmissionFee { get; set; }
    public decimal? ExaminationFee { get; set; }
    public decimal? BooksFee { get; set; }
    public decimal? UniformFee { get; set; }
    public decimal? OtherFees { get; set; }
    public string? OtherFeesDescription { get; set; }

    // Discounts
    public decimal? ScholarshipDiscount { get; set; }
    public decimal? SiblingDiscount { get; set; }
    public decimal? OtherDiscount { get; set; }
    public string? DiscountRemarks { get; set; }

    public DateTime? DueDate { get; set; }
}

public class RecordFeePaymentRequest
{
    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    public string? Remarks { get; set; }

    public DateTime? PaymentDate { get; set; }
}

public class ChildFeeResponse
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public int? RollNumber { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;

    // Fees
    public decimal TuitionFee { get; set; }
    public decimal? AdmissionFee { get; set; }
    public decimal? ExaminationFee { get; set; }
    public decimal? BooksFee { get; set; }
    public decimal? UniformFee { get; set; }
    public decimal? OtherFees { get; set; }
    public decimal TotalFees { get; set; }

    // Discounts
    public decimal? ScholarshipDiscount { get; set; }
    public decimal? SiblingDiscount { get; set; }
    public decimal? OtherDiscount { get; set; }
    public decimal TotalDiscounts { get; set; }

    // Totals
    public decimal NetPayable { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance { get; set; }
    public decimal? LateFee { get; set; }

    // Status
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}

public class FeePaymentHistoryResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Remarks { get; set; }
    public string? ReceivedByName { get; set; }
}

public class ChildFeeSummaryResponse
{
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public decimal TotalFees { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalBalance { get; set; }
    public int PaidMonths { get; set; }
    public int PendingMonths { get; set; }
}

public class MonthlyFeeReportResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public decimal TotalExpectedFees { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalPending { get; set; }
    public decimal CollectionPercentage { get; set; }
}