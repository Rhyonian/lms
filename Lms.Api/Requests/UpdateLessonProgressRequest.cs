using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Requests;

public sealed class UpdateLessonProgressRequest
{
    [Required]
    public Guid LessonId { get; set; }

    [Range(0, 100)]
    public decimal ProgressPercent { get; set; }

    public bool MarkComplete { get; set; }
}
