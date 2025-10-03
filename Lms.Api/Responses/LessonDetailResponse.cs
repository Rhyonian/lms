namespace Lms.Api.Responses;

public sealed record LessonDetailResponse(Guid Id, string Title, string? Content, int DisplayOrder, int DurationMinutes);
