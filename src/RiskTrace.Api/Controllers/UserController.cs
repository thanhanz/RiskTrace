using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Auth;

namespace RiskTrace.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class UserController(
    IMyInfoUseCase myInfoUseCase) : ApiControllerBase
{

    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserInfoResponse>>> MyInfo(CancellationToken cancellationToken)
    {
        var result = await myInfoUseCase.ExecuteAsync(cancellationToken);
        return ToActionResult(result);
    }
}
