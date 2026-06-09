using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;
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
    IOptions<JwtOptions> jwtOptions) : ApiControllerBase
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await registerUseCase.ExecuteAsync(request, cancellationToken);

        if (response.IsSuccess && response.Data is not null)
        {
            SetAccessTokenCookie(response.Data.AccessToken);
        }

        return ToActionResult(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await loginUseCase.ExecuteAsync(request, cancellationToken);

        if (response.IsSuccess && response.Data is not null)
        {
            SetAccessTokenCookie(response.Data.AccessToken);
        }

        return ToActionResult(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await refreshTokenUseCase.ExecuteAsync(request, cancellationToken);

        if (response.IsSuccess && response.Data is not null)
        {
            SetAccessTokenCookie(response.Data.AccessToken);
        }

        return ToActionResult(response);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var accessToken = string.IsNullOrEmpty(request.AccessToken) ? 
                          AccessTokenReader.ReadToken(Request, _jwtOptions.AccessTokenCookieName) :
                          request.AccessToken;

        var response = await logoutUseCase.ExecuteAsync(
            new LogoutRequest(request.RefreshToken, accessToken),
            cancellationToken);

        if (response.IsSuccess)
        {
            ClearAccessTokenCookie();
        }

        return ToNoContentResult(response);
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
