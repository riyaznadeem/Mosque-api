using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MosquesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MosquesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var mosques = await _context.Mosques
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync();

        return Ok(mosques);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var mosque = await _context.Mosques.FindAsync(id);
        if (mosque == null || !mosque.IsActive)
            return NotFound();

        return Ok(mosque);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Mosque mosque)
    {
        mosque.CreatedAt = DateTime.UtcNow;
        mosque.IsActive = true;

        _context.Mosques.Add(mosque);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = mosque.Id }, mosque);
    }
}