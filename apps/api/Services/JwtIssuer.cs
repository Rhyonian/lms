using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lms.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Lms.Api.Services;

public class JwtIssuer
{
    public const string TokenType = "Bearer";
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    private readonly ILogger<JwtIssuer> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly byte[] _signingKey;

    public JwtIssuer(IConfiguration configuration, ILogger<JwtIssuer> logger)
    {
        _logger = logger;
        var secret = configuration["JWT_SECRET"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT secret is not configured. Set the JWT_SECRET environment variable.");
        }

        _signingKey = Encoding.UTF8.GetBytes(secret);
    }

    public JwtToken Issue(User user)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(Lifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role ?? string.Empty)
        };

        if (!string.IsNullOrWhiteSpace(user.GivenName))
        {
            claims.Add(new(ClaimTypes.GivenName, user.GivenName));
        }

        if (!string.IsNullOrWhiteSpace(user.FamilyName))
        {
            claims.Add(new(ClaimTypes.Surname, user.FamilyName));
        }

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_signingKey), SecurityAlgorithms.HmacSha256)
        );

        var accessToken = _tokenHandler.WriteToken(token);
        _logger.LogInformation("Issued JWT for user {UserId} expiring at {ExpiresAt}", user.Id, expiresAt);

        return new JwtToken(accessToken, TokenType, expiresAt);
    }

    public record JwtToken(string AccessToken, string TokenType, DateTime ExpiresAt)
    {
        public int ExpiresIn => (int)Math.Round((ExpiresAt - DateTime.UtcNow).TotalSeconds);
    }
}
