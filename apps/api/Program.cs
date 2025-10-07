using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lms.Api.Data;
using Lms.Api.Models;
using Lms.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=lms.db";
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddSingleton<JwtIssuer>();

var jwtSecret = builder.Configuration["JWT_SECRET"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT secret is not configured. Set JWT_SECRET in the environment.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            NameClaimType = ClaimTypes.Email,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireClaim(ClaimTypes.Role)
        .Build();
});

const string CorsPolicyName = "SpaClient";
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (allowedOrigins.Length == 0)
{
    allowedOrigins = new[] { "http://localhost:5173" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();

// Preserve existing OIDC wiring for future external identity providers.
// Local email/password authentication is additive and does not remove OIDC stubs.

var app = builder.Build();

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
    context.Database.Migrate();

    const string adminEmail = "admin@example.com";
    const string adminPassword = "Admin123!";

    var admin = context.Users.SingleOrDefault(u => u.Email == adminEmail);
    if (admin is null)
    {
        admin = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            GivenName = "Admin",
            FamilyName = "User",
            Role = "Administrator",
            IsDeleted = false,
        };
        context.Users.Add(admin);
    }

    if (admin.PasswordHash is null || !passwordHasher.Verify(adminPassword, admin.PasswordHash))
    {
        admin.PasswordHash = passwordHasher.Hash(adminPassword);
    }

    if (admin.Role is null or "")
    {
        admin.Role = "Administrator";
    }

    context.SaveChanges();

    if (app.Environment.IsDevelopment())
    {
        app.Logger.LogInformation("Development admin credentials: {Email} / {Password}", adminEmail, adminPassword);
    }
}

app.MapPost("/auth/login", async (LoginRequest request, ApplicationDbContext db, PasswordHasher passwordHasher, JwtIssuer jwtIssuer, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
    {
        return Results.BadRequest("invalid credentials");
    }

    var normalizedEmail = request.Email.Trim().ToLowerInvariant();
    var user = await db.Users
        .AsNoTracking()
        .SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted, cancellationToken);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrEmpty(user.PasswordHash))
    {
        return Results.BadRequest("password auth disabled");
    }

    if (!passwordHasher.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = jwtIssuer.Issue(user);

    return Results.Ok(new
    {
        accessToken = token.AccessToken,
        tokenType = token.TokenType,
        expiresIn = 60 * 60 * 24,
    });
});

app.MapPost("/auth/logout", () => Results.NoContent());

app.MapGet("/me", (ClaimsPrincipal principal) =>
{
    var id = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
    return Results.Ok(new
    {
        id,
        email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email),
        given_name = principal.FindFirstValue(ClaimTypes.GivenName),
        family_name = principal.FindFirstValue(ClaimTypes.Surname),
        role = principal.FindFirstValue(ClaimTypes.Role)
    });
}).RequireAuthorization();

app.MapGet("/auth/oidc/callback", () => Results.StatusCode(StatusCodes.Status501NotImplemented));

app.Run();

public record LoginRequest(string Email, string Password);
