namespace Lms.Api.Responses;

public sealed record CourseSummaryResponse(Guid Id, string Title, string? Description, DateTimeOffset CreatedAt, DateTimeOffset? PublishedAt, int ModuleCount, int EnrollmentCount);
