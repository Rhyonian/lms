namespace Lms.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public string? PasswordHash { get; set; }
}
