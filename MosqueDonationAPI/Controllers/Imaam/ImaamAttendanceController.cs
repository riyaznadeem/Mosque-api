using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Common;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Controllers.Imaam;


[ApiController]
[Route("api/[controller]")]
public class ImaamAttendanceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImaamAttendanceController> _logger;

    public ImaamAttendanceController(ApplicationDbContext context, ILogger<ImaamAttendanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/imaam-attendance?mosqueId=1&date=2024-01-15&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<PagedResult<ImaamAttendanceResponse>>> GetAttendance(
        [FromQuery] int mosqueId,
        [FromQuery] DateTime? date,
        [FromQuery] int? imaamId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.ImaamAttendances
            .Where(a => a.MosqueId == mosqueId && a.IsActive)
            .Include(a => a.Imaam)
            .Include(a => a.MarkedBy)
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(a => a.Date.Date == date.Value.Date);

        if (imaamId.HasValue)
            query = query.Where(a => a.ImaamId == imaamId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Imaam.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ImaamAttendanceResponse
            {
                Id = a.Id,
                ImaamId = a.ImaamId,
                ImaamName = a.Imaam.FullName,
                Date = a.Date,
                Status = a.Status,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Remarks = a.Remarks,
                MarkedByName = a.MarkedBy != null ? a.MarkedBy.Username : null,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(new PagedResult<ImaamAttendanceResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    // GET: api/imaam-attendance/summary?imaamId=1&year=2024&month=1
    [HttpGet("summary")]
    public async Task<ActionResult<ImaamAttendanceSummaryResponse>> GetAttendanceSummary(
        [FromQuery] int imaamId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var attendances = await _context.ImaamAttendances
            .Where(a => a.ImaamId == imaamId && a.Date >= startDate && a.Date <= endDate && a.IsActive)
            .ToListAsync();

        var imaam = await _context.Imaams.FindAsync(imaamId);
        if (imaam == null) return NotFound();

        var totalDays = attendances.Count;
        var presentDays = attendances.Count(a => a.Status == "Present");
        var absentDays = attendances.Count(a => a.Status == "Absent");
        var lateDays = attendances.Count(a => a.Status == "Late");
        var onLeaveDays = attendances.Count(a => a.Status == "OnLeave");
        var halfDayDays = attendances.Count(a => a.Status == "HalfDay");

        var percentage = totalDays > 0 ? (presentDays + (halfDayDays * 0.5)) / totalDays * 100 : 0;

        return Ok(new ImaamAttendanceSummaryResponse
        {
            ImaamId = imaamId,
            ImaamName = imaam.FullName,
            TotalDays = totalDays,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            LateDays = lateDays,
            OnLeaveDays = onLeaveDays,
            HalfDayDays = halfDayDays,
            AttendancePercentage = Math.Round(percentage, 2)
        });
    }

    // POST: api/imaam-attendance/mark
    [HttpPost("mark")]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkImaamAttendanceRequest request)
    {
        // Normalize date to remove time component for comparison
        var requestDate = request.Date.Date;

        // Check if already marked
        var existing = await _context.ImaamAttendances
            .FirstOrDefaultAsync(a => a.ImaamId == request.ImaamId
                && a.Date.Date == requestDate
                && a.IsActive);

        if (existing != null)
            return BadRequest(new { message = "Attendance already marked for this date" });

        var imaam = await _context.Imaams.FindAsync(request.ImaamId);
        if (imaam == null) return NotFound("Imaam not found");

        var attendance = new ImaamAttendance
        {
            ImaamId = request.ImaamId,
            MosqueId = imaam.MosqueId,
            // Store date without time component
            Date = requestDate,
            Status = request.Status,
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime,
            Remarks = request.Remarks,
            MarkedById = GetCurrentUserId()
        };

        _context.ImaamAttendances.Add(attendance);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Attendance marked successfully", id = attendance.Id });
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> MarkBulkAttendance([FromBody] List<MarkImaamAttendanceRequest> requests)
    {
        var userId = GetCurrentUserId();
        var markedCount = 0;
        var skippedCount = 0;

        foreach (var request in requests)
        {
            var requestDate = request.Date.Date;

            var existing = await _context.ImaamAttendances
                .FirstOrDefaultAsync(a => a.ImaamId == request.ImaamId
                    && a.Date.Date == requestDate
                    && a.IsActive);

            if (existing != null)
            {
                skippedCount++;
                continue;
            }

            var imaam = await _context.Imaams.FindAsync(request.ImaamId);
            if (imaam == null)
            {
                skippedCount++;
                continue;
            }

            var attendance = new ImaamAttendance
            {
                ImaamId = request.ImaamId,
                MosqueId = imaam.MosqueId,
                Date = requestDate,
                Status = request.Status,
                CheckInTime = request.CheckInTime,
                CheckOutTime = request.CheckOutTime,
                Remarks = request.Remarks,
                MarkedById = userId
            };

            _context.ImaamAttendances.Add(attendance);
            markedCount++;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"{markedCount} attendances marked successfully",
            markedCount,
            skippedCount
        });
    }

    // PUT: api/imaam-attendance/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAttendance(int id, [FromBody] UpdateImaamAttendanceRequest request)
    {
        var attendance = await _context.ImaamAttendances.FindAsync(id);
        if (attendance == null) return NotFound();

        attendance.Status = request.Status;
        attendance.CheckInTime = request.CheckInTime;
        attendance.CheckOutTime = request.CheckOutTime;
        attendance.Remarks = request.Remarks;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Attendance updated successfully" });
    }

    // DELETE: api/imaam-attendance/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttendance(int id)
    {
        var attendance = await _context.ImaamAttendances.FindAsync(id);
        if (attendance == null) return NotFound();

        attendance.IsActive = false;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Attendance deleted successfully" });
    }

    private int GetCurrentUserId()
    {
        // Implement based on your auth system
        return int.Parse(User.FindFirst("userId")?.Value ?? "0");
    }
}
