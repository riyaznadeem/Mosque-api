using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;
using System.Security.Claims;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DonationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DonationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    #region CREATE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDonationRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var user = await _context.Users
            .Include(u => u.AssignedMosque)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        // Determine mosqueId
        int mosqueId;

        if (user.Role == "Admin")
        {
            // Admin can specify any mosque
            if (!request.MosqueId.HasValue)
            {
                return BadRequest(new { message = "MosqueId required for admin" });
            }
            mosqueId = request.MosqueId.Value;
        }
        else
        {
            // Regular user must have assigned mosque
            if (!user.AssignedMosqueId.HasValue)
            {
                return BadRequest(new { message = "No mosque assigned to user" });
            }
            mosqueId = user.AssignedMosqueId.Value;
        }

        // Verify mosque exists
        var mosque = await _context.Mosques.FindAsync(mosqueId);
        if (mosque == null || !mosque.IsActive)
        {
            return BadRequest(new { message = "Invalid mosque selected" });
        }

        var donation = new Donation
        {
            MosqueId = mosqueId,
            ReceivedByUserId = userId,
            DonorName = request.DonorName,
            DonorPhone = request.DonorPhone,
            Amount = request.Amount,
            Purpose = request.Purpose,
            Description = request.Description,
            PaymentMode = request.PaymentMode ?? "Cash",
            DonationDate = DateTime.UtcNow,
            IsPrinted = false,
            ReceiptNumber = null
        };

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        donation.ReceiptNumber = $"{user.AssignedMosque.ShortName}-{donation.Id:D6}";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Donation recorded successfully",
            donationId = donation.Id,
            receiptNumber = donation.ReceiptNumber,
            amount = donation.Amount,
            mosqueName = mosque.Name
        });
    }
    #endregion

    #region Get All
    [HttpGet]
    public async Task<IActionResult> GetAll(
     [FromQuery] int? mosqueId,
     [FromQuery] DateTime? fromDate,
     [FromQuery] DateTime? toDate,
     [FromQuery] string? purpose,
     [FromQuery] int pageNumber = 1,      // Default page 1
     [FromQuery] int pageSize = 10)       // Default 10 items per page
    {
        // Validate pagination parameters
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;  // Max limit

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var query = _context.Donations
            .Include(d => d.Mosque)
            .Include(d => d.ReceivedBy)
            .AsQueryable();

        // Non-admin users can only see their assigned mosque donations
        if (user.Role != "Admin")
        {
            if (!user.AssignedMosqueId.HasValue)
            {
                return Ok(new
                {
                    donations = new List<DonationResponse>(),
                    totalAmount = 0,
                    count = 0,
                    pageNumber,
                    pageSize,
                    totalPages = 0,
                    hasPreviousPage = false,
                    hasNextPage = false
                });
            }
            query = query.Where(d => d.MosqueId == user.AssignedMosqueId);
        }
        else if (mosqueId.HasValue)
        {
            // Admin can filter by mosque
            query = query.Where(d => d.MosqueId == mosqueId);
        }

        if (fromDate.HasValue)
            query = query.Where(d => d.DonationDate >= fromDate);

        if (toDate.HasValue)
            query = query.Where(d => d.DonationDate <= toDate);

        if (!string.IsNullOrEmpty(purpose))
            query = query.Where(d => d.Purpose.Contains(purpose));

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var donations = await query
            .OrderByDescending(d => d.DonationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DonationResponse
            {
                Id = d.Id,
                ReceiptNumber = d.ReceiptNumber ?? $"RECPT-{d.Id:D6}",
                MosqueName = d.Mosque.Name,
                DonorName = d.DonorName,
                DonorPhone = d.DonorPhone,
                Amount = d.Amount,
                Purpose = d.Purpose,
                Description = d.Description,
                DonationDate = d.DonationDate,
                ReceivedBy = d.ReceivedBy.Username,
                PaymentMode = d.PaymentMode,
                IsPrinted = d.IsPrinted
            })
            .ToListAsync();

        // Calculate totals for current page
        var totalAmount = donations.Sum(d => d.Amount);

        // Calculate total amount of all records (optional - for dashboard)
        var grandTotal = await query.SumAsync(d => d.Amount);

        // Pagination metadata
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasPreviousPage = pageNumber > 1;
        var hasNextPage = pageNumber < totalPages;

        return Ok(new
        {
            donations,
            totalAmount,           // Current page total
            grandTotal,            // All records total
            count = donations.Count,  // Current page count
            totalCount,            // All records count
            pageNumber,
            pageSize,
            totalPages,
            hasPreviousPage,
            hasNextPage
        });
    }

    #endregion

    #region by-purpose
    [HttpGet("by-purpose")]
    public async Task<IActionResult> GetByPurpose(
    [FromQuery] int? mosqueId,
    [FromQuery] int pageNumber = 1,      // Default page 1
    [FromQuery] int pageSize = 10)       // Default 10 items per page
    {
        // Validate pagination parameters
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;  // Max limit

        var query = _context.Donations.AsQueryable();

        if (mosqueId.HasValue)
            query = query.Where(d => d.MosqueId == mosqueId);

        // Get grouped data
        var groupedQuery = query
            .GroupBy(d => d.Purpose)
            .Select(g => new
            {
                Purpose = g.Key,
                TotalAmount = g.Sum(d => d.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount);

        // Get total count before pagination
        var totalCount = await groupedQuery.CountAsync();

        // Apply pagination
        var summary = await groupedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Pagination metadata
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasPreviousPage = pageNumber > 1;
        var hasNextPage = pageNumber < totalPages;

        // Calculate grand totals
        var grandTotalAmount = await query.SumAsync(d => d.Amount);
        var grandTotalCount = await query.CountAsync();

        return Ok(new
        {
            data = summary,
            summary = new
            {
                grandTotalAmount,
                grandTotalCount,
                currentPageTotal = summary.Sum(x => x.TotalAmount),
                currentPageCount = summary.Sum(x => x.Count)
            },
            pagination = new
            {
                pageNumber,
                pageSize,
                totalCount,
                totalPages,
                hasPreviousPage,
                hasNextPage
            }
        });
    }
    #endregion

    #region print-receipt
    // POST: Receipt Print
    [HttpPost("print-receipt")]
    public async Task<IActionResult> PrintReceipt([FromBody] ReceiptPrintRequest request)
    {
        var donation = await _context.Donations
            .Include(d => d.Mosque)
            .Include(d => d.ReceivedBy)
            .FirstOrDefaultAsync(d => d.Id == request.DonationId);

        if (donation == null)
            return NotFound(new { message = "Donation not found" });

        // Mark as printed
        donation.IsPrinted = true;
        donation.PrintedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Return receipt data for printing
        var receipt = new
        {
            ReceiptNumber = donation.ReceiptNumber,
            Date = donation.DonationDate.ToString("dd-MM-yyyy"),
            MosqueName = donation.Mosque.Name,
            MosqueAddress = donation.Mosque.Address,
            DonorName = donation.DonorName,
            Amount = donation.Amount,
            AmountInWords = ConvertToWords(donation.Amount),
            Purpose = donation.Purpose,
            PaymentMode = donation.PaymentMode,
            ReceivedBy = donation.ReceivedBy.Username,
            PrintDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm")
        };

        return Ok(receipt);
    }

    // GET: Single donation for receipt reprint
    [HttpGet("{id}/receipt")]
    public async Task<IActionResult> GetReceipt(int id)
    {
        var donation = await _context.Donations
            .Include(d => d.Mosque)
            .Include(d => d.ReceivedBy)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
            return NotFound();

        var receipt = new
        {
            ReceiptNumber = donation.ReceiptNumber ?? $"RECPT-{donation.Id:D6}",
            Date = donation.DonationDate.ToString("dd-MM-yyyy"),
            MosqueName = donation.Mosque.Name,
            MosqueAddress = donation.Mosque.Address,
            DonorName = donation.DonorName,
            Amount = donation.Amount,
            AmountInWords = ConvertToWords(donation.Amount),
            Purpose = donation.Purpose,
            Description = donation.Description,
            PaymentMode = donation.PaymentMode,
            ReceivedBy = donation.ReceivedBy.Username,
            IsPrinted = donation.IsPrinted
        };

        return Ok(receipt);
    }
    #endregion

    #region Helper
    // Helper: Convert number to words for receipt
    private static string ConvertToWords(decimal amount)
    {
        // Simple implementation - you can enhance this
        var rupees = (int)amount;
        var paise = (int)((amount - rupees) * 100);

        var words = $"{rupees} Rupees";
        if (paise > 0)
            words += $" and {paise} Paise";

        return words + " Only";
    }
    #endregion

    #region Delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only admin can delete
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var donation = await _context.Donations.FindAsync(id);
        if (donation == null)
        {
            return NotFound(new { message = "Donation not found" });
        }

        // Create audit log before deleting
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "DELETE_DONATION",
            EntityType = "Donation",
            EntityId = donation.Id,
            OldValues = System.Text.Json.JsonSerializer.Serialize(donation),
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        _context.Donations.Remove(donation);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Donation deleted successfully" });
    }
    #endregion

    #region Update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDonationRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        // Find donation
        var donation = await _context.Donations
            .Include(d => d.Mosque)
            .Include(d => d.ReceivedBy)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
        {
            return NotFound(new { message = "Donation not found" });
        }

        // Check permissions
        if (user.Role != "Admin")
        {
            // Non-admin can only update their own donations and only if mosque matches
            if (donation.ReceivedByUserId != userId)
            {
                return Forbid(); // Can only update own donations
            }

            if (user.AssignedMosqueId.HasValue && donation.MosqueId != user.AssignedMosqueId)
            {
                return Forbid(); // Can only update donations from assigned mosque
            }

            // Non-admin cannot update amount (optional security rule)
            if (request.Amount.HasValue && request.Amount != donation.Amount)
            {
                return BadRequest(new { message = "Cannot change donation amount" });
            }
        }

        // Store old values for audit
        var oldValues = new
        {
            donation.DonorName,
            donation.DonorPhone,
            donation.Amount,
            donation.Purpose,
            donation.Description,
            donation.PaymentMode,
            donation.IsPrinted
        };

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.DonorName))
            donation.DonorName = request.DonorName;

        if (request.DonorPhone != null)
            donation.DonorPhone = request.DonorPhone;

        if (request.Amount.HasValue && user.Role == "Admin") // Only admin can change amount
            donation.Amount = request.Amount.Value;

        if (!string.IsNullOrEmpty(request.Purpose))
            donation.Purpose = request.Purpose;

        if (request.Description != null)
            donation.Description = request.Description;

        if (!string.IsNullOrEmpty(request.PaymentMode))
            donation.PaymentMode = request.PaymentMode;

        if (request.IsPrinted.HasValue)
        {
            donation.IsPrinted = request.IsPrinted.Value;
            if (request.IsPrinted.Value)
                donation.PrintedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Create audit log
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "UPDATE_DONATION",
            EntityType = "Donation",
            EntityId = donation.Id,
            OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValues = System.Text.Json.JsonSerializer.Serialize(request),
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Donation updated successfully",
            donationId = donation.Id,
            receiptNumber = donation.ReceiptNumber,
            updatedFields = request
        });
    }
    #endregion

    #region GetById
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var donation = await _context.Donations
            .Include(d => d.Mosque)
            .Include(d => d.ReceivedBy)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
            return NotFound(new { message = "Donation not found" });

        // Check permissions
        if (user.Role != "Admin")
        {
            if (donation.ReceivedByUserId != userId && donation.MosqueId != user.AssignedMosqueId)
                return Forbid();
        }

        return Ok(new DonationResponse
        {
            Id = donation.Id,
            ReceiptNumber = donation.ReceiptNumber ?? $"RECPT-{donation.Id:D6}",
            MosqueName = donation.Mosque.Name,
            DonorName = donation.DonorName,
            DonorPhone = donation.DonorPhone,
            Amount = donation.Amount,
            Purpose = donation.Purpose,
            Description = donation.Description,
            DonationDate = donation.DonationDate,
            ReceivedBy = donation.ReceivedBy.Username,
            PaymentMode = donation.PaymentMode,
            IsPrinted = donation.IsPrinted
        });
    }
    #endregion
}