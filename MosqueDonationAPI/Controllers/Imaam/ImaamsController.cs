using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Common;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Controllers.Imaam;

[ApiController]
[Route("api/[controller]")]
public class ImaamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ImaamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ImaamResponse>>> GetImaams(
        [FromQuery] int mosqueId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Imaams
            .Where(i => i.MosqueId == mosqueId && i.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i =>
                i.FullName.Contains(search) ||
                (i.Email != null && i.Email.Contains(search)) ||
                (i.PhoneNumber != null && i.PhoneNumber.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ImaamResponse
            {
                Id = i.Id,
                FullName = i.FullName,
                PhoneNumber = i.PhoneNumber,
                Email = i.Email,
                Qualification = i.Qualification,
                MosqueId = i.MosqueId,
                AssignedClassesCount = i.Classes.Count(c => c.IsActive)
            })
            .ToListAsync();

        return Ok(new PagedResult<ImaamResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ImaamDetailResponse>> GetImaam(int id)
    {
        var imaam = await _context.Imaams
            .Include(i => i.Classes.Where(c => c.IsActive))
            .Include(i => i.Subjects.Where(s => s.IsActive))
            .FirstOrDefaultAsync(i => i.Id == id && i.IsActive);

        if (imaam == null) return NotFound();

        return Ok(new ImaamDetailResponse
        {
            Id = imaam.Id,
            FullName = imaam.FullName,
            PhoneNumber = imaam.PhoneNumber,
            Email = imaam.Email,
            Address = imaam.Address,
            Qualification = imaam.Qualification,
            JoiningDate = imaam.JoiningDate,
            MosqueId = imaam.MosqueId,
            Classes = imaam.Classes.Select(c => new ClassBriefResponse
            {
                Id = c.Id,
                Name = c.Name,
                Section = c.Section
            }).ToList(),
            Subjects = imaam.Subjects.Select(s => new SubjectBriefResponse
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateImaam(CreateImaamRequest request)
    {
        if (!await _context.Mosques.AnyAsync(m => m.Id == request.MosqueId))
            return BadRequest(new { message = "Mosque not found" });

        var imaam = new Entities.Imaam
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Address = request.Address,
            Qualification = request.Qualification,
            JoiningDate = request.JoiningDate ?? DateTime.UtcNow,
            MosqueId = request.MosqueId
        };

        _context.Imaams.Add(imaam);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Imaam created successfully", id = imaam.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateImaam(int id, CreateImaamRequest request)
    {
        var imaam = await _context.Imaams.FindAsync(id);
        if (imaam == null) return NotFound();

        imaam.FullName = request.FullName;
        imaam.PhoneNumber = request.PhoneNumber;
        imaam.Email = request.Email;
        imaam.Address = request.Address;
        imaam.Qualification = request.Qualification;
        imaam.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Imaam updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImaam(int id)
    {
        var imaam = await _context.Imaams.FindAsync(id);
        if (imaam == null) return NotFound();

        imaam.IsActive = false;
        imaam.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Imaam deleted successfully" });
    }
}