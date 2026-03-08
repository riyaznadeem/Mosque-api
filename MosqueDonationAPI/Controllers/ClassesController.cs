using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Common;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClassesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClassListResponse>>> GetClasses(
        [FromQuery] int mosqueId,
        [FromQuery] string? search,
        [FromQuery] int? teacherId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Classes
            .Where(c => c.MosqueId == mosqueId && c.IsActive)
            .Include(c => c.ClassTeacher)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.Section != null && c.Section.Contains(search)));

        if (teacherId.HasValue)
            query = query.Where(c => c.ClassTeacherId == teacherId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClassListResponse
            {
                Id = c.Id,
                Name = c.Name,
                Section = c.Section,
                MaxCapacity = c.MaxCapacity,
                ClassTeacherId = c.ClassTeacherId,
                ClassTeacherName = c.ClassTeacher != null ? c.ClassTeacher.FullName : null,
                StartTime = c.StartTime,
                EndTime = c.EndTime,
                EnrolledChildrenCount = c.ClassChildren.Count(cc => cc.IsActive && cc.Status == "Active"),
                SubjectCount = c.ClassSubjects.Count(cs => cs.IsActive)
            })
            .ToListAsync();

        return Ok(new PagedResult<ClassListResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClassDetailResponse>> GetClass(int id)
    {
        var classEntity = await _context.Classes
            .Include(c => c.ClassTeacher)
            .Include(c => c.ClassSubjects)
                .ThenInclude(cs => cs.Subject)
            .Include(c => c.ClassSubjects)
                .ThenInclude(cs => cs.AssignedImaam)
            .Include(c => c.ClassChildren.Where(cc => cc.IsActive))
                .ThenInclude(cc => cc.Child)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (classEntity == null) return NotFound();

        return Ok(new ClassDetailResponse
        {
            Id = classEntity.Id,
            Name = classEntity.Name,
            Section = classEntity.Section,
            Description = classEntity.Description,
            MaxCapacity = classEntity.MaxCapacity,
            StartTime = classEntity.StartTime,
            EndTime = classEntity.EndTime,
            ClassTeacherId = classEntity.ClassTeacherId,
            ClassTeacherName = classEntity.ClassTeacher?.FullName,
            MosqueId = classEntity.MosqueId,
            Subjects = classEntity.ClassSubjects.Where(cs => cs.IsActive).Select(cs => new ClassSubjectResponse
            {
                SubjectId = cs.SubjectId,
                SubjectName = cs.Subject.Name,
                SubjectCode = cs.Subject.Code,
                AssignedImaamId = cs.AssignedImaamId,
                AssignedImaamName = cs.AssignedImaam?.FullName,
                DayOfWeek = cs.DayOfWeek,
                StartTime = cs.StartTime,
                EndTime = cs.EndTime
            }).ToList(),
            Children = classEntity.ClassChildren.Select(cc => new ChildBriefResponse
            {
                Id = cc.ChildId,
                FullName = cc.Child.FullName,
                RollNumber = cc.RollNumber,
                Status = cc.Status,
                GuardianPhone = cc.Child.GuardianPhone
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass(CreateClassRequest request)
    {
        if (!await _context.Mosques.AnyAsync(m => m.Id == request.MosqueId))
            return BadRequest(new { message = "Mosque not found" });

        if (request.ClassTeacherId.HasValue &&
            !await _context.Imaams.AnyAsync(i => i.Id == request.ClassTeacherId && i.IsActive))
            return BadRequest(new { message = "Class teacher not found" });

        var classEntity = new Class
        {
            Name = request.Name,
            Section = request.Section,
            Description = request.Description,
            MaxCapacity = request.MaxCapacity,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MosqueId = request.MosqueId,
            ClassTeacherId = request.ClassTeacherId
        };

        _context.Classes.Add(classEntity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Class created successfully", id = classEntity.Id });
    }

    [HttpPost("{id}/subjects")]
    public async Task<IActionResult> AssignSubjectToClass(int id, [FromBody] AssignSubjectRequest request)
    {
        var classEntity = await _context.Classes.FindAsync(id);
        if (classEntity == null) return NotFound(new { message = "Class not found" });

        // Check if subject exists and belongs to same mosque
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == request.SubjectId && s.MosqueId == classEntity.MosqueId && s.IsActive);

        if (subject == null)
            return BadRequest(new { message = "Subject not found or does not belong to this mosque" });

        // Check if already assigned
        var existing = await _context.ClassSubjects
            .FirstOrDefaultAsync(cs => cs.ClassId == id && cs.SubjectId == request.SubjectId && cs.IsActive);

        if (existing != null)
            return BadRequest(new { message = "Subject is already assigned to this class" });

        // Verify assigned imaam exists and belongs to same mosque
        if (request.AssignedImaamId.HasValue)
        {
            var imaam = await _context.Imaams
                .FirstOrDefaultAsync(i => i.Id == request.AssignedImaamId && i.MosqueId == classEntity.MosqueId && i.IsActive);

            if (imaam == null)
                return BadRequest(new { message = "Assigned imaam not found" });
        }

        var classSubject = new ClassSubject
        {
            ClassId = id,
            SubjectId = request.SubjectId,
            AssignedImaamId = request.AssignedImaamId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClassSubjects.Add(classSubject);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Subject assigned to class successfully",
            id = classSubject.ClassId // Composite key
        });
    }

    [HttpPut("{id}/subjects/{subjectId}")]
    public async Task<IActionResult> UpdateClassSubject(int id, int subjectId, [FromBody] UpdateClassSubjectRequest request)
    {
        var classSubject = await _context.ClassSubjects
            .FirstOrDefaultAsync(cs => cs.ClassId == id && cs.SubjectId == subjectId && cs.IsActive);

        if (classSubject == null) return NotFound(new { message = "Subject assignment not found" });

        // Verify new imaam if provided
        if (request.AssignedImaamId.HasValue)
        {
            var classEntity = await _context.Classes.FindAsync(id);
            var imaam = await _context.Imaams
                .FirstOrDefaultAsync(i => i.Id == request.AssignedImaamId && i.MosqueId == classEntity.MosqueId && i.IsActive);

            if (imaam == null)
                return BadRequest(new { message = "Assigned imaam not found" });
        }

        classSubject.AssignedImaamId = request.AssignedImaamId;
        classSubject.DayOfWeek = request.DayOfWeek;
        classSubject.StartTime = request.StartTime;
        classSubject.EndTime = request.EndTime;
        classSubject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Subject assignment updated successfully" });
    }

    [HttpDelete("{id}/subjects/{subjectId}")]
    public async Task<IActionResult> RemoveSubjectFromClass(int id, int subjectId)
    {
        var classSubject = await _context.ClassSubjects
            .FirstOrDefaultAsync(cs => cs.ClassId == id && cs.SubjectId == subjectId && cs.IsActive);

        if (classSubject == null) return NotFound(new { message = "Subject assignment not found" });

        classSubject.IsActive = false;
        classSubject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Subject removed from class successfully" });
    }

    [HttpGet("{id}/available-subjects")]
    public async Task<ActionResult<List<SubjectResponse>>> GetAvailableSubjects(int id)
    {
        var classEntity = await _context.Classes.FindAsync(id);
        if (classEntity == null) return NotFound();

        // Get subjects not already assigned to this class
        var assignedSubjectIds = await _context.ClassSubjects
            .Where(cs => cs.ClassId == id && cs.IsActive)
            .Select(cs => cs.SubjectId)
            .ToListAsync();

        var availableSubjects = await _context.Subjects
            .Where(s => s.MosqueId == classEntity.MosqueId
                && s.IsActive
                && !assignedSubjectIds.Contains(s.Id))
            .Select(s => new SubjectResponse
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Description = s.Description,
                DefaultImaamId = s.DefaultImaamId,
                DefaultImaamName = s.DefaultImaam != null ? s.DefaultImaam.FullName : null
            })
            .ToListAsync();

        return Ok(availableSubjects);
    }
}
