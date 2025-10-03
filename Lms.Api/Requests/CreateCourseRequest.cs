using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Requests;

public sealed class CreateCourseRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
}
