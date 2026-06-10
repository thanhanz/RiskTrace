using Microsoft.Extensions.Options;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;
using RiskTrace.Infrastructure.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RiskTrace.Api.Middleware;

public sealed class TokenBlacklistMiddleware(
    RequestDelegate next,
    IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task InvokeAsync(HttpContext context, ITokenBlackList tokenBlackList)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        //Find by JTI (JWT ID) claim, which is a unique identifier for the token
        var jti = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (string.IsNullOrWhiteSpace(jti))
        {
            await next(context);
            return;
        }

        if (await tokenBlackList.ExistsAsync(jti, context.RequestAborted))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(
                new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthenticated"),
                context.RequestAborted);
            return;
        }

        await next(context);
    }
}
