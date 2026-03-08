using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class CreateSalaryRequest
{
    [Required]
    public int ImaamId { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(0, 1000000)]
    public decimal BasicSalary { get; set; }

    public decimal? HousingAllowance { get; set; }
    public decimal? TransportAllowance { get; set; }
    public decimal? OtherAllowances { get; set; }
    public decimal? AbsenceDeduction { get; set; }
    public decimal? LateDeduction { get; set; }
    public decimal? OtherDeductions { get; set; }
    public string? DeductionRemarks { get; set; }
}

public class ProcessSalaryPaymentRequest
{
    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Paid"; // Paid, PartiallyPaid

    public DateTime? PaymentDate { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    public string? PaymentRemarks { get; set; }
}

public class ImaamSalaryResponse
{
    public int Id { get; set; }
    public int ImaamId { get; set; }
    public string ImaamName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal? HousingAllowance { get; set; }
    public decimal? TransportAllowance { get; set; }
    public decimal? OtherAllowances { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal? AbsenceDeduction { get; set; }
    public decimal? LateDeduction { get; set; }
    public decimal? OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
}

public class ImaamSalarySummaryResponse
{
    public int ImaamId { get; set; }
    public string ImaamName { get; set; } = string.Empty;
    public int TotalMonths { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal AverageMonthlySalary { get; set; }
}
