using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class AuthModels
{
}
public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? AssignedMosqueId { get; set; }
    public string? AssignedMosqueName { get; set; }
    public bool IsActive { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? AssignedMosqueId { get; set; }  // NEW
    public string? AssignedMosqueName { get; set; }  // NEW
    public DateTime ExpiresAt { get; set; }
}

public class CreateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "User";
    public int? AssignedMosqueId { get; set; }
}