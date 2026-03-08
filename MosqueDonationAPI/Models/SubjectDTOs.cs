using MosqueDonationAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace MosqueDonationAPI.Models;

public class CreateSubjectRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int MosqueId { get; set; }
    public int? DefaultImaamId { get; set; }
}
public class UpdateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int MosqueId { get; set; }
    public int? DefaultImaamId { get; set; }
}
public class SubjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int MosqueId { get; set; }
    public int? DefaultImaamId { get; set; }
    public string? DefaultImaamName { get; set; }
    public int? ClassCount { get; set; }
}
