using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MosqueDonationAPI.Data;
using MosqueDonationAPI.Entities;
using MosqueDonationAPI.Models;
using MosqueDonationAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace MosqueDonationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.AssignedMosque)  // Include mosque
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            AssignedMosqueId = user.AssignedMosqueId,
            AssignedMosqueName = user.AssignedMosque?.Name,
            ExpiresAt = DateTime.Now.AddHours(8)
        });
    }

    [HttpPost("register")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest(new { message = "Username already exists" });
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Email already exists" });
        }

        // Verify mosque exists if provided
        if (request.AssignedMosqueId.HasValue)
        {
            var mosque = await _context.Mosques.FindAsync(request.AssignedMosqueId.Value);
            if (mosque == null)
            {
                return BadRequest(new { message = "Assigned mosque not found" });
            }
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = request.Role,
            AssignedMosqueId = request.AssignedMosqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User created successfully", userId = user.Id });
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.AssignedMosque)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                AssignedMosqueId = u.AssignedMosqueId,
                AssignedMosqueName = u.AssignedMosque != null ? u.AssignedMosque.Name : null,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("users/{id}/assign-mosque")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignMosque(int id, [FromBody] int? mosqueId)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (mosqueId.HasValue)
        {
            var mosque = await _context.Mosques.FindAsync(mosqueId.Value);
            if (mosque == null)
            {
                return BadRequest(new { message = "Mosque not found" });
            }
        }

        user.AssignedMosqueId = mosqueId;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Mosque assigned successfully" });
    }
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}