using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RiskTrace.UseCases.Ports.Auth;

namespace RiskTrace.Infrastructure.Auth;

public sealed class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            var userIdValue = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(userIdValue, out var userId) ? userId : null;
        }
    }

    public string? Email
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.Email)
                ?? user?.FindFirstValue(JwtRegisteredClaimNames.Email);
        }
    }
}
