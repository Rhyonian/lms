namespace Lms.Api.Responses;

public sealed record LessonProgressResponse(Guid LessonId, Guid EnrollmentId, decimal ProgressPercent, DateTimeOffset? CompletedAt);
