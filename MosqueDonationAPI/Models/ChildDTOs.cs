using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class CreateChildRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianEmail { get; set; }
    public string? GuardianPhone { get; set; }
    public string? Address { get; set; }
    public int MosqueId { get; set; }
}

public class ChildResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianPhone { get; set; }

    public DateTime? EnrollmentDate { get; set; }
    public int? ActiveClassesCount { get; set; }
}

public class EnrollChildRequest
{
    public int ChildId { get; set; }
    public int ClassId { get; set; }
    public int? RollNumber { get; set; }
    public string? Notes { get; set; }
}
public class EnrollmentResponse
{

    public int ClassId { get; set; }
    public string? ClassName { get; set; }
    public string? Section{ get; set; }
    public int? RollNumber { get; set; }
    public string? Status { get; set; }
    public DateTime EnrollmentDate { get; set; }
}
public class ChildDetailResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; } // Male, Female

    public string? GuardianName { get; set; }

    public string? GuardianPhone { get; set; }

    public string? GuardianEmail { get; set; }

    public string? Address { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    public int MosqueId { get; set; }
    public IList<EnrollmentResponse> Enrollments { get; set; } = new List<EnrollmentResponse>();
}
public class UpdateEnrollmentRequest
{
    public int RollNumber { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}