using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class CreateImaamRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Qualification { get; set; }
    public DateTime? JoiningDate { get; set; }
    public int MosqueId { get; set; }
}

public class ImaamResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Qualification { get; set; }
    public int MosqueId { get; set; }
    public string? MosqueName { get; set; }
    public int AssignedClassesCount { get; set; }
}
public class ImaamDetailResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Qualification { get; set; }

    public DateTime? JoiningDate { get; set; }

    public int MosqueId { get; set; }
    public List<ClassBriefResponse> Classes { get; set; } = new();
    public List<SubjectBriefResponse> Subjects { get; set; } = new();
}
public class ClassBriefResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Section { get; set; }
}
public class SubjectBriefResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
}