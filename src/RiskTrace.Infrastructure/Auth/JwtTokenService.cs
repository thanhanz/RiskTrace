using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RiskTrace.Domain.Entities;
using RiskTrace.UseCases.Ports.Auth;

namespace RiskTrace.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenResult GenerateAccessToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.ToString()),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = _tokenHandler.WriteToken(tokenDescriptor);

        return new TokenResult(token, expiresAt);
    }

    public TokenResult GenerateRefreshToken()
    {
        var expiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes);

        return new TokenResult(token, expiresAt);
    }

    public T? GetClaim<T>(string token, string claimType)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            var value = jwtToken?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                return default;

            return typeof(T) switch
            {
                Type t when t == typeof(string) => (T)(object)value,
                Type t when t == typeof(Guid) => (T)(object)Guid.Parse(value),
                Type t when t == typeof(int) => (T)(object)int.Parse(value),
                Type t when t == typeof(long) => (T)(object)long.Parse(value),
                Type t when t == typeof(bool) => (T)(object)bool.Parse(value),
                Type t when t == typeof(DateTime) => (T)(object)DateTime.Parse(value),
                _ => (T)Convert.ChangeType(value, typeof(T))
            };
        }
        catch
        {
            return default;
        }
    }

    public DateTime? GetTokenExpirationUtc(string token)
    {
        try
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwtToken.ValidTo == DateTime.MinValue
                ? null
                : DateTime.SpecifyKind(jwtToken.ValidTo, DateTimeKind.Utc);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
