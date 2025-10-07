namespace Lms.Api.Models;

public class LessonProgress
{
    public Guid Id { get; set; }

    public Guid EnrollmentId { get; set; }
    public Enrollment Enrollment { get; set; } = null!;

    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public decimal ProgressPercent { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
