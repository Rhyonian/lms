using Lms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Data;

public class LmsDbContext(DbContextOptions<LmsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasCheckConstraint("ck_users_role", $"role in ('{UserRoles.Admin}', '{UserRoles.Learner}')");
            entity.HasData(SeedData.Users);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.PublishedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("modules");
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.DisplayOrder).IsRequired();
            entity.HasIndex(e => new { e.CourseId, e.DisplayOrder }).IsUnique();
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("lessons");
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.DisplayOrder).IsRequired();
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.HasIndex(e => new { e.ModuleId, e.DisplayOrder }).IsUnique();
            entity.HasOne(e => e.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollments");
            entity.Property(e => e.EnrolledAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
            entity.HasCheckConstraint("ck_enrollments_status", $"status in ('{EnrollmentStatus.Active}', '{EnrollmentStatus.Completed}', '{EnrollmentStatus.Dropped}')");
            entity.HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.ToTable("lesson_progress");
            entity.Property(e => e.ProgressPercent).HasPrecision(5, 2);
            entity.Property(e => e.CompletedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(e => new { e.EnrollmentId, e.LessonId }).IsUnique();
            entity.HasOne(e => e.Enrollment)
                .WithMany(e => e.ProgressEntries)
                .HasForeignKey(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.ProgressEntries)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
