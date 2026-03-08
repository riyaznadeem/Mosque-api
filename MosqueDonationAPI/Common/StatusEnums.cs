namespace MosqueDonationAPI.Common;

public static class AttendanceStatus
{
    public const string Present = "Present";
    public const string Absent = "Absent";
    public const string Late = "Late";
    public const string OnLeave = "OnLeave";
    public const string HalfDay = "HalfDay";
    public const string Excused = "Excused";
}

public static class PaymentStatus
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string PartiallyPaid = "PartiallyPaid";
    public const string Overdue = "Overdue";
    public const string Waived = "Waived";
}
