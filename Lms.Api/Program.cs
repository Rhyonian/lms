using Lms.Api.Data;
using Lms.Api.Extensions;
using Lms.Api.Models;
using Lms.Api.Options;
using Lms.Api.Requests;
using Lms.Api.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LmsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseSnakeCaseNamingConvention();
});

builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("Pagination"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying migrations");
    }
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

var userGroup = app.MapGroup("/users");
userGroup.MapGet("", async (LmsDbContext db, IOptions<PaginationOptions> paginationOptions, int pageNumber = 1, int pageSize = 0, string? role = null) =>
{
    var (normalizedPage, normalizedSize) = paginationOptions.Value.Normalize(pageNumber, pageSize);

    var query = db.Users.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(role))
    {
        query = query.Where(u => u.Role == role);
    }

    var total = await query.LongCountAsync();
    var users = await query
        .OrderBy(u => u.FullName)
        .Skip((normalizedPage - 1) * normalizedSize)
        .Take(normalizedSize)
        .Select(u => new UserSummaryResponse(u.Id, u.Email, u.FullName, u.Role, u.CreatedAt))
        .ToListAsync();

    return Results.Ok(new PagedResult<UserSummaryResponse>(total, normalizedPage, normalizedSize, users));
}).RequireRoles(UserRoles.Admin);

var coursesGroup = app.MapGroup("/courses");

coursesGroup.MapGet("", async (LmsDbContext db, IOptions<PaginationOptions> paginationOptions, int pageNumber = 1, int pageSize = 0, string? search = null) =>
{
    var (normalizedPage, normalizedSize) = paginationOptions.Value.Normalize(pageNumber, pageSize);
    var query = db.Courses.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(c => EF.Functions.ILike(c.Title, $"%{search}%"));
    }

    var total = await query.LongCountAsync();

    var items = await query
        .OrderBy(c => c.Title)
        .Skip((normalizedPage - 1) * normalizedSize)
        .Take(normalizedSize)
        .Select(c => new CourseSummaryResponse(
            c.Id,
            c.Title,
            c.Description,
            c.CreatedAt,
            c.PublishedAt,
            c.Modules.Count,
            c.Enrollments.Count))
        .ToListAsync();

    return Results.Ok(new PagedResult<CourseSummaryResponse>(total, normalizedPage, normalizedSize, items));
}).RequireRoles(UserRoles.Admin, UserRoles.Learner);

coursesGroup.MapGet("/{courseId:guid}", async (Guid courseId, LmsDbContext db) =>
{
    var course = await db.Courses
        .AsNoTracking()
        .Where(c => c.Id == courseId)
        .Select(c => new CourseDetailResponse(
            c.Id,
            c.Title,
            c.Description,
            c.CreatedAt,
            c.PublishedAt,
            c.Modules
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new ModuleDetailResponse(
                    m.Id,
                    m.Title,
                    m.Description,
                    m.DisplayOrder,
                    m.Lessons
                        .OrderBy(l => l.DisplayOrder)
                        .Select(l => new LessonDetailResponse(l.Id, l.Title, l.Content, l.DisplayOrder, l.DurationMinutes))
                        .ToList()))
                .ToList()))
        .SingleOrDefaultAsync();

    return course is not null ? Results.Ok(course) : Results.NotFound();
}).RequireRoles(UserRoles.Admin, UserRoles.Learner);

coursesGroup.MapPost("", async (CreateCourseRequest request, LmsDbContext db) =>
{
    var now = DateTimeOffset.UtcNow;
    var course = new Course
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        Description = request.Description,
        CreatedAt = now,
        PublishedAt = request.PublishedAt
    };

    db.Courses.Add(course);
    await db.SaveChangesAsync();

    return Results.Created($"/courses/{course.Id}", new CourseDetailResponse(course.Id, course.Title, course.Description, course.CreatedAt, course.PublishedAt, Array.Empty<ModuleDetailResponse>()));
}).RequireRoles(UserRoles.Admin);

coursesGroup.MapPut("/{courseId:guid}", async (Guid courseId, UpdateCourseRequest request, LmsDbContext db) =>
{
    var course = await db.Courses.FindAsync(courseId);
    if (course is null)
    {
        return Results.NotFound();
    }

    course.Title = request.Title;
    course.Description = request.Description;
    course.PublishedAt = request.PublishedAt;
    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireRoles(UserRoles.Admin);

coursesGroup.MapDelete("/{courseId:guid}", async (Guid courseId, LmsDbContext db) =>
{
    var course = await db.Courses.FindAsync(courseId);
    if (course is null)
    {
        return Results.NotFound();
    }

    db.Courses.Remove(course);
    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireRoles(UserRoles.Admin);

coursesGroup.MapPost("/{courseId:guid}/modules", async (Guid courseId, CreateModuleRequest request, LmsDbContext db) =>
{
    var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);
    if (!courseExists)
    {
        return Results.NotFound();
    }

    var module = new Module
    {
        Id = Guid.NewGuid(),
        CourseId = courseId,
        Title = request.Title,
        Description = request.Description,
        DisplayOrder = request.DisplayOrder
    };

    db.Modules.Add(module);
    await db.SaveChangesAsync();

    return Results.Created($"/modules/{module.Id}", module.Id);
}).RequireRoles(UserRoles.Admin);

app.MapPost("/modules/{moduleId:guid}/lessons", async (Guid moduleId, CreateLessonRequest request, LmsDbContext db) =>
{
    var moduleExists = await db.Modules.AnyAsync(m => m.Id == moduleId);
    if (!moduleExists)
    {
        return Results.NotFound();
    }

    var lesson = new Lesson
    {
        Id = Guid.NewGuid(),
        ModuleId = moduleId,
        Title = request.Title,
        Content = request.Content,
        DisplayOrder = request.DisplayOrder,
        DurationMinutes = request.DurationMinutes
    };

    db.Lessons.Add(lesson);
    await db.SaveChangesAsync();

    return Results.Created($"/lessons/{lesson.Id}", lesson.Id);
}).RequireRoles(UserRoles.Admin);

coursesGroup.MapGet("/{courseId:guid}/enrollments", async (Guid courseId, LmsDbContext db, IOptions<PaginationOptions> paginationOptions, int pageNumber = 1, int pageSize = 0) =>
{
    var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);
    if (!courseExists)
    {
        return Results.NotFound();
    }

    var (normalizedPage, normalizedSize) = paginationOptions.Value.Normalize(pageNumber, pageSize);

    var query = db.Enrollments.AsNoTracking().Where(e => e.CourseId == courseId);
    var total = await query.LongCountAsync();
    var items = await query
        .OrderByDescending(e => e.EnrolledAt)
        .Skip((normalizedPage - 1) * normalizedSize)
        .Take(normalizedSize)
        .Select(e => new EnrollmentResponse(e.Id, e.CourseId, e.UserId, e.Status, e.EnrolledAt))
        .ToListAsync();

    return Results.Ok(new PagedResult<EnrollmentResponse>(total, normalizedPage, normalizedSize, items));
}).RequireRoles(UserRoles.Admin);

coursesGroup.MapPost("/{courseId:guid}/enrollments", async (Guid courseId, CreateEnrollmentRequest request, LmsDbContext db) =>
{
    var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == request.UserId);
    if (user is null)
    {
        return Results.BadRequest(new { message = "Learner not found." });
    }

    if (!string.Equals(user.Role, UserRoles.Learner, StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new { message = "Only learners can be enrolled." });
    }

    var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);
    if (!courseExists)
    {
        return Results.NotFound();
    }

    var alreadyEnrolled = await db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == request.UserId);
    if (alreadyEnrolled)
    {
        return Results.Conflict(new { message = "Learner is already enrolled in this course." });
    }

    var enrollment = new Enrollment
    {
        Id = Guid.NewGuid(),
        CourseId = courseId,
        UserId = request.UserId,
        EnrolledAt = DateTimeOffset.UtcNow,
        Status = EnrollmentStatus.Active
    };

    db.Enrollments.Add(enrollment);
    await db.SaveChangesAsync();

    return Results.Created($"/enrollments/{enrollment.Id}", new EnrollmentResponse(enrollment.Id, enrollment.CourseId, enrollment.UserId, enrollment.Status, enrollment.EnrolledAt));
}).RequireRoles(UserRoles.Admin);

app.MapGet("/learners/{learnerId:guid}/enrollments", async (Guid learnerId, HttpContext httpContext, LmsDbContext db, IOptions<PaginationOptions> paginationOptions, int pageNumber = 1, int pageSize = 0) =>
{
    var currentRole = httpContext.GetUserRole();
    var currentUserId = httpContext.GetUserId();
    var isSelf = currentUserId.HasValue && currentUserId.Value == learnerId;
    if (!string.Equals(currentRole, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && !isSelf)
    {
        return Results.Forbid();
    }

    var (normalizedPage, normalizedSize) = paginationOptions.Value.Normalize(pageNumber, pageSize);

    var query = db.Enrollments.AsNoTracking().Where(e => e.UserId == learnerId);
    var total = await query.LongCountAsync();
    var items = await query
        .OrderByDescending(e => e.EnrolledAt)
        .Skip((normalizedPage - 1) * normalizedSize)
        .Take(normalizedSize)
        .Select(e => new EnrollmentResponse(e.Id, e.CourseId, e.UserId, e.Status, e.EnrolledAt))
        .ToListAsync();

    return Results.Ok(new PagedResult<EnrollmentResponse>(total, normalizedPage, normalizedSize, items));
}).RequireRoles(UserRoles.Admin, UserRoles.Learner);

app.MapPut("/enrollments/{enrollmentId:guid}/status", async (Guid enrollmentId, UpdateEnrollmentStatusRequest request, LmsDbContext db) =>
{
    if (!EnrollmentStatus.All.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new { message = "Invalid enrollment status." });
    }

    var enrollment = await db.Enrollments.FindAsync(enrollmentId);
    if (enrollment is null)
    {
        return Results.NotFound();
    }

    enrollment.Status = request.Status;
    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireRoles(UserRoles.Admin);

app.MapPut("/enrollments/{enrollmentId:guid}/progress", async (Guid enrollmentId, UpdateLessonProgressRequest request, HttpContext httpContext, LmsDbContext db) =>
{
    var enrollment = await db.Enrollments.Include(e => e.User).SingleOrDefaultAsync(e => e.Id == enrollmentId);
    if (enrollment is null)
    {
        return Results.NotFound();
    }

    var currentRole = httpContext.GetUserRole();
    var currentUserId = httpContext.GetUserId();
    var isSelf = currentUserId.HasValue && currentUserId.Value == enrollment.UserId;

    if (!string.Equals(currentRole, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && !isSelf)
    {
        return Results.Forbid();
    }

    var lessonExists = await db.Lessons.AnyAsync(l => l.Id == request.LessonId);
    if (!lessonExists)
    {
        return Results.BadRequest(new { message = "Lesson not found." });
    }

    var progress = await db.LessonProgress.SingleOrDefaultAsync(lp => lp.EnrollmentId == enrollmentId && lp.LessonId == request.LessonId);
    if (progress is null)
    {
        progress = new LessonProgress
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollmentId,
            LessonId = request.LessonId,
            ProgressPercent = request.ProgressPercent,
            CompletedAt = request.MarkComplete ? DateTimeOffset.UtcNow : null
        };
        db.LessonProgress.Add(progress);
    }
    else
    {
        progress.ProgressPercent = request.ProgressPercent;
        progress.CompletedAt = request.MarkComplete ? DateTimeOffset.UtcNow : progress.CompletedAt;
    }

    await db.SaveChangesAsync();

    return Results.Ok(new LessonProgressResponse(progress.LessonId, progress.EnrollmentId, progress.ProgressPercent, progress.CompletedAt));
}).RequireRoles(UserRoles.Admin, UserRoles.Learner);

app.MapGet("/learners/{learnerId:guid}/progress", async (Guid learnerId, HttpContext httpContext, LmsDbContext db) =>
{
    var currentRole = httpContext.GetUserRole();
    var currentUserId = httpContext.GetUserId();
    var isSelf = currentUserId.HasValue && currentUserId.Value == learnerId;
    if (!string.Equals(currentRole, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && !isSelf)
    {
        return Results.Forbid();
    }

    var progress = await db.Enrollments
        .AsNoTracking()
        .Where(e => e.UserId == learnerId)
        .Select(e => new LearnerProgressResponse(
            e.CourseId,
            e.Course.Title,
            e.Id,
            e.Status,
            e.ProgressEntries
                .Select(lp => new LearnerLessonProgressItem(
                    lp.LessonId,
                    lp.Lesson.Title,
                    lp.ProgressPercent,
                    lp.CompletedAt))
                .ToList()))
        .ToListAsync();

    return Results.Ok(progress);
}).RequireRoles(UserRoles.Admin, UserRoles.Learner);

app.Run();
