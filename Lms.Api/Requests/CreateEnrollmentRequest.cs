using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Requests;

public sealed class CreateEnrollmentRequest
{
    [Required]
    public Guid UserId { get; set; }
}
