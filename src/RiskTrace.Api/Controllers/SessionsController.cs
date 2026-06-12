using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Sessions;

namespace RiskTrace.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sessions")]
public sealed class SessionsController(
    ICreateSessionUseCase createSessionUseCase,
    IGetUserSessionsUseCase getUserSessionsUseCase,
    IGetSessionDetailUseCase getSessionDetailUseCase,
    IRenameSessionUseCase renameSessionUseCase,
    IDeleteSessionUseCase deleteSessionUseCase) : ApiControllerBase
{
    [ProducesResponseType(typeof(ApiResponse<SessionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> Create(
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createSessionUseCase.ExecuteAsync(request, cancellationToken);
        return ToActionResult(response, StatusCodes.Status201Created);
    }

    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SessionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SessionResponse>>>> GetUserSessions(
        CancellationToken cancellationToken)
    {
        var response = await getUserSessionsUseCase.ExecuteAsync(cancellationToken);
        return ToActionResult(response);
    }

    [ProducesResponseType(typeof(ApiResponse<SessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> GetDetail(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await getSessionDetailUseCase.ExecuteAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    [ProducesResponseType(typeof(ApiResponse<SessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> Rename(
        Guid id,
        [FromBody] RenameSessionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await renameSessionUseCase.ExecuteAsync(id, request, cancellationToken);
        return ToActionResult(response);
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await deleteSessionUseCase.ExecuteAsync(id, cancellationToken);
        return ToNoContentResult(response);
    }
}
