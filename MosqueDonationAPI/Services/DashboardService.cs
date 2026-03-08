using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Models;

namespace MosqueDonationAPI.Services;

public interface IDashboardService
{
    Task<DashboardResponseDto> GetDashboardDataAsync(int mosqueId);
    Task<DashboardSummaryDto> GetSummaryAsync(int mosqueId);
    Task<List<MonthlyFinancialDto>> GetMonthlyFinancialsAsync(int mosqueId, int year);
    Task<List<ClassEnrollmentDto>> GetClassEnrollmentsAsync(int mosqueId);
    Task<List<AttendanceTrendDto>> GetAttendanceTrendAsync(int mosqueId, DateTime startDate, DateTime endDate);
    Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int mosqueId, int count = 10);
}
public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponseDto> GetDashboardDataAsync(int mosqueId)
    {
        var currentDate = DateTime.UtcNow;
        var currentYear = currentDate.Year;
        var currentMonth = currentDate.Month;
        var today = currentDate.Date;

        var dashboard = new DashboardResponseDto
        {
            Summary = await GetSummaryAsync(mosqueId),
            MonthlyFinancials = await GetMonthlyFinancialsAsync(mosqueId, currentYear),
            ClassEnrollments = await GetClassEnrollmentsAsync(mosqueId),
            TopPerformers = await GetTopPerformersAsync(mosqueId),
            RecentActivities = await GetRecentActivitiesAsync(mosqueId, 10),
            FeeCollectionByClass = await GetFeeCollectionByClassAsync(mosqueId, currentYear, currentMonth),
            AttendanceByStatus = await GetTodayAttendanceStatusAsync(mosqueId, today)
        };

        return dashboard;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(int mosqueId)
    {
        var currentDate = DateTime.UtcNow;
        var today = currentDate.Date;
        var currentYear = currentDate.Year;
        var currentMonth = currentDate.Month;

        var totalImaams = await _context.Imaams.CountAsync(i => i.MosqueId == mosqueId);
        var totalChildren = await _context.Children.CountAsync(c => c.MosqueId == mosqueId);
        var totalClasses = await _context.Classes.CountAsync(c => c.MosqueId == mosqueId);
        var totalSubjects = await _context.Subjects.CountAsync(s => s.MosqueId == mosqueId);

        // Fix: Materialize first, then sum in memory
        var monthlySalaries = await _context.ImaamSalaries
            .Where(s => s.MosqueId == mosqueId && s.Year == currentYear && s.Month == currentMonth)
            .Select(s => s.NetSalary)  // Select only the value needed
            .ToListAsync();

        var monthlySalaryExpense = monthlySalaries.Sum();

        // Fix: Materialize first, then sum in memory
        var monthlyFees = await _context.ChildFees
            .Where(f => f.MosqueId == mosqueId && f.Year == currentYear && f.Month == currentMonth)
            .Select(f => new { f.AmountPaid, f.Balance })  // Select only needed fields
            .ToListAsync();

        var monthlyFeeCollection = monthlyFees.Sum(f => f.AmountPaid);
        var pendingFees = monthlyFees.Sum(f => f.Balance);

        var todayPresentImaams = await _context.ImaamAttendances
            .CountAsync(a => a.MosqueId == mosqueId && a.Date.Date == today && a.Status == "Present");

        var todayPresentChildren = await _context.ChildAttendances
            .CountAsync(a => a.MosqueId == mosqueId && a.Date.Date == today && a.Status == "Present");

        return new DashboardSummaryDto
        {
            TotalImaams = totalImaams,
            TotalChildren = totalChildren,
            TotalClasses = totalClasses,
            TotalSubjects = totalSubjects,
            MonthlySalaryExpense = monthlySalaryExpense,
            MonthlyFeeCollection = monthlyFeeCollection,
            PendingFees = pendingFees,
            TodayPresentImaams = todayPresentImaams,
            TodayPresentChildren = todayPresentChildren
        };
    }

    public async Task<List<MonthlyFinancialDto>> GetMonthlyFinancialsAsync(int mosqueId, int year)
    {
        var months = Enumerable.Range(1, 12);
        var financials = new List<MonthlyFinancialDto>();

        foreach (var month in months)
        {
            // Fix: Materialize data first, then perform aggregation in memory
            var salaryData = await _context.ImaamSalaries
                .Where(s => s.MosqueId == mosqueId && s.Year == year && s.Month == month)
                .Select(s => s.NetSalary)
                .ToListAsync();

            var salaryExpense = salaryData.Sum();

            var feeData = await _context.ChildFees
                .Where(f => f.MosqueId == mosqueId && f.Year == year && f.Month == month)
                .Select(f => f.AmountPaid)
                .ToListAsync();

            var feeIncome = feeData.Sum();

            financials.Add(new MonthlyFinancialDto
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMM"),
                TotalIncome = feeIncome,
                TotalExpense = salaryExpense,
                NetAmount = feeIncome - salaryExpense
            });
        }

        return financials;
    }

    // Alternative optimized version that reduces database round trips:
    public async Task<List<MonthlyFinancialDto>> GetMonthlyFinancialsAsyncOptimized(int mosqueId, int year)
    {
        // Fetch all data for the year in single queries
        var allSalaries = await _context.ImaamSalaries
            .Where(s => s.MosqueId == mosqueId && s.Year == year)
            .Select(s => new { s.Month, s.NetSalary })
            .ToListAsync();

        var allFees = await _context.ChildFees
            .Where(f => f.MosqueId == mosqueId && f.Year == year)
            .Select(f => new { f.Month, f.AmountPaid })
            .ToListAsync();

        var months = Enumerable.Range(1, 12);
        var financials = months.Select(month =>
        {
            var salaryExpense = allSalaries
                .Where(s => s.Month == month)
                .Sum(s => s.NetSalary);

            var feeIncome = allFees
                .Where(f => f.Month == month)
                .Sum(f => f.AmountPaid);

            return new MonthlyFinancialDto
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMM"),
                TotalIncome = feeIncome,
                TotalExpense = salaryExpense,
                NetAmount = feeIncome - salaryExpense
            };
        }).ToList();

        return financials;
    }
   

    public async Task<List<ClassEnrollmentDto>> GetClassEnrollmentsAsync(int mosqueId)
    {
        var classes = await _context.Classes
            .Where(c => c.MosqueId == mosqueId)
            .Include(c => c.ClassChildren)
            .Select(c => new ClassEnrollmentDto
            {
                ClassId = c.Id,
                ClassName = c.Name + (!string.IsNullOrEmpty(c.Section) ? $" ({c.Section})" : ""),
                EnrolledStudents = c.ClassChildren.Count(cc => cc.Status == "Active"),
                MaxCapacity = c.MaxCapacity
            })
            .ToListAsync();

        return classes;
    }

    public async Task<List<AttendanceTrendDto>> GetAttendanceTrendAsync(int mosqueId, DateTime startDate, DateTime endDate)
    {
        var trends = await _context.ChildAttendances
            .Where(a => a.MosqueId == mosqueId && a.Date >= startDate && a.Date <= endDate)
            .GroupBy(a => a.Date.Date)
            .Select(g => new AttendanceTrendDto
            {
                Date = g.Key,
                PresentCount = g.Count(x => x.Status == "Present"),
                AbsentCount = g.Count(x => x.Status == "Absent"),
                LateCount = g.Count(x => x.Status == "Late")
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return trends;
    }

    public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int mosqueId, int count = 10)
    {
        var activities = new List<RecentActivityDto>();

        // Recent Fee Payments
        var feePayments = await _context.FeePayments
            .Include(fp => fp.ChildFee)
            .ThenInclude(cf => cf.Child)
            .Include(fp => fp.ReceivedBy)
            .Where(fp => fp.ChildFee.MosqueId == mosqueId)
            .OrderByDescending(fp => fp.PaymentDate)
            .Take(count)
            .Select(fp => new RecentActivityDto
            {
                ActivityType = "FeePayment",
                Description = $"Fee payment received from {fp.ChildFee.Child.FullName}",
                Amount = fp.Amount,
                Timestamp = fp.PaymentDate,
                PerformedBy = fp.ReceivedBy != null ? fp.ReceivedBy.Username : "System"
            })
            .ToListAsync();

        // Recent Salary Payments
        var salaryPayments = await _context.ImaamSalaries
            .Include(s => s.Imaam)
            .Include(s => s.ProcessedBy)
            .Where(s => s.MosqueId == mosqueId && s.PaymentStatus == "Paid" && s.PaymentDate != null)
            .OrderByDescending(s => s.PaymentDate)
            .Take(count)
            .Select(s => new RecentActivityDto
            {
                ActivityType = "SalaryPayment",
                Description = $"Salary paid to {s.Imaam.FullName} for {new DateTime(s.Year, s.Month, 1):MMM yyyy}",
                Amount = s.NetSalary,
                Timestamp = s.PaymentDate!.Value,
                PerformedBy = s.ProcessedBy != null ? s.ProcessedBy.Username : "System"
            })
            .ToListAsync();

        // Recent Enrollments
        var enrollments = await _context.ClassChildren
            .Include(cc => cc.Child)
            .Include(cc => cc.Class)
            .Where(cc => cc.Class.MosqueId == mosqueId)
            .OrderByDescending(cc => cc.EnrollmentDate)
            .Take(count)
            .Select(cc => new RecentActivityDto
            {
                ActivityType = "Enrollment",
                Description = $"{cc.Child.FullName} enrolled in {cc.Class.Name}",
                Amount = null,
                Timestamp = cc.EnrollmentDate,
                PerformedBy = "System"
            })
            .ToListAsync();

        activities.AddRange(feePayments);
        activities.AddRange(salaryPayments);
        activities.AddRange(enrollments);

        return activities
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToList();
    }

    private async Task<List<TopPerformerDto>> GetTopPerformersAsync(int mosqueId)
    {
        var imaams = await _context.Imaams
            .Where(i => i.MosqueId == mosqueId)
            .Include(i => i.Classes)
            .Include(i => i.Subjects)
            .Select(i => new TopPerformerDto
            {
                ImaamId = i.Id,
                FullName = i.FullName,
                ClassesCount = i.Classes.Count,
                SubjectsCount = i.Subjects.Count
            })
            .OrderByDescending(i => i.ClassesCount + i.SubjectsCount)
            .Take(5)
            .ToListAsync();

        return imaams;
    }

    private async Task<ChartDataDto> GetFeeCollectionByClassAsync(int mosqueId, int year, int month)
    {
        var data = await _context.ChildFees
            .Where(f => f.MosqueId == mosqueId && f.Year == year && f.Month == month)
            .Include(f => f.Class)
            .GroupBy(f => f.Class.Name)
            .Select(g => new { ClassName = g.Key, TotalCollected = g.Sum(f => f.AmountPaid) })
            .ToListAsync();

        return new ChartDataDto
        {
            Labels = data.Select(d => d.ClassName).ToArray(),
            Data = data.Select(d => d.TotalCollected).ToArray()
        };
    }

    private async Task<ChartDataDto> GetTodayAttendanceStatusAsync(int mosqueId, DateTime today)
    {
        var attendance = await _context.ChildAttendances
            .Where(a => a.MosqueId == mosqueId && a.Date.Date == today)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var statuses = new[] { "Present", "Absent", "Late", "OnLeave", "Excused" };
        var labels = new List<string>();
        var data = new List<decimal>();

        foreach (var status in statuses)
        {
            var count = attendance.FirstOrDefault(a => a.Status == status)?.Count ?? 0;
            if (count > 0 || status == "Present") // Always show Present even if 0
            {
                labels.Add(status);
                data.Add(count);
            }
        }

        return new ChartDataDto
        {
            Labels = labels.ToArray(),
            Data = data.ToArray()
        };
    }
}