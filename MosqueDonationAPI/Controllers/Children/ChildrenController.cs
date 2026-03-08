using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Common;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Controllers.Children;

[ApiController]
[Route("api/[controller]")]
public class ChildrenController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChildrenController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ChildResponse>>> GetChildren(
        [FromQuery] int mosqueId,
        [FromQuery] string? search,
        [FromQuery] int? classId,
        [FromQuery] string? gender,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Children
            .Where(c => c.MosqueId == mosqueId && c.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.FullName.Contains(search) ||
                (c.GuardianPhone != null && c.GuardianPhone.Contains(search)));

        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(c => c.Gender == gender);

        if (classId.HasValue)
            query = query.Where(c => c.ClassEnrollments.Any(ce => ce.ClassId == classId && ce.IsActive));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ChildResponse
            {
                Id = c.Id,
                FullName = c.FullName,
                DateOfBirth = c.DateOfBirth,
                Gender = c.Gender,
                GuardianName = c.GuardianName,
                GuardianPhone = c.GuardianPhone,
                EnrollmentDate = c.EnrollmentDate,
                ActiveClassesCount = c.ClassEnrollments.Count(ce => ce.IsActive && ce.Status == "Active")
            })
            .ToListAsync();

        return Ok(new PagedResult<ChildResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChildDetailResponse>> GetChild(int id)
    {
        var child = await _context.Children
            .Include(c => c.ClassEnrollments.Where(ce => ce.IsActive))
                .ThenInclude(ce => ce.Class)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (child == null) return NotFound();

        return Ok(new ChildDetailResponse
        {
            Id = child.Id,
            FullName = child.FullName,
            DateOfBirth = child.DateOfBirth,
            Gender = child.Gender,
            GuardianName = child.GuardianName,
            GuardianPhone = child.GuardianPhone,
            GuardianEmail = child.GuardianEmail,
            Address = child.Address,
            EnrollmentDate = child.EnrollmentDate,
            MosqueId = child.MosqueId,
            Enrollments = child.ClassEnrollments.Select(ce => new EnrollmentResponse
            {
                ClassId = ce.ClassId,
                ClassName = ce.Class.Name,
                Section = ce.Class.Section,
                RollNumber = ce.RollNumber,
                Status = ce.Status,
                EnrollmentDate = ce.EnrollmentDate
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateChild(CreateChildRequest request)
    {
        if (!await _context.Mosques.AnyAsync(m => m.Id == request.MosqueId))
            return BadRequest(new { message = "Mosque not found" });

        var child = new Child
        {
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            GuardianName = request.GuardianName,
            GuardianPhone = request.GuardianPhone,
            GuardianEmail = request.GuardianEmail,
            Address = request.Address,
            MosqueId = request.MosqueId
        };

        _context.Children.Add(child);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Child registered successfully", id = child.Id });
    }

    [HttpPost("enroll")]
    public async Task<IActionResult> EnrollChild(EnrollChildRequest request)
    {
        var child = await _context.Children.FindAsync(request.ChildId);
        if (child == null) return NotFound("Child not found");

        var classEntity = await _context.Classes
            .Include(c => c.ClassChildren)
            .FirstOrDefaultAsync(c => c.Id == request.ClassId && c.IsActive);

        if (classEntity == null) return NotFound("Class not found");

        // Check capacity
        if (classEntity.MaxCapacity.HasValue &&
            classEntity.ClassChildren.Count(cc => cc.IsActive && cc.Status == "Active") >= classEntity.MaxCapacity)
            return BadRequest(new { message = "Class has reached maximum capacity" });

        // Check if already enrolled
        if (await _context.ClassChildren.AnyAsync(cc =>
            cc.ChildId == request.ChildId &&
            cc.ClassId == request.ClassId &&
            cc.IsActive))
            return BadRequest(new { message = "Child is already enrolled in this class" });

        var enrollment = new ClassChild
        {
            ChildId = request.ChildId,
            ClassId = request.ClassId,
            RollNumber = request.RollNumber,
            Notes = request.Notes,
            Status = "Active"
        };

        _context.ClassChildren.Add(enrollment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Child enrolled successfully" });
    }

    [HttpPut("enrollments/{classId}/{childId}")]
    public async Task<IActionResult> UpdateEnrollment(int classId, int childId, UpdateEnrollmentRequest request)
    {
        var enrollment = await _context.ClassChildren
            .FirstOrDefaultAsync(cc => cc.ClassId == classId && cc.ChildId == childId);

        if (enrollment == null) return NotFound();

        enrollment.RollNumber = request.RollNumber;
        enrollment.Status = request.Status;
        enrollment.Notes = request.Notes;
        enrollment.CompletionDate = request.Status == "Completed" ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Enrollment updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChild(int id)
    {
        var child = await _context.Children.FindAsync(id);
        if (child == null) return NotFound();

        child.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Child record deleted successfully" });
    }
}
