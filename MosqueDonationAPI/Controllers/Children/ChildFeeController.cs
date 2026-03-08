using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Common;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Controllers.Children;

[ApiController]
[Route("api/[controller]")]
public class ChildFeeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChildFeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/child-fees?mosqueId=1&year=2024&month=1&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<PagedResult<ChildFeeResponse>>> GetFees(
        [FromQuery] int mosqueId,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string? paymentStatus,
        [FromQuery] int? childId,
        [FromQuery] int? classId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.ChildFees
            .Where(f => f.MosqueId == mosqueId && f.IsActive)
            .Include(f => f.Child)
            .Include(f => f.Class)
            .AsQueryable();

        if (year.HasValue)
            query = query.Where(f => f.Year == year);
        if (month.HasValue)
            query = query.Where(f => f.Month == month);
        if (!string.IsNullOrEmpty(paymentStatus))
            query = query.Where(f => f.PaymentStatus == paymentStatus);
        if (childId.HasValue)
            query = query.Where(f => f.ChildId == childId);
        if (classId.HasValue)
            query = query.Where(f => f.ClassId == classId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.Year)
            .ThenByDescending(f => f.Month)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new ChildFeeResponse
            {
                Id = f.Id,
                ChildId = f.ChildId,
                ChildName = f.Child.FullName,
                RollNumber = _context.ClassChildren
                    .Where(cc => cc.ChildId == f.ChildId && cc.ClassId == f.ClassId && cc.IsActive)
                    .Select(cc => cc.RollNumber)
                    .FirstOrDefault(),
                ClassName = f.Class.Name,
                Year = f.Year,
                Month = f.Month,
                MonthName = new DateTime(f.Year, f.Month, 1).ToString("MMMM"),
                TuitionFee = f.TuitionFee,
                AdmissionFee = f.AdmissionFee,
                ExaminationFee = f.ExaminationFee,
                BooksFee = f.BooksFee,
                UniformFee = f.UniformFee,
                OtherFees = f.OtherFees,
                TotalFees = f.TotalFees,
                ScholarshipDiscount = f.ScholarshipDiscount,
                SiblingDiscount = f.SiblingDiscount,
                OtherDiscount = f.OtherDiscount,
                TotalDiscounts = f.TotalDiscounts,
                NetPayable = f.NetPayable,
                AmountPaid = f.AmountPaid,
                Balance = f.Balance,
                LateFee = f.LateFee,
                PaymentStatus = f.PaymentStatus,
                DueDate = f.DueDate,
                LastPaymentDate = f.LastPaymentDate
            })
            .ToListAsync();

        return Ok(new PagedResult<ChildFeeResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    // GET: api/child-fees/5/history
    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<FeePaymentHistoryResponse>>> GetPaymentHistory(int id)
    {
        var payments = await _context.FeePayments
            .Where(p => p.ChildFeeId == id && p.IsActive)
            .Include(p => p.ReceivedBy)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new FeePaymentHistoryResponse
            {
                Id = p.Id,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Remarks = p.Remarks,
                ReceivedByName = p.ReceivedBy != null ? p.ReceivedBy.Username : null
            })
            .ToListAsync();

        return Ok(payments);
    }

    // GET: api/child-fees/summary?childId=1
    [HttpGet("summary")]
    public async Task<ActionResult<ChildFeeSummaryResponse>> GetFeeSummary([FromQuery] int childId)
    {
        var fees = await _context.ChildFees
            .Where(f => f.ChildId == childId && f.IsActive)
            .ToListAsync();

        var child = await _context.Children.FindAsync(childId);
        if (child == null) return NotFound();

        var totalFees = fees.Sum(f => f.TotalFees);
        var totalDiscounts = fees.Sum(f => f.TotalDiscounts);
        var totalPaid = fees.Sum(f => f.AmountPaid);
        var paidMonths = fees.Count(f => f.PaymentStatus == "Paid");
        var pendingMonths = fees.Count(f => f.PaymentStatus == "Pending" || f.PaymentStatus == "PartiallyPaid");

        return Ok(new ChildFeeSummaryResponse
        {
            ChildId = childId,
            ChildName = child.FullName,
            TotalFees = totalFees,
            TotalDiscounts = totalDiscounts,
            TotalPaid = totalPaid,
            TotalBalance = fees.Sum(f => f.Balance),
            PaidMonths = paidMonths,
            PendingMonths = pendingMonths
        });
    }


    [HttpGet("monthly-report")]
    public async Task<ActionResult<MonthlyFeeReportResponse>> GetMonthlyReport(
     [FromQuery] int year,
     [FromQuery] int month)
    {
        var fees = await _context.ChildFees
            .Where(f => f.Year == year && f.Month == month && f.IsActive)
            .ToListAsync();

        var totalExpected = fees.Sum(f => f.NetPayable);
        var totalDiscounts = fees.Sum(f => f.TotalDiscounts);
        var totalCollected = fees.Sum(f => f.AmountPaid);
        var totalPending = fees.Sum(f => f.Balance);

        // Keep as decimal throughout
        var percentage = totalExpected > 0
            ? (totalCollected / totalExpected) * 100
            : 0m;

        return Ok(new MonthlyFeeReportResponse
        {
            Year = year,
            Month = month,
            MonthName = new DateTime(year, month, 1).ToString("MMMM"),
            TotalStudents = fees.Count,
            TotalExpectedFees = totalExpected,
            TotalDiscounts = totalDiscounts,
            TotalCollected = totalCollected,
            TotalPending = totalPending,
            CollectionPercentage = Math.Round(percentage, 2)  // decimal stays decimal
        });
    }

    // POST: api/child-fees
    [HttpPost]
    public async Task<IActionResult> CreateFee([FromBody] CreateFeeRequest request)
    {
        var existing = await _context.ChildFees
            .FirstOrDefaultAsync(f => f.ChildId == request.ChildId && f.Year == request.Year && f.Month == request.Month && f.IsActive);

        if (existing != null)
            return BadRequest(new { message = "Fee already exists for this month" });

        var child = await _context.Children.FindAsync(request.ChildId);
        if (child == null) return NotFound("Child not found");

        var enrollment = await _context.ClassChildren
            .FirstOrDefaultAsync(cc => cc.ChildId == request.ChildId && cc.IsActive && cc.Status == "Active");

        if (enrollment == null)
            return BadRequest(new { message = "Child is not enrolled in any active class" });

        // Calculate late fee if past due date
        decimal? lateFee = null;
        if (request.DueDate.HasValue && DateTime.UtcNow > request.DueDate.Value.AddDays(1))
        {
            lateFee = CalculateLateFee(request.TuitionFee);
        }

        var fee = new ChildFee
        {
            ChildId = request.ChildId,
            ClassId = enrollment.ClassId,
            MosqueId = child.MosqueId,
            Year = request.Year,
            Month = request.Month,
            TuitionFee = request.TuitionFee,
            AdmissionFee = request.AdmissionFee,
            ExaminationFee = request.ExaminationFee,
            BooksFee = request.BooksFee,
            UniformFee = request.UniformFee,
            OtherFees = request.OtherFees,
            OtherFeesDescription = request.OtherFeesDescription,
            ScholarshipDiscount = request.ScholarshipDiscount,
            SiblingDiscount = request.SiblingDiscount,
            OtherDiscount = request.OtherDiscount,
            DiscountRemarks = request.DiscountRemarks,
            DueDate = request.DueDate,
            LateFee = lateFee,
            PaymentStatus = "Pending",
            ProcessedById = GetCurrentUserId()
        };

        _context.ChildFees.Add(fee);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Fee created successfully", id = fee.Id });
    }

    // POST: api/child-fees/5/pay
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> RecordPayment(int id, [FromBody] RecordFeePaymentRequest request)
    {
        var fee = await _context.ChildFees.FindAsync(id);
        if (fee == null) return NotFound();

        if (request.Amount > fee.Balance)
            return BadRequest(new { message = "Payment amount exceeds remaining balance" });

        // Create payment record
        var payment = new FeePayment
        {
            ChildFeeId = id,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
            PaymentMethod = request.PaymentMethod,
            Remarks = request.Remarks,
            ReceivedById = GetCurrentUserId()
        };

        _context.FeePayments.Add(payment);

        // Update fee record
        fee.AmountPaid += request.Amount;
        fee.LastPaymentDate = request.PaymentDate ?? DateTime.UtcNow;
        fee.LastPaymentMethod = request.PaymentMethod;
        fee.PaymentStatus = fee.Balance <= 0 ? "Paid" : "PartiallyPaid";
        fee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Payment recorded successfully",
            balance = fee.Balance,
            status = fee.PaymentStatus
        });
    }

    // DELETE: api/child-fees/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFee(int id)
    {
        var fee = await _context.ChildFees.FindAsync(id);
        if (fee == null) return NotFound();

        fee.IsActive = false;
        fee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Fee deleted successfully" });
    }

    private decimal CalculateLateFee(decimal tuitionFee)
    {
        return tuitionFee * 0.05m; // 5% late fee
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst("userId")?.Value ?? "0");
    }
}