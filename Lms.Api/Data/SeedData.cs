using System.Security.Cryptography;
using System.Text;
using Lms.Api.Models;

namespace Lms.Api.Data;

public static class SeedData
{
    public static readonly Guid AdminId = Guid.Parse("d2a8c4fd-3bee-4e02-9f7f-141e58aabfaa");
    public static readonly Guid[] LearnerIds =
    {
        Guid.Parse("8f0a3bc4-9d6f-4ac7-8c30-0550aa8a61bd"),
        Guid.Parse("a0d2c0f1-4e35-4af9-9b27-37d1b8cb0d7b"),
        Guid.Parse("2b9d6c6b-8e8e-4a79-8b1c-8c5901a6e35f"),
        Guid.Parse("a78ad293-6cd1-4e91-9f6c-98eb4146d5e0"),
        Guid.Parse("abf4b18c-bde3-4d49-89b7-6f1258f1be98")
    };

    private static readonly DateTimeOffset SeedTimestamp = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<User> Users { get; } = new List<User>
    {
        new()
        {
            Id = AdminId,
            Email = "admin@lms.test",
            FullName = "System Administrator",
            Role = UserRoles.Admin,
            PasswordHash = ComputeHash("Admin#2024"),
            CreatedAt = SeedTimestamp
        },
        new()
        {
            Id = LearnerIds[0],
            Email = "learner1@lms.test",
            FullName = "Learner One",
            Role = UserRoles.Learner,
            PasswordHash = ComputeHash("Learner#1"),
            CreatedAt = SeedTimestamp
        },
        new()
        {
            Id = LearnerIds[1],
            Email = "learner2@lms.test",
            FullName = "Learner Two",
            Role = UserRoles.Learner,
            PasswordHash = ComputeHash("Learner#2"),
            CreatedAt = SeedTimestamp
        },
        new()
        {
            Id = LearnerIds[2],
            Email = "learner3@lms.test",
            FullName = "Learner Three",
            Role = UserRoles.Learner,
            PasswordHash = ComputeHash("Learner#3"),
            CreatedAt = SeedTimestamp
        },
        new()
        {
            Id = LearnerIds[3],
            Email = "learner4@lms.test",
            FullName = "Learner Four",
            Role = UserRoles.Learner,
            PasswordHash = ComputeHash("Learner#4"),
            CreatedAt = SeedTimestamp
        },
        new()
        {
            Id = LearnerIds[4],
            Email = "learner5@lms.test",
            FullName = "Learner Five",
            Role = UserRoles.Learner,
            PasswordHash = ComputeHash("Learner#5"),
            CreatedAt = SeedTimestamp
        }
    };

    private static string ComputeHash(string value)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
