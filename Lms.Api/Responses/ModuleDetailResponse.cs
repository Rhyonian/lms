namespace Lms.Api.Responses;

public sealed record ModuleDetailResponse(Guid Id, string Title, string? Description, int DisplayOrder, IReadOnlyCollection<LessonDetailResponse> Lessons);
