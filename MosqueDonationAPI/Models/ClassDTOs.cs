using MosqueDonationAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class CreateClassRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Description { get; set; }
    public int? MaxCapacity { get; set; }
    public int MosqueId { get; set; }
    public int? ClassTeacherId { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public class ClassDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public int? ClassTeacherId { get; set; }
    public int? MaxCapacity { get; set; }
    public string? ClassTeacherName { get; set; }
    public string? Description { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int MosqueId { get; set; }
    public List<ClassSubjectResponse> Subjects { get; set; } = new();
    public List<ChildBriefResponse> Children { get; set; } = new();
}
public class AddSubjectToClassRequest
{
    public int SubjectId { get; set; }
    public int? AssignedImaamId { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public class ChildBriefResponse
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public int? RollNumber { get; set; }
    public string? Status { get; set; }
    public string? GuardianPhone { get; set; }
}

public class ClassListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Class 1", "Hifz Section A"
    public string? Section { get; set; } // e.g., "A", "B", "Boys", "Girls"
    public string? Description { get; set; }
    public int? MaxCapacity { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    public int MosqueId { get; set; }
    public int? ClassTeacherId { get; set; }
    public int? EnrolledChildrenCount { get; set; }
    public int? SubjectCount { get; set; }
    public string? ClassTeacherName { get; set; }

}
public class AssignSubjectRequest
{
    [Required]
    public int SubjectId { get; set; }

    public int? AssignedImaamId { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public class UpdateClassSubjectRequest
{
    public int? AssignedImaamId { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public class ClassSubjectResponse
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? SubjectCode { get; set; }
    public int? AssignedImaamId { get; set; }
    public string? AssignedImaamName { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}