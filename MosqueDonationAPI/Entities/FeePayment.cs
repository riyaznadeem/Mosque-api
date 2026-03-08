using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MosqueDonationAPI.Entities;

public class FeePayment : BaseEntity
{
    [Required]
    public int ChildFeeId { get; set; }
    public virtual ChildFee ChildFee { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public int? ReceivedById { get; set; }
    public virtual User? ReceivedBy { get; set; }
}
