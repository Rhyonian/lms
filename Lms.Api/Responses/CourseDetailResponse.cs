namespace Lms.Api.Responses;

public sealed record CourseDetailResponse(Guid Id, string Title, string? Description, DateTimeOffset CreatedAt, DateTimeOffset? PublishedAt, IReadOnlyCollection<ModuleDetailResponse> Modules);
