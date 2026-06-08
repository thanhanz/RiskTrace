using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Auth;

namespace RiskTrace.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class UserController(
    IMyInfoUseCase myInfoUseCase) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<UserInfoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfoResponse>> Me(CancellationToken cancellationToken)
    {
        var result = await myInfoUseCase.ExecuteAsync(cancellationToken);

        if (!result.IsAuthenticated)
        {
            return Unauthorized();
        }

        if (result.User is null)
        {
            return NotFound();
        }

        return Ok(result.User);
    }
}
