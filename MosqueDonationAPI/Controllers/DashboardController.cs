using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MosqueDonationAPI.Models;
using MosqueDonationAPI.Services;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get complete dashboard data for a mosque
    /// </summary>
    [HttpGet("mosque/{mosqueId}")]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard(int mosqueId)
    {
        try
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync(mosqueId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard data for mosque {MosqueId}", mosqueId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get summary statistics only
    /// </summary>
    [HttpGet("mosque/{mosqueId}/summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(int mosqueId)
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync(mosqueId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching summary for mosque {MosqueId}", mosqueId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get monthly financial data for charts
    /// </summary>
    [HttpGet("mosque/{mosqueId}/financials")]
    public async Task<ActionResult<List<MonthlyFinancialDto>>> GetMonthlyFinancials(
        int mosqueId,
        [FromQuery] int year = 0)
    {
        try
        {
            if (year == 0) year = DateTime.UtcNow.Year;

            var financials = await _dashboardService.GetMonthlyFinancialsAsync(mosqueId, year);
            return Ok(financials);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching financials for mosque {MosqueId}", mosqueId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get class enrollment statistics
    /// </summary>
    [HttpGet("mosque/{mosqueId}/enrollments")]
    public async Task<ActionResult<List<ClassEnrollmentDto>>> GetClassEnrollments(int mosqueId)
    {
        try
        {
            var enrollments = await _dashboardService.GetClassEnrollmentsAsync(mosqueId);
            return Ok(enrollments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching enrollments for mosque {MosqueId}", mosqueId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get attendance trends for date range
    /// </summary>
    [HttpGet("mosque/{mosqueId}/attendance-trends")]
    public async Task<ActionResult<List<AttendanceTrendDto>>> GetAttendanceTrends(
        int mosqueId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var trends = await _dashboardService.GetAttendanceTrendAsync(mosqueId, start, end);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching attendance trends for mosque {MosqueId}", mosqueId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get recent activities
    /// </summary>
    [HttpGet("mosque/{mosqueId}/activities")]
    public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities(
        int mosqueId,
        [FromQuery] int count = 10)
    {
        try
        {
            var activities = await _dashboardService.GetRecentActivitiesAsync(mosqueId, count);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching activities for mosque {MosqueId}", mosqueId);
            return StatusCode(500, "Internal server error");
        }
    }
}
