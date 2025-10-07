namespace Lms.Api.Responses;

public sealed record UserSummaryResponse(Guid Id, string Email, string FullName, string Role, DateTimeOffset CreatedAt);
