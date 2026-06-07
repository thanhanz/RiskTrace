using Microsoft.AspNetCore.Http;

namespace RiskTrace.Infrastructure.Auth;

public static class AccessTokenReader
{
    public static string? ReadToken(HttpRequest request, string cookieName)
    {
        if (request.Cookies.TryGetValue(cookieName, out var cookieToken)
            && !string.IsNullOrWhiteSpace(cookieToken))
        {
            return cookieToken;
        }

        var authorizationHeader = request.Headers["Authorization"].ToString();
        if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var bearerToken = authorizationHeader["Bearer ".Length..].Trim();
            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                return bearerToken;
            }
        }

        return null;
    }
}
