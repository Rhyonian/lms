namespace Lms.Api.Models;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Learner = "Learner";

    public static readonly string[] All = { Admin, Learner };
}
