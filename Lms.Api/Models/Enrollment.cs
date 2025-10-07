namespace Lms.Api.Models;

public class Enrollment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTimeOffset EnrolledAt { get; set; }

    public string Status { get; set; } = EnrollmentStatus.Active;

    public ICollection<LessonProgress> ProgressEntries { get; set; } = new List<LessonProgress>();
}

public static class EnrollmentStatus
{
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Dropped = "Dropped";

    public static readonly string[] All = { Active, Completed, Dropped };
}
