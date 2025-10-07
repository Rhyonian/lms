using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Models;

public class Module
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
