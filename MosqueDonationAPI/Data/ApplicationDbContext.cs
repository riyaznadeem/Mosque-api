using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Entities;

namespace MosqueDonationAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Mosque> Mosques => Set<Mosque>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Imaam> Imaams { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<ClassSubject> ClassSubjects { get; set; }
    public DbSet<Child> Children { get; set; }
    public DbSet<ClassChild> ClassChildren { get; set; }
    public DbSet<ImaamAttendance> ImaamAttendances { get; set; }
    public DbSet<ChildAttendance> ChildAttendances { get; set; }
    public DbSet<ImaamSalary> ImaamSalaries { get; set; }
    public DbSet<ChildFee> ChildFees { get; set; }
    public DbSet<FeePayment> FeePayments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Mosque>()
            .HasIndex(m => m.Name);

        // User-Mosque relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.AssignedMosque)
            .WithMany()
            .HasForeignKey(u => u.AssignedMosqueId)
            .OnDelete(DeleteBehavior.SetNull);

        // IMPORTANT: Foreign key relationships sahi se define karo
        modelBuilder.Entity<Donation>()
            .HasOne(d => d.Mosque)
            .WithMany(m => m.Donations)
            .HasForeignKey(d => d.MosqueId)
            .OnDelete(DeleteBehavior.Restrict); // Cascade mat karo, restrict karo

        modelBuilder.Entity<Donation>()
            .HasOne(d => d.ReceivedBy)
            .WithMany(u => u.Donations)
            .HasForeignKey(d => d.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Imaam - Mosque
        modelBuilder.Entity<Imaam>()
            .HasOne(i => i.Mosque)
            .WithMany()
            .HasForeignKey(i => i.MosqueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Subject - Mosque
        modelBuilder.Entity<Subject>()
            .HasOne(s => s.Mosque)
            .WithMany()
            .HasForeignKey(s => s.MosqueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Class - Mosque
        modelBuilder.Entity<Class>()
            .HasOne(c => c.Mosque)
            .WithMany()
            .HasForeignKey(c => c.MosqueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Class - ClassTeacher (Imaam)
        modelBuilder.Entity<Class>()
            .HasOne(c => c.ClassTeacher)
            .WithMany(i => i.Classes)
            .HasForeignKey(c => c.ClassTeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        // ClassSubject (Many-to-Many)
        modelBuilder.Entity<ClassSubject>()
            .HasKey(cs => new { cs.ClassId, cs.SubjectId });

        modelBuilder.Entity<ClassSubject>()
            .HasOne(cs => cs.Class)
            .WithMany(c => c.ClassSubjects)
            .HasForeignKey(cs => cs.ClassId);

        modelBuilder.Entity<ClassSubject>()
            .HasOne(cs => cs.Subject)
            .WithMany(s => s.ClassSubjects)
            .HasForeignKey(cs => cs.SubjectId);

        // Child - Mosque
        modelBuilder.Entity<Child>()
            .HasOne(c => c.Mosque)
            .WithMany()
            .HasForeignKey(c => c.MosqueId)
            .OnDelete(DeleteBehavior.Cascade);

        // ClassChild (Many-to-Many Enrollment)
        modelBuilder.Entity<ClassChild>()
            .HasKey(cc => new { cc.ClassId, cc.ChildId });

        modelBuilder.Entity<ClassChild>()
            .HasOne(cc => cc.Class)
            .WithMany(c => c.ClassChildren)
            .HasForeignKey(cc => cc.ClassId);

        modelBuilder.Entity<ClassChild>()
            .HasOne(cc => cc.Child)
            .WithMany(c => c.ClassEnrollments)
            .HasForeignKey(cc => cc.ChildId);

        // Unique constraints
        modelBuilder.Entity<Subject>()
            .HasIndex(s => new { s.MosqueId, s.Code })
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        modelBuilder.Entity<ClassChild>()
            .HasIndex(cc => new { cc.ClassId, cc.RollNumber })
            .IsUnique()
            .HasFilter("[RollNumber] IS NOT NULL");

        // Imaam Attendance
        modelBuilder.Entity<ImaamAttendance>()
            .HasIndex(ia => new { ia.ImaamId, ia.Date })
            .IsUnique();

        modelBuilder.Entity<ImaamAttendance>()
            .HasOne(ia => ia.Imaam)
            .WithMany()
            .HasForeignKey(ia => ia.ImaamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Child Attendance
        modelBuilder.Entity<ChildAttendance>()
            .HasIndex(ca => new { ca.ChildId, ca.Date })
            .IsUnique();

        modelBuilder.Entity<ChildAttendance>()
            .HasOne(ca => ca.Child)
            .WithMany()
            .HasForeignKey(ca => ca.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChildAttendance>()
            .HasOne(ca => ca.Class)
            .WithMany()
            .HasForeignKey(ca => ca.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Imaam Salary
        modelBuilder.Entity<ImaamSalary>()
            .HasIndex(s => new { s.ImaamId, s.Year, s.Month })
            .IsUnique();

        modelBuilder.Entity<ImaamSalary>()
            .HasOne(s => s.Imaam)
            .WithMany()
            .HasForeignKey(s => s.ImaamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Child Fee
        modelBuilder.Entity<ChildFee>()
            .HasIndex(f => new { f.ChildId, f.Year, f.Month })
            .IsUnique();

        modelBuilder.Entity<ChildFee>()
            .HasOne(f => f.Child)
            .WithMany()
            .HasForeignKey(f => f.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChildFee>()
            .HasOne(f => f.Class)
            .WithMany()
            .HasForeignKey(f => f.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Fee Payment
        modelBuilder.Entity<FeePayment>()
            .HasOne(fp => fp.ChildFee)
            .WithMany(cf => cf.FeePayments)
            .HasForeignKey(fp => fp.ChildFeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}