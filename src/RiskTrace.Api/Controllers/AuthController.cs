using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RiskTrace.Domain.Request;
using RiskTrace.Infrastructure.Auth;
using RiskTrace.UseCases.Interfaces.Auth;

namespace RiskTrace.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class AuthController(
    IRegisterUseCase registerUseCase,
    ILoginUseCase loginUseCase,
    IRefreshTokenUseCase refreshTokenUseCase,
    ILogoutUseCase logoutUseCase,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await registerUseCase.ExecuteAsync(request, cancellationToken);

        SetAccessTokenCookie(response.AccessToken);

        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await loginUseCase.ExecuteAsync(request, cancellationToken);

        SetAccessTokenCookie(response.AccessToken);

        return Ok(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await refreshTokenUseCase.ExecuteAsync(request, cancellationToken);

        SetAccessTokenCookie(response.AccessToken);

        return Ok(response);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await logoutUseCase.ExecuteAsync(request, cancellationToken);

        ClearAccessTokenCookie();

        return NoContent();
    }

    private void SetAccessTokenCookie(string accessToken)
    {
        Response.Cookies.Append(
            _jwtOptions.AccessTokenCookieName,
            accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes)
            });
    }

    private void ClearAccessTokenCookie()
    {
        Response.Cookies.Delete(
            _jwtOptions.AccessTokenCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
    }
}
