using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Enums;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Messages;

namespace RiskTrace.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sessions/{sessionId:guid}/messages")]
public sealed class MessagesController(
    IGetSessionMessagesUseCase getSessionMessagesUseCase,
    ISendMessageUseCase sendMessageUseCase) : ApiControllerBase
{
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<MessageResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<MessageResponse>>>> GetBySessionId(
        Guid sessionId,
        [FromQuery] GetSessionMessagesRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = new PaginationRequest
        {
            Limit = request.PageSize,
            Cursor = request.NextCursor
        };

        var response = await getSessionMessagesUseCase.ExecuteAsync(
            sessionId,
            pagination,
            cancellationToken);

        return ToActionResult(response);
    }

    [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> Create(
        Guid sessionId,
        [FromBody] CreateMessageRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sendMessageUseCase.ExecuteAsync(
            sessionId,
            request.Content,
            MessageRole.USER,
            cancellationToken);

        return ToActionResult(response, StatusCodes.Status201Created);
    }
}
