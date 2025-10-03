using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Models;

public class Lesson
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    [MaxLength(200)]
    public required string Title { get; set; }

    public string? Content { get; set; }

    public int DisplayOrder { get; set; }

    public int DurationMinutes { get; set; }

    public ICollection<LessonProgress> ProgressEntries { get; set; } = new List<LessonProgress>();
}
