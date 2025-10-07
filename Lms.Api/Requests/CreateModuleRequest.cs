using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Requests;

public sealed class CreateModuleRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }
}
