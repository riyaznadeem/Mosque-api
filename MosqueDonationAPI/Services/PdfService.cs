using SelectPdf;
using QRCoder;

namespace MosqueDonationAPI.Services;

public interface IPdfService
{
    byte[] GenerateReceiptPdf(DonationReceiptData data, bool IsLanguage);
}

public class DonationReceiptData
{
    public string MosqueName { get; set; } = string.Empty;
    public string? MosqueNameUrdu { get; set; } = string.Empty;
    public string? MosqueAddress { get; set; }
    public string? MosquePhone { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    public string? DonorNameUrdu { get; set; } = string.Empty;
    public string AmountInWords { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? PaymentMode { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
}

public class PdfService : IPdfService
{
    public byte[] GenerateReceiptPdf(DonationReceiptData data, bool IsLanguage)
    {
        string qrBase64 = GenerateQRCode($"https://mosque-api-production.up.railway.app/api/Verification/{data.ReceiptNumber}");

        string html = IsLanguage switch
        {
            true => GenerateUrduTemplate(data, qrBase64),
            _ => GenerateEnglishTemplate(data, qrBase64)
        };

        // Configure SelectPdf
        HtmlToPdf converter = new HtmlToPdf();
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
        converter.Options.MarginTop = 0;
        converter.Options.MarginBottom = 0;
        converter.Options.MarginLeft = 0;
        converter.Options.MarginRight = 0;
        converter.Options.WebPageWidth = 1123;
        converter.Options.WebPageHeight = 794;
        converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;
        converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
        converter.Options.PdfStandard = PdfStandard.Full;
        converter.Options.ColorSpace = PdfColorSpace.RGB;

        PdfDocument doc = converter.ConvertHtmlString(html);
        byte[] pdf = doc.Save();
        doc.Close();

        return pdf;
    }
    private string GenerateEnglishTemplate(DonationReceiptData data, string qrBase64)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        @page {{ size: A4 landscape; margin: 0; }}
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Segoe UI', 'Arial', sans-serif;
            background: #ffffff !important;
            width: 297mm;
            height: 210mm;
            margin: 0;
            padding: 10mm;
        }}
        .receipt-wrapper {{
            width: 277mm;
            height: 190mm;
            background: #ffffff !important;
            border: 3px solid #1e3a8a;
            border-radius: 10px;
            overflow: hidden;
            position: relative;
        }}
        .receipt-header {{
            background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%);
            color: white;
            padding: 15px 25px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            height: 80px;
        }}
        .mosque-info h1 {{
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 5px;
            font-style: italic;
        }}
        .mosque-info p {{
            font-size: 12px;
            opacity: 0.9;
            line-height: 1.4;
        }}
        .receipt-badge {{
            text-align: right;
        }}
        .receipt-badge .label {{
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: 1px;
            opacity: 0.8;
        }}
        .receipt-badge .number {{
            font-size: 24px;
            font-weight: 800;
            color: #fbbf24;
        }}
        .receipt-badge .date {{
            font-size: 13px;
            margin-top: 3px;
        }}
        .receipt-body {{
            padding: 20px 25px;
            background: #ffffff !important;
            height: calc(190mm - 80px);
            position: relative;
        }}
        .form-row {{
            margin-bottom: 15px;
            background: #ffffff !important;
        }}
        .form-label {{
            font-size: 11px;
            color: #6b7280;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 5px;
            display: block;
        }}
        .form-input {{
            border-bottom: 2px solid #3b82f6;
            padding: 8px 0;
            font-size: 16px;
            color: #1f2937;
            font-weight: 600;
            min-height: 35px;
            background: #ffffff !important;
        }}
        .two-column {{
            display: flex;
            gap: 30px;
            margin-top: 15px;
        }}
        .column {{
            flex: 1;
        }}
        .amount-display {{
            background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%);
            border: 2px solid #10b981;
            border-radius: 8px;
            padding: 15px;
            text-align: center;
        }}
        .amount-display .currency {{
            font-size: 14px;
            color: #059669;
            font-weight: 600;
        }}
        .amount-display .value {{
            font-size: 28px;
            font-weight: 800;
            color: #047857;
            margin-top: 5px;
        }}
        .bottom-section {{
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            margin-top: 20px;
            padding-top: 15px;
            border-top: 1px dashed #d1d5db;
        }}
        .digital-signature {{
            text-align: center;
            padding: 10px;
            background: #f9fafb;
            border-radius: 8px;
            border: 2px dashed #d1d5db;
            width: 200px;
        }}
        .signature-line {{
            width: 180px;
            height: 40px;
            border-bottom: 2px solid #1f2937;
            margin-bottom: 5px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-family: 'Brush Script MT', cursive;
            font-size: 20px;
            color: #1f2937;
            transform: rotate(-3deg);
        }}
        .qr-section {{
            text-align: center;
        }}
        .qr-code {{
            width: 80px;
            height: 80px;
            background: #ffffff !important;
            border: 2px solid #e5e7eb;
            border-radius: 8px;
            padding: 5px;
        }}
        .qr-code img {{
            width: 70px;
            height: 70px;
        }}
        .footer-info {{
            margin-top: 15px;
            display: flex;
            justify-content: space-between;
            font-size: 10px;
            color: #6b7280;
        }}
        .stamp {{
            position: absolute;
            top: 10px;
            right: 20px;
            width: 70px;
            height: 70px;
            border: 3px solid #ef4444;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #ef4444;
            font-size: 12px;
            font-weight: 700;
            transform: rotate(-15deg);
            opacity: 0.6;
        }}
        .purpose-tag {{
            background: #dbeafe;
            color: #1e40af;
            padding: 4px 12px;
            border-radius: 15px;
            font-size: 14px;
            display: inline-block;
        }}
        .lang-badge {{
            position: absolute;
            top: 10px;
            left: 20px;
            background: rgba(255,255,255,0.2);
            color: white;
            padding: 2px 8px;
            border-radius: 4px;
            font-size: 10px;
        }}
    </style>
</head>
<body>
    <div class='receipt-wrapper'>
        <div class='receipt-header'>
            <div class='lang-badge'>ENGLISH</div>
            <div class='mosque-info'>
                <h1>{data.MosqueName}</h1>
                <p>{data.MosqueAddress}</p>
                <p>📞 {data.MosquePhone}</p>
            </div>
            <div class='receipt-badge'>
                <div class='label'>Official Receipt</div>
                <div class='number'>#{data.ReceiptNumber}</div>
                <div class='date'>📅 {data.Date}</div>
            </div>
        </div>
        
        <div class='receipt-body'>
            <div class='stamp'>PAID</div>
            
            <div class='form-row'>
                <label class='form-label'>Received with thanks from</label>
                <div class='form-input' style='font-size: 18px;'>{data.DonorName}</div>
            </div>
            
            <div class='two-column'>
                <div class='column'>
                    <div class='form-row'>
                        <label class='form-label'>Amount in words</label>
                        <div class='form-input' style='font-style: italic;'>{data.AmountInWords}</div>
                    </div>
                </div>
                <div class='column'>
                    <div class='form-row'>
                        <label class='form-label'>Purpose of Donation</label>
                        <div class='form-input'>
                            <span class='purpose-tag'>{data.Purpose}</span>
                        </div>
                    </div>
                </div>
            </div>
            
            <div class='two-column'>
                <div class='column'>
                    <label class='form-label'>Amount Received</label>
                    <div class='amount-display'>
                        <div class='currency'>INR</div>
                        <div class='value'>₹{data.Amount:N2}</div>
                    </div>
                </div>
                <div class='column'>
                    <div class='form-row'>
                        <label class='form-label'>Payment Mode</label>
                        <div class='form-input'>💳 {data.PaymentMode ?? "Cash"}</div>
                    </div>
                </div>
            </div>
            
            <div class='bottom-section'>
                <div class='digital-signature'>
                    <div class='signature-line'>{data.ReceivedBy}</div>
                    <div style='font-size: 11px; color: #4b5563;'>Authorized Signature</div>
                </div>
                
                <div class='qr-section'>
                    <div class='qr-code'>
                        <img src='data:image/png;base64,{qrBase64}' alt='QR' />
                    </div>
                    <div style='font-size: 9px; color: #6b7280; margin-top: 3px;'>Scan to Verify</div>
                </div>
            </div>
            
            <div class='footer-info'>
                <div>
                    <strong>Transaction ID:</strong> TXN{data.ReceiptNumber.Replace("RECPT-", "")}{DateTime.Now:yyyyMMdd}
                </div>
                <div>
                    This is a computer generated receipt and does not require physical signature.
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateUrduTemplate(DonationReceiptData data, string qrBase64)
    {
        // Urdu translations
        string receivedFrom = "وصول کنندہ کا نام";
        string amountInWords = "رقم الفاظ میں";
        string purpose = "عطیہ کا مقصد";
        string amountReceived = "وصول شدہ رقم";
        string paymentMode = "ادائیگی کا طریقہ";
        string authorizedSign = "مجاز دستخط";
        string scanToVerify = "اسکین کریں";
        string officialReceipt = "دفتری رسید";
        string transactionId = "ٹرانزیکشن آئی ڈی";
        string computerGenerated = "یہ کمپیوٹر سے تیار کردہ رسید ہے، دستخط کی ضرورت نہیں ہے";
        string paid = "ادا شدہ";

        // Translate payment modes
        string paymentModeUrdu = data.PaymentMode?.ToLower() switch
        {
            "cash" => "نقد",
            "upi" => "یو پی آئی",
            "bank transfer" => "بینک ٹرانسفر",
            "cheque" => "چیک",
            _ => data.PaymentMode ?? "نقد"
        };

        return $@"
<!DOCTYPE html>
<html dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Noto+Nastaliq+Urdu:wght@400;700&display=swap');
        
        @page {{ size: A4 landscape; margin: 0; }}
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Noto Nastaliq Urdu', 'Segoe UI', 'Arial', sans-serif;
            background: #ffffff !important;
            width: 297mm;
            height: 210mm;
            margin: 0;
            padding: 10mm;
            direction: rtl;
        }}
        .receipt-wrapper {{
            width: 277mm;
            height: 190mm;
            background: #ffffff !important;
            border: 3px solid #1e3a8a;
            border-radius: 10px;
            overflow: hidden;
            position: relative;
        }}
        .receipt-header {{
            background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%);
            color: white;
            padding: 15px 25px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            height: 80px;
            flex-direction: row-reverse;
        }}
        .mosque-info {{
            text-align: right;
        }}
        .mosque-info h1 {{
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 5px;
            font-family: 'Noto Nastaliq Urdu', serif;
        }}
        .mosque-info p {{
            font-size: 13px;
            opacity: 0.9;
            line-height: 1.6;
            font-family: 'Noto Nastaliq Urdu', serif;
        }}
        .receipt-badge {{
            text-align: left;
        }}
        .receipt-badge .label {{
            font-size: 12px;
            text-transform: uppercase;
            letter-spacing: 1px;
            opacity: 0.8;
            font-family: 'Segoe UI', sans-serif;
        }}
        .receipt-badge .number {{
            font-size: 24px;
            font-weight: 800;
            color: #fbbf24;
            font-family: 'Segoe UI', sans-serif;
        }}
        .receipt-badge .date {{
            font-size: 13px;
            margin-top: 3px;
            font-family: 'Segoe UI', sans-serif;
        }}
        .receipt-body {{
            padding: 20px 25px;
            background: #ffffff !important;
            height: calc(190mm - 80px);
            position: relative;
            direction: rtl;
        }}
        .form-row {{
            margin-bottom: 15px;
            background: #ffffff !important;
            text-align: right;
        }}
        .form-label {{
            font-size: 12px;
            color: #6b7280;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 5px;
            display: block;
            text-align: right;
            font-family: 'Noto Nastaliq Urdu', serif;
        }}
        .form-input {{
            border-bottom: 2px solid #3b82f6;
            padding: 8px 0;
            font-size: 16px;
            color: #1f2937;
            font-weight: 600;
            min-height: 35px;
            background: #ffffff !important;
            text-align: right;
            font-family: 'Noto Nastaliq Urdu', serif;
        }}
        .two-column {{
            display: flex;
            gap: 30px;
            margin-top: 15px;
            flex-direction: row-reverse;
        }}
        .column {{
            flex: 1;
        }}
        .amount-display {{
            background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%);
            border: 2px solid #10b981;
            border-radius: 8px;
            padding: 15px;
            text-align: center;
        }}
        .amount-display .currency {{
            font-size: 14px;
            color: #059669;
            font-weight: 600;
            font-family: 'Segoe UI', sans-serif;
        }}
        .amount-display .value {{
            font-size: 28px;
            font-weight: 800;
            color: #047857;
            margin-top: 5px;
            font-family: 'Segoe UI', sans-serif;
        }}
        .bottom-section {{
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            margin-top: 20px;
            padding-top: 15px;
            border-top: 1px dashed #d1d5db;
            flex-direction: row-reverse;
        }}
        .digital-signature {{
            text-align: center;
            padding: 10px;
            background: #f9fafb;
            border-radius: 8px;
            border: 2px dashed #d1d5db;
            width: 200px;
        }}
        .signature-line {{
            width: 180px;
            height: 40px;
            border-bottom: 2px solid #1f2937;
            margin-bottom: 5px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-family: 'Noto Nastaliq Urdu', serif;
            font-size: 18px;
            color: #1f2937;
        }}
        .qr-section {{
            text-align: center;
        }}
        .qr-code {{
            width: 80px;
            height: 80px;
            background: #ffffff !important;
            border: 2px solid #e5e7eb;
            border-radius: 8px;
            padding: 5px;
        }}
        .qr-code img {{
            width: 70px;
            height: 70px;
        }}
        .footer-info {{
            margin-top: 15px;
            display: flex;
            justify-content: space-between;
            font-size: 10px;
            color: #6b7280;
            flex-direction: row-reverse;
            font-family: 'Noto Nastaliq Urdu', serif;
        }}
        .stamp {{
            position: absolute;
            top: 10px;
            left: 20px;
            width: 70px;
            height: 70px;
            border: 3px solid #ef4444;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #ef4444;
            font-size: 14px;
            font-weight: 700;
            transform: rotate(15deg);
            opacity: 0.6;
            font-family: 'Noto Nastaliq Urdu', serif;
        }}
        .purpose-tag {{
            background: #dbeafe;
            color: #1e40af;
            padding: 4px 12px;
            border-radius: 15px;
            font-size: 14px;
            display: inline-block;
            font-family: 'Noto Nastaliq Urdu', serif;
        }}
        .lang-badge {{
            position: absolute;
            top: 10px;
            right: 20px;
            background: rgba(255,255,255,0.2);
            color: white;
            padding: 2px 8px;
            border-radius: 4px;
            font-size: 10px;
            font-family: 'Noto Nastaliq Urdu', serif;
        }}
    </style>
</head>
<body>
    <div class='receipt-wrapper'>
        <div class='receipt-header'>
            <div class='lang-badge'>اردو</div>
            <div class='receipt-badge'>
                <div class='label'>{officialReceipt}</div>
                <div class='number'>#{data.ReceiptNumber}</div>
                <div class='date'>📅 {data.Date}</div>
            </div>
            <div class='mosque-info'>
                <h1>{data.MosqueNameUrdu ?? data.MosqueName}</h1>
                <p>{data.MosqueAddress}</p>
                <p>📞 {data.MosquePhone}</p>
            </div>
        </div>
        
        <div class='receipt-body'>
            <div class='stamp'>{paid}</div>
            
            <div class='form-row'>
                <label class='form-label'>{receivedFrom}</label>
                <div class='form-input' style='font-size: 18px;'>{data.DonorNameUrdu ?? data.DonorName}</div>
            </div>
            
            <div class='two-column'>
                <div class='column'>
                    <div class='form-row'>
                        <label class='form-label'>{purpose}</label>
                        <div class='form-input'>
                            <span class='purpose-tag'>{data.Purpose}</span>
                        </div>
                    </div>
                </div>
                <div class='column'>
                    <div class='form-row'>
                        <label class='form-label'>{amountInWords}</label>
                        <div class='form-input' style='font-style: italic;'>{data.AmountInWords}</div>
                    </div>
                </div>
            </div>
            
            <div class='two-column'>
                <div class='column'>
                    <div class='form-row'>
                        <label class='form-label'>{paymentMode}</label>
                        <div class='form-input'>💳 {paymentModeUrdu}</div>
                    </div>
                </div>
                <div class='column'>
                    <label class='form-label'>{amountReceived}</label>
                    <div class='amount-display'>
                        <div class='currency'>روپے</div>
                        <div class='value'>₹{data.Amount:N2}</div>
                    </div>
                </div>
            </div>
            
            <div class='bottom-section'>
                <div class='qr-section'>
                    <div class='qr-code'>
                        <img src='data:image/png;base64,{qrBase64}' alt='QR' />
                    </div>
                    <div style='font-size: 9px; color: #6b7280; margin-top: 3px;'>{scanToVerify}</div>
                </div>
                
                <div class='digital-signature'>
                    <div class='signature-line'>{data.ReceivedBy}</div>
                    <div style='font-size: 11px; color: #4b5563;'>{authorizedSign}</div>
                </div>
            </div>
            
            <div class='footer-info'>
                <div>
                    {computerGenerated}
                </div>
                <div>
                    <strong>{transactionId}:</strong> TXN{data.ReceiptNumber.Replace("RECPT-", "")}{DateTime.Now:yyyyMMdd}
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateQRCode(string text)
    {
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeImage);
        }
    }
}