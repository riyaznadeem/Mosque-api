using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Data;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Only Admin can view audit logs
public class AuditController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuditController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] int? userId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action.Contains(action));

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        var totalCount = await query.CountAsync();

        var logs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogResponse
            {
                Id = a.Id,
                UserId = a.UserId,
                Username = a.User != null ? a.User.Username : "System",
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return Ok(new
        {
            logs,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("actions")]
    public IActionResult GetAvailableActions()
    {
        var actions = new[]
        {
            "CREATE_DONATION",
            "UPDATE_DONATION",
            "DELETE_DONATION",
            "CREATE_USER",
            "UPDATE_USER",
            "ASSIGN_MOSQUE",
            "LOGIN",
            "LOGOUT"
        };

        return Ok(actions);
    }

    [HttpGet("entity-types")]
    public IActionResult GetEntityTypes()
    {
        var types = new[] { "Donation", "User", "Mosque", "AuditLog" };
        return Ok(types);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetAuditStats([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        var stats = new
        {
            TotalLogs = await query.CountAsync(),
            ByAction = await query
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToListAsync(),
            ByEntityType = await query
                .GroupBy(a => a.EntityType)
                .Select(g => new { EntityType = g.Key, Count = g.Count() })
                .ToListAsync(),
            RecentActivity = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .Select(a => new { a.Action, a.EntityType, a.Timestamp })
                .ToListAsync()
        };

        return Ok(stats);
    }
}

public class AuditLogResponse
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}