using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MosqueDonationAPI.Entities;

namespace MosqueDonationAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Mosque> Mosques => Set<Mosque>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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

    }
}