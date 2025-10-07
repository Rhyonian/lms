using Lms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.GivenName).HasColumnName("given_name");
            entity.Property(e => e.FamilyName).HasColumnName("family_name");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
        });
    }
}
