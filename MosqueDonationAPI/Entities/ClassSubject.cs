namespace MosqueDonationAPI.Entities;

public class ClassSubject : BaseEntity
{
    public int ClassId { get; set; }
    public virtual Class Class { get; set; } = null!;

    public int SubjectId { get; set; }
    public virtual Subject Subject { get; set; } = null!;

    // Assigned Teacher for this specific class-subject combination
    public int? AssignedImaamId { get; set; }
    public virtual Imaam? AssignedImaam { get; set; }

    // Schedule for this subject in this class
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}