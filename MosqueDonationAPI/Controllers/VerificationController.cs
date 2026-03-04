using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Data;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VerificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/verification/{receiptNumber}
    [HttpGet("{receiptNumber}")]
    public async Task<IActionResult> VerifyReceipt(string receiptNumber)
    {
        // Try to find by receipt number
        var donation = await _context.Donations
            .Include(d => d.Mosque)
            .Include(d => d.ReceivedBy)
            .FirstOrDefaultAsync(d => d.ReceiptNumber == receiptNumber);

        // If not found, try by ID format (RECPT-000001)
        if (donation == null && receiptNumber.StartsWith("RECPT-"))
        {
            if (int.TryParse(receiptNumber.Replace("RECPT-", ""), out int id))
            {
                donation = await _context.Donations
                    .Include(d => d.Mosque)
                    .Include(d => d.ReceivedBy)
                    .FirstOrDefaultAsync(d => d.Id == id);
            }
        }

        if (donation == null)
        {
            return Ok(new
            {
                isValid = false,
                message = "Receipt not found. This may be a fake or invalid receipt."
            });
        }

        // Check if donation was deleted (soft delete check if implemented)
        if (!donation.Mosque.IsActive)
        {
            return Ok(new
            {
                isValid = false,
                message = "This receipt has been marked as invalid."
            });
        }

        return Ok(new
        {
            isValid = true,
            message = "Receipt verified successfully!",
            receipt = new
            {
                receiptNumber = donation.ReceiptNumber,
                donorName = donation.DonorName,
                amount = donation.Amount,
                amountInWords = ConvertToWords(donation.Amount),
                purpose = donation.Purpose,
                donationDate = donation.DonationDate.ToString("dd-MM-yyyy"),
                mosqueName = donation.Mosque.Name,
                mosqueAddress = $"{donation.Mosque.Address}, {donation.Mosque.City}",
                receivedBy = donation.ReceivedBy.Username,
                paymentMode = donation.PaymentMode
            }
        });
    }

    // Public verification page (no auth required)
    [HttpGet("public/{receiptNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> PublicVerify(string receiptNumber)
    {
        var result = await VerifyReceipt(receiptNumber);
        return result;
    }

    private static string ConvertToWords(decimal amount)
    {
        var rupees = (int)amount;
        var paise = (int)((amount - rupees) * 100);
        var words = $"{NumberToWords(rupees)} Rupees";
        if (paise > 0)
            words += $" and {NumberToWords(paise)} Paise";
        return words + " Only";
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