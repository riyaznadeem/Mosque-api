using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Controllers.Children;

[ApiController]
[Route("api/[controller]")]
public class ChildAttendanceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChildAttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/child-attendance?classId=1&date=2024-01-15
    [HttpGet]
    public async Task<ActionResult<DailyAttendanceReportResponse>> GetDailyAttendance(
        [FromQuery] int classId,
        [FromQuery] DateTime date)
    {
        var classEntity = await _context.Classes
            .Include(c => c.ClassChildren.Where(cc => cc.IsActive && cc.Status == "Active"))
                .ThenInclude(cc => cc.Child)
            .FirstOrDefaultAsync(c => c.Id == classId && c.IsActive);

        if (classEntity == null) return NotFound("Class not found");

        var attendances = await _context.ChildAttendances
            .Where(a => a.ClassId == classId && a.Date.Date == date.Date && a.IsActive)
            .Include(a => a.MarkedBy)
            .ToListAsync();

        var enrolledChildren = classEntity.ClassChildren.Select(cc => cc.Child).ToList();

        var attendanceDetails = enrolledChildren.Select(child =>
        {
            var attendance = attendances.FirstOrDefault(a => a.ChildId == child.Id);
            return new ChildAttendanceResponse
            {
                Id = attendance?.Id ?? 0,
                ChildId = child.Id,
                ChildName = child.FullName,
                RollNumber = classEntity.ClassChildren.FirstOrDefault(cc => cc.ChildId == child.Id)?.RollNumber,
                ClassId = classId,
                ClassName = classEntity.Name,
                Date = date,
                Status = attendance?.Status ?? "NotMarked",
                CheckInTime = attendance?.CheckInTime,
                CheckOutTime = attendance?.CheckOutTime,
                Remarks = attendance?.Remarks,
                MarkedByName = attendance?.MarkedBy?.FullName
            };
        }).ToList();

        var presentCount = attendanceDetails.Count(a => a.Status == "Present");
        var absentCount = attendanceDetails.Count(a => a.Status == "Absent");
        var lateCount = attendanceDetails.Count(a => a.Status == "Late");

        return Ok(new DailyAttendanceReportResponse
        {
            Date = date,
            ClassId = classId,
            ClassName = classEntity.Name,
            TotalStudents = enrolledChildren.Count,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            LateCount = lateCount,
            Details = attendanceDetails
        });
    }

    // GET: api/child-attendance/child/5?year=2024&month=1
    [HttpGet("child/{childId}")]
    public async Task<ActionResult<ChildAttendanceSummaryResponse>> GetChildAttendanceSummary(
        int childId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var attendances = await _context.ChildAttendances
            .Where(a => a.ChildId == childId && a.Date >= startDate && a.Date <= endDate && a.IsActive)
            .ToListAsync();

        var child = await _context.Children.FindAsync(childId);
        if (child == null) return NotFound();

        var enrollment = await _context.ClassChildren
            .FirstOrDefaultAsync(cc => cc.ChildId == childId && cc.IsActive && cc.Status == "Active");

        var totalDays = attendances.Count;
        var presentDays = attendances.Count(a => a.Status == "Present");
        var absentDays = attendances.Count(a => a.Status == "Absent");
        var lateDays = attendances.Count(a => a.Status == "Late");

        var percentage = totalDays > 0 ? (double)presentDays / totalDays * 100 : 0;

        return Ok(new ChildAttendanceSummaryResponse
        {
            ChildId = childId,
            ChildName = child.FullName,
            RollNumber = enrollment?.RollNumber,
            TotalDays = totalDays,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            LateDays = lateDays,
            AttendancePercentage = Math.Round(percentage, 2)
        });
    }

    // POST: api/child-attendance/mark
    [HttpPost("mark")]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkChildAttendanceRequest request)
    {
        var existing = await _context.ChildAttendances
            .FirstOrDefaultAsync(a => a.ChildId == request.ChildId && a.Date.Date == request.Date.Date && a.IsActive);

        if (existing != null)
            return BadRequest(new { message = "Attendance already marked for this child on this date" });

        var child = await _context.Children.FindAsync(request.ChildId);
        if (child == null) return NotFound("Child not found");

        var classEntity = await _context.Classes.FindAsync(request.ClassId);
        if (classEntity == null) return NotFound("Class not found");

        var attendance = new ChildAttendance
        {
            ChildId = request.ChildId,
            ClassId = request.ClassId,
            MosqueId = classEntity.MosqueId,
            Date = request.Date,
            Status = request.Status,
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime,
            Remarks = request.Remarks,
            MarkedById = null,
        };

        _context.ChildAttendances.Add(attendance);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Attendance marked successfully", id = attendance.Id });
    }

    // POST: api/child-attendance/bulk
    [HttpPost("bulk")]
    public async Task<IActionResult> MarkBulkAttendance([FromBody] BulkMarkAttendanceRequest request)
    {
        var classEntity = await _context.Classes.FindAsync(request.ClassId);
        if (classEntity == null) return NotFound("Class not found");

        var markedCount = 0;

        foreach (var item in request.Attendances)
        {
            var existing = await _context.ChildAttendances
                .FirstOrDefaultAsync(a => a.ChildId == item.ChildId && a.Date.Date == request.Date.Date && a.IsActive);

            if (existing != null)
            {
                // Update existing
                existing.Status = item.Status;
                existing.CheckInTime = item.CheckInTime;
                existing.CheckOutTime = item.CheckOutTime;
                existing.Remarks = item.Remarks;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var attendance = new ChildAttendance
                {
                    ChildId = item.ChildId,
                    ClassId = request.ClassId,
                    MosqueId = classEntity.MosqueId,
                    Date = request.Date,
                    Status = item.Status,
                    CheckInTime = item.CheckInTime,
                    CheckOutTime = item.CheckOutTime,
                    Remarks = item.Remarks,
                    MarkedById = null
                };
                _context.ChildAttendances.Add(attendance);
            }
            markedCount++;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"{markedCount} attendances marked/updated successfully" });
    }

    // PUT: api/child-attendance/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAttendance(int id, [FromBody] MarkChildAttendanceRequest request)
    {
        var attendance = await _context.ChildAttendances.FindAsync(id);
        if (attendance == null) return NotFound();

        attendance.Status = request.Status;
        attendance.CheckInTime = request.CheckInTime;
        attendance.CheckOutTime = request.CheckOutTime;
        attendance.Remarks = request.Remarks;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Attendance updated successfully" });
    }

    private int GetCurrentImaamId()
    {
        // Implement based on your auth system
        return int.Parse(User.FindFirst("imaamId")?.Value ?? "0");
    }
}