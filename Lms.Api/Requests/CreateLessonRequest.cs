using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Requests;

public sealed class CreateLessonRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    [Range(0, int.MaxValue)]
    public int DurationMinutes { get; set; }
}
