using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Requests;

public sealed class UpdateEnrollmentStatusRequest
{
    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = string.Empty;
}
