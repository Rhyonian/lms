namespace Lms.Api.Responses;

public sealed record LearnerProgressResponse(Guid CourseId, string CourseTitle, Guid EnrollmentId, string Status, IReadOnlyCollection<LearnerLessonProgressItem> Lessons);

public sealed record LearnerLessonProgressItem(Guid LessonId, string LessonTitle, decimal ProgressPercent, DateTimeOffset? CompletedAt);
