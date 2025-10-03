namespace Lms.Api.Responses;

public sealed record EnrollmentResponse(Guid Id, Guid CourseId, Guid UserId, string Status, DateTimeOffset EnrolledAt);
