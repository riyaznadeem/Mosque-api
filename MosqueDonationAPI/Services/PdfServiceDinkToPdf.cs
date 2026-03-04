using DinkToPdf;
using DinkToPdf.Contracts;
using QRCoder;

namespace MosqueDonationAPI.Services;

public class PdfServiceDinkToPdf : IPdfService
{
    private readonly IConverter _converter;

    public PdfServiceDinkToPdf(IConverter converter)
    {
        _converter = converter;
    }

    public byte[] GenerateReceiptPdf(DonationReceiptData data , bool IsLanguage)
    {
        string qrBase64 = GenerateSimpleQRCode($"https://localhost:44351/Verification/api/{data.ReceiptNumber}");

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        @page {{
            size: A4;
            margin: 10mm;
        }}
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{ 
            font-family: 'Segoe UI', Arial, sans-serif; 
            background: white !important;  /* FORCE WHITE BACKGROUND */
            padding: 0;
            margin: 0;
        }}
        .receipt-container {{ 
            background: white !important;  /* FORCE WHITE */
            border: 2px solid #1e3a8a;
            padding: 30px;
            max-width: 190mm;
            margin: 0 auto;
        }}
        .header {{
            background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%) !important;
            color: white !important;
            padding: 25px;
            margin: -30px -30px 30px -30px;
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
        }}
        .mosque-name {{ 
            color: white !important;  /* WHITE TEXT */
            font-size: 24px; 
            font-weight: bold; 
            font-style: italic;
            margin-bottom: 8px;
        }}
        .mosque-address {{
            font-size: 13px;
            color: rgba(255,255,255,0.9) !important;  /* LIGHT WHITE */
            line-height: 1.5;
        }}
        .receipt-info {{
            text-align: right;
        }}
        .receipt-label {{
            color: rgba(255,255,255,0.8) !important;
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}
        .receipt-no {{ 
            color: #fbbf24 !important;  /* GOLD COLOR */
            font-size: 22px; 
            font-weight: bold;
            margin: 5px 0;
        }}
        .date {{
            font-size: 13px;
            color: white !important;
        }}
        .content {{
            margin-top: 20px;
        }}
        .form-row {{
            margin-bottom: 25px;
            background: white !important;  /* FORCE WHITE */
        }}
        .form-label {{
            font-size: 11px;
            color: #6b7280;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 8px;
            background: white !important;
        }}
        .form-line {{
            border-bottom: 2px solid #3b82f6;
            padding: 10px 5px;
            font-size: 16px;
            color: #1f2937 !important;  /* DARK GRAY TEXT */
            font-weight: 600;
            min-height: 40px;
            background: white !important;  /* NO BLACK BACKGROUND */
        }}
        .amount-section {{
            margin-top: 30px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            background: white !important;
        }}
        .amount-box {{
            display: flex;
            align-items: center;
            gap: 15px;
        }}
        .amount-label {{
            font-size: 12px;
            color: #4b5563;
        }}
        .amount-value {{
            border: 2px solid #1e3a8a;
            border-radius: 8px;
            padding: 15px 30px;
            font-size: 20px;
            font-weight: bold;
            color: #1e3a8a;
            background: #eff6ff !important;  /* LIGHT BLUE */
        }}
        .signature-section {{
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            margin-top: 40px;
            padding-top: 30px;
            border-top: 1px dashed #d1d5db;
            background: white !important;
        }}
        .signature {{
            text-align: center;
            background: white !important;
        }}
        .signature-line {{
            width: 200px;
            border-bottom: 2px solid #1f2937;
            margin-bottom: 10px;
            padding-bottom: 5px;
            font-style: italic;
            color: #374151 !important;
            font-size: 18px;
            background: white !important;
        }}
        .qr-section {{
            text-align: center;
            background: white !important;
        }}
        .qr-code {{
            width: 100px;
            height: 100px;
            border: 1px solid #e5e7eb;
            border-radius: 8px;
            padding: 5px;
            background: white !important;
        }}
        .footer {{
            margin-top: 30px;
            text-align: center;
            font-size: 10px;
            color: #9ca3af;
            background: white !important;
        }}
    </style>
</head>
<body>
    <div class='receipt-container'>
        <div class='header'>
            <div>
                <div class='mosque-name'>{data.MosqueName}</div>
                <div class='mosque-address'>{data.MosqueAddress}</div>
                <div class='mosque-address'>📞 {data.MosquePhone}</div>
            </div>
            <div class='receipt-info'>
                <div class='receipt-label'>Official Receipt</div>
                <div class='receipt-no'>#{data.ReceiptNumber}</div>
                <div class='date'>📅 {data.Date}</div>
            </div>
        </div>
        
        <div class='content'>
            <div class='form-row'>
                <div class='form-label'>Received with thanks from Mr./Miss/Mrs.</div>
                <div class='form-line'>{data.DonorName}</div>
            </div>
            
            <div class='form-row'>
                <div class='form-label'>A sum of Rupees</div>
                <div class='form-line' style='font-style: italic;'>{data.AmountInWords}</div>
            </div>
            
            <div class='form-row'>
                <div class='form-label'>For</div>
                <div class='form-line'>{data.Purpose}</div>
            </div>
            
            <div class='amount-section'>
                <div class='amount-box'>
                    <span class='amount-label'>Amount Received:</span>
                    <span class='amount-value'>₹ {data.Amount:N2}</span>
                </div>
                <div style='font-size: 13px; color: #4b5563;'>
                    💳 {data.PaymentMode ?? "Cash"}
                </div>
            </div>
        </div>
        
        <div class='signature-section'>
            <div class='signature'>
                <div class='signature-line'>{data.ReceivedBy}</div>
                <div style='font-size: 12px; color: #4b5563;'>Authorized Signature</div>
            </div>
            
            <div class='qr-section'>
                <div class='qr-code'>
                    <img src='data:image/png;base64,{qrBase64}' width='90' height='90' />
                </div>
                <div style='font-size: 10px; color: #6b7280; margin-top: 5px;'>Scan to Verify</div>
            </div>
        </div>
        
        <div class='footer'>
            <p>This is a computer generated receipt. No signature required.</p>
            <p>Transaction ID: TXN{data.ReceiptNumber.Replace("RECPT-", "")}</p>
        </div>
    </div>
</body>
</html>";

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings { Top = 0, Bottom = 0, Left = 0, Right = 0 }
        },
            Objects = {
            new ObjectSettings() {
                HtmlContent = html,
                WebSettings = {
                    DefaultEncoding = "utf-8",
                    PrintMediaType = true,  /* IMPORTANT: Use print styles */
                    EnableJavascript = false
                }
            }
        }
        };

        return _converter.Convert(doc);
    }

    // Simple QR Code Generator (using QRCode.js or similar)
    private string GenerateSimpleQRCode(string data)
    {
        // You need to install QRCoder package: dotnet add package QRCoder
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeImage);
        }
    }
}