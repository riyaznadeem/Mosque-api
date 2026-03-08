using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Common;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SubjectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SubjectResponse>>> GetSubjects(
        [FromQuery] int mosqueId,
        [FromQuery] string? search,
        [FromQuery] int? imaamId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Subjects
            .Where(s => s.MosqueId == mosqueId && s.IsActive)
            .Include(s => s.DefaultImaam)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search) || (s.Code != null && s.Code.Contains(search)));

        if (imaamId.HasValue)
            query = query.Where(s => s.DefaultImaamId == imaamId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SubjectResponse
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Description = s.Description,
                DefaultImaamId = s.DefaultImaamId,
                DefaultImaamName = s.DefaultImaam != null ? s.DefaultImaam.FullName : null,
                ClassCount = s.ClassSubjects.Count(cs => cs.IsActive)
            })
            .ToListAsync();

        return Ok(new PagedResult<SubjectResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubject(CreateSubjectRequest request)
    {
        if (!await _context.Mosques.AnyAsync(m => m.Id == request.MosqueId))
            return BadRequest(new { message = "Mosque not found" });

        if (request.DefaultImaamId.HasValue &&
            !await _context.Imaams.AnyAsync(i => i.Id == request.DefaultImaamId && i.IsActive))
            return BadRequest(new { message = "Imaam not found" });

        var subject = new Subject
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            MosqueId = request.MosqueId,
            DefaultImaamId = request.DefaultImaamId
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Subject created successfully", id = subject.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubject(int id, UpdateSubjectRequest request)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        subject.Name = request.Name;
        subject.Code = request.Code;
        subject.Description = request.Description;
        subject.DefaultImaamId = request.DefaultImaamId;
        subject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Subject updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        subject.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Subject deleted successfully" });
    }
}