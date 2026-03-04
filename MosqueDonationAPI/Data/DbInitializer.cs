using MosqueDonationAPI.Entities;
using System.Security.Cryptography;
using System.Text;

namespace MosqueDonationAPI.Data;

public static class DbInitializer
{
    public static async Task SeedData(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Seed Admin User if not exists
        if (!context.Users.Any())
        {
            // Sample Mosques
            var mosques = new List<Mosque>
        {
            new Mosque
            {
                Name = "Jama Masjid",
                ShortName = "JM",
                Address = "123 Main Road, Old Delhi",
                City = "Delhi",
                State = "Delhi",
                Phone = "011-12345678"
            },
            new Mosque
            {
                Name = "Madarsa Arabia",
                ShortName = "MA",
                Address = "456 Market Street",
                City = "Lucknow",
                State = "Uttar Pradesh",
                Phone = "0522-87654321"
            }
        };
            context.Mosques.AddRange(mosques);
            await context.SaveChangesAsync();
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@mosque.com",
                PasswordHash = HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                AssignedMosqueId = 1
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync(); // Pehle user save karo taaki ID generate ho


        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}