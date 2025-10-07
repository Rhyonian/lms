using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Models;

public class User
{
    public Guid Id { get; set; }

    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(200)]
    public required string FullName { get; set; }

    [MaxLength(32)]
    public required string Role { get; set; }

    [MaxLength(512)]
    public required string PasswordHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
