using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Common;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Controllers.Imaam;

[ApiController]
[Route("api/[controller]")]
public class ImaamSalaryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ImaamSalaryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/imaam-salary?mosqueId=1&year=2024&month=1&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<PagedResult<ImaamSalaryResponse>>> GetSalaries(
        [FromQuery] int mosqueId,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string? paymentStatus,
        [FromQuery] int? imaamId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.ImaamSalaries
            .Where(s => s.MosqueId == mosqueId && s.IsActive)
            .Include(s => s.Imaam)
            .AsQueryable();

        if (year.HasValue)
            query = query.Where(s => s.Year == year);
        if (month.HasValue)
            query = query.Where(s => s.Month == month);
        if (!string.IsNullOrEmpty(paymentStatus))
            query = query.Where(s => s.PaymentStatus == paymentStatus);
        if (imaamId.HasValue)
            query = query.Where(s => s.ImaamId == imaamId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ImaamSalaryResponse
            {
                Id = s.Id,
                ImaamId = s.ImaamId,
                ImaamName = s.Imaam.FullName,
                Year = s.Year,
                Month = s.Month,
                MonthName = new DateTime(s.Year, s.Month, 1).ToString("MMMM"),
                BasicSalary = s.BasicSalary,
                HousingAllowance = s.HousingAllowance,
                TransportAllowance = s.TransportAllowance,
                OtherAllowances = s.OtherAllowances,
                TotalAllowances = s.TotalAllowances,
                AbsenceDeduction = s.AbsenceDeduction,
                LateDeduction = s.LateDeduction,
                OtherDeductions = s.OtherDeductions,
                TotalDeductions = s.TotalDeductions,
                GrossSalary = s.GrossSalary,
                NetSalary = s.NetSalary,
                PaymentStatus = s.PaymentStatus,
                PaymentDate = s.PaymentDate,
                PaymentMethod = s.PaymentMethod
            })
            .ToListAsync();

        return Ok(new PagedResult<ImaamSalaryResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    // GET: api/imaam-salary/summary?imaamId=1
    [HttpGet("summary")]
    public async Task<ActionResult<ImaamSalarySummaryResponse>> GetSalarySummary([FromQuery] int imaamId)
    {
        var salaries = await _context.ImaamSalaries
            .Where(s => s.ImaamId == imaamId && s.IsActive)
            .ToListAsync();

        var imaam = await _context.Imaams.FindAsync(imaamId);
        if (imaam == null) return NotFound();

        var totalMonths = salaries.Count;
        var totalPaid = salaries.Where(s => s.PaymentStatus == "Paid").Sum(s => s.NetSalary);
        var totalPending = salaries.Where(s => s.PaymentStatus == "Pending").Sum(s => s.NetSalary);
        var averageMonthly = totalMonths > 0 ? salaries.Average(s => s.NetSalary) : 0;

        return Ok(new ImaamSalarySummaryResponse
        {
            ImaamId = imaamId,
            ImaamName = imaam.FullName,
            TotalMonths = totalMonths,
            TotalPaid = totalPaid,
            TotalPending = totalPending,
            AverageMonthlySalary = Math.Round(averageMonthly, 2)
        });
    }

    // POST: api/imaam-salary
    [HttpPost]
    public async Task<IActionResult> CreateSalary([FromBody] CreateSalaryRequest request)
    {
        // Check if salary already exists for this month
        var existing = await _context.ImaamSalaries
            .FirstOrDefaultAsync(s => s.ImaamId == request.ImaamId && s.Year == request.Year && s.Month == request.Month && s.IsActive);

        if (existing != null)
            return BadRequest(new { message = "Salary already exists for this month" });

        var imaam = await _context.Imaams.FindAsync(request.ImaamId);
        if (imaam == null) return NotFound("Imaam not found");

        // Auto-calculate deductions based on attendance
        var attendanceSummary = await GetAttendanceSummaryInternal(request.ImaamId, request.Year, request.Month);
        var absenceDeduction = CalculateAbsenceDeduction(request.BasicSalary, attendanceSummary.AbsentDays);
        var lateDeduction = CalculateLateDeduction(request.BasicSalary, attendanceSummary.LateDays);

        var salary = new ImaamSalary
        {
            ImaamId = request.ImaamId,
            MosqueId = imaam.MosqueId,
            Year = request.Year,
            Month = request.Month,
            BasicSalary = request.BasicSalary,
            HousingAllowance = request.HousingAllowance,
            TransportAllowance = request.TransportAllowance,
            OtherAllowances = request.OtherAllowances,
            AbsenceDeduction = absenceDeduction > 0 ? absenceDeduction : request.AbsenceDeduction,
            LateDeduction = lateDeduction > 0 ? lateDeduction : request.LateDeduction,
            OtherDeductions = request.OtherDeductions,
            DeductionRemarks = request.DeductionRemarks,
            PaymentStatus = "Pending",
            ProcessedById = GetCurrentUserId()
        };

        _context.ImaamSalaries.Add(salary);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Salary created successfully", id = salary.Id });
    }

    // PUT: api/imaam-salary/5/pay
    [HttpPut("{id}/pay")]
    public async Task<IActionResult> ProcessPayment(int id, [FromBody] ProcessSalaryPaymentRequest request)
    {
        var salary = await _context.ImaamSalaries.FindAsync(id);
        if (salary == null) return NotFound();

        salary.PaymentStatus = request.PaymentStatus;
        salary.PaymentDate = request.PaymentDate ?? DateTime.UtcNow;
        salary.PaymentMethod = request.PaymentMethod;
        salary.PaymentRemarks = request.PaymentRemarks;
        salary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Payment processed successfully" });
    }

    // DELETE: api/imaam-salary/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSalary(int id)
    {
        var salary = await _context.ImaamSalaries.FindAsync(id);
        if (salary == null) return NotFound();

        salary.IsActive = false;
        salary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Salary deleted successfully" });
    }

    private async Task<ImaamAttendanceSummaryResponse> GetAttendanceSummaryInternal(int imaamId, int year, int month)
    {
        // Implementation from attendance controller
        return new ImaamAttendanceSummaryResponse();
    }

    private decimal CalculateAbsenceDeduction(decimal basicSalary, int absentDays)
    {
        var dailyRate = basicSalary / 30;
        return dailyRate * absentDays;
    }

    private decimal CalculateLateDeduction(decimal basicSalary, int lateDays)
    {
        var dailyRate = basicSalary / 30;
        return dailyRate * 0.5m * lateDays; // Half day deduction for late
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst("userId")?.Value ?? "0");
    }
}
