using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using System.Security.Claims;

namespace MosqueDonationAPI.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // Store original body for potential logging
        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);
        }
        finally
        {
            // Log specific actions based on path and method
            if (ShouldLog(context))
            {
                await LogAction(context, dbContext);
            }
        }
    }

    private bool ShouldLog(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method;

        return (path.Contains("/donations") || path.Contains("/auth"))
               && (method == "POST" || method == "PUT" || method == "DELETE");
    }

    private async Task LogAction(HttpContext context, ApplicationDbContext dbContext)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var action = GetActionType(context);

        var auditLog = new AuditLog
        {
            UserId = userId != null ? int.Parse(userId) : null,
            Action = action,
            EntityType = GetEntityType(context),
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            Timestamp = DateTime.UtcNow
        };

        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync();
    }

    private string GetActionType(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method;

        return method switch
        {
            "POST" when path.Contains("login") => "LOGIN",
            "POST" => "CREATE",
            "PUT" => "UPDATE",
            "DELETE" => "DELETE",
            _ => "UNKNOWN"
        };
    }

    private string GetEntityType(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("donation")) return "Donation";
        if (path.Contains("mosque")) return "Mosque";
        if (path.Contains("user")) return "User";
        return "Unknown";
    }
}
