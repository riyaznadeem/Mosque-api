using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Services;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPdfService _pdfService;

    public ReceiptsController(ApplicationDbContext context, IPdfService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    // GET: api/receipts/{donationId}/download
    [HttpGet("{donationId}/{IsLanguage}/download")]
    public async Task<IActionResult> DownloadReceipt(int donationId,bool IsLanguage)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var donation = await _context.Donations
            .Include(d => d.Mosque)
            .Include(d => d.ReceivedBy)
            .FirstOrDefaultAsync(d => d.Id == donationId);

        if (donation == null)
            return NotFound(new { message = "Donation not found" });

        // Permission check
        if (user.Role != "Admin")
        {
            if (donation.ReceivedByUserId != userId &&
                (!user.AssignedMosqueId.HasValue || donation.MosqueId != user.AssignedMosqueId))
                return Forbid();
        }

        // Prepare receipt data
        var receiptData = new DonationReceiptData
        {
            MosqueName = donation.Mosque.Name,
            MosqueNameUrdu = donation.Mosque.UrduName,
            MosqueAddress = $"{donation.Mosque.Address}, {donation.Mosque.City}, {donation.Mosque.State}",
            MosquePhone = donation.Mosque.Phone ?? "N/A",
            ReceiptNumber = donation.ReceiptNumber ?? $"RECPT-{donation.Id:D6}",
            Date = donation.DonationDate.ToString("dd-MM-yyyy"),
            DonorName = donation.DonorName,
            DonorNameUrdu = donation.DonorNameUrdu,
            AmountInWords = ConvertToWords(donation.Amount),
            Amount = donation.Amount,
            Purpose = donation.Purpose,
            PaymentMode = donation.PaymentMode,
            ReceivedBy = donation.ReceivedBy.Username
        };

        // Generate PDF
        var pdfBytes = _pdfService.GenerateReceiptPdf(receiptData, IsLanguage);

        // Return PDF file
        return File(pdfBytes, "application/pdf", $"Receipt-{receiptData.ReceiptNumber}.pdf");
    }

    // GET: api/receipts/{donationId}/preview
    [HttpGet("{donationId}/preview")]
    public async Task<IActionResult> PreviewReceipt(int donationId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var donation = await _context.Donations
            .Include(d => d.Mosque)
            .Include(d => d.ReceivedBy)
            .FirstOrDefaultAsync(d => d.Id == donationId);

        if (donation == null)
            return NotFound(new { message = "Donation not found" });

        // Permission check
        if (user.Role != "Admin")
        {
            if (donation.ReceivedByUserId != userId &&
                (!user.AssignedMosqueId.HasValue || donation.MosqueId != user.AssignedMosqueId))
                return Forbid();
        }

        var receiptData = new DonationReceiptData
        {
            MosqueName = donation.Mosque.Name,
            MosqueAddress = $"{donation.Mosque.Address}, {donation.Mosque.City}, {donation.Mosque.State}",
            MosquePhone = donation.Mosque.Phone ?? "N/A",
            ReceiptNumber = donation.ReceiptNumber ?? $"RECPT-{donation.Id:D6}",
            Date = donation.DonationDate.ToString("dd-MM-yyyy"),
            DonorName = donation.DonorName,
            AmountInWords = ConvertToWords(donation.Amount),
            Amount = donation.Amount,
            Purpose = donation.Purpose,
            PaymentMode = donation.PaymentMode,
            ReceivedBy = donation.ReceivedBy.Username
        };

        var pdfBytes = _pdfService.GenerateReceiptPdf(receiptData,true);

        // Return as base64 for preview
        return Ok(new
        {
            pdfBase64 = Convert.ToBase64String(pdfBytes),
            fileName = $"Receipt-{receiptData.ReceiptNumber}.pdf"
        });
    }

    private static string ConvertToWords(decimal amount)
    {
        // Simple implementation - you can use a proper number-to-words library
        var rupees = (int)amount;
        var paise = (int)((amount - rupees) * 100);

        string result = $"{NumberToWords(rupees)} Rupees";
        if (paise > 0)
            result += $" and {NumberToWords(paise)} Paise";
        result += " Only";

        return result;
    }

    private static string NumberToWords(int number)
    {
        if (number == 0) return "Zero";

        string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
            "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        if (number < 20) return ones[number];
        if (number < 100) return tens[number / 10] + (number % 10 > 0 ? " " + ones[number % 10] : "");
        if (number < 1000) return ones[number / 100] + " Hundred" + (number % 100 > 0 ? " and " + NumberToWords(number % 100) : "");
        if (number < 100000) return NumberToWords(number / 1000) + " Thousand" + (number % 1000 > 0 ? " " + NumberToWords(number % 1000) : "");
        if (number < 10000000) return NumberToWords(number / 100000) + " Lakh" + (number % 100000 > 0 ? " " + NumberToWords(number % 100000) : "");

        return NumberToWords(number / 10000000) + " Crore" + (number % 10000000 > 0 ? " " + NumberToWords(number % 10000000) : "");
    }
}
