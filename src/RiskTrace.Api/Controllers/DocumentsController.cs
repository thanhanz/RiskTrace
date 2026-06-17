using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Request.Documents;
using RiskTrace.Domain.Response.Documents;
using RiskTrace.UseCases.Interfaces.Documents;

namespace RiskTrace.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sessions/{sessionId:guid}/documents")]
public sealed class DocumentsController(
    IInitiateDocumentUploadUseCase initiateDocumentUploadUseCase,
    ICompleteDocumentUploadUseCase completeDocumentUploadUseCase) : ApiControllerBase
{
    [ProducesResponseType(typeof(ApiResponse<DocumentUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Consumes("multipart/form-data")]
    
    [HttpPost("upload/initiate")]
    public async Task<ActionResult<ApiResponse<DocumentUploadResponse>>> InitiateUpload(
        Guid sessionId,
        [FromForm] InitiateDocumentUploadRequest request,
        CancellationToken cancellationToken)
    {
        var response = await initiateDocumentUploadUseCase.ExecuteAsync(
            sessionId,
            request,
            cancellationToken);

        return ToActionResult(response);
    }

    [ProducesResponseType(typeof(ApiResponse<DocumentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    
    [HttpPost("upload/complete")]
    public async Task<ActionResult<ApiResponse<DocumentResponse>>> CompleteUpload(
        Guid sessionId,
        [FromBody] CompleteDocumentUploadRequest request,
        CancellationToken cancellationToken)
    {
        var response = await completeDocumentUploadUseCase.ExecuteAsync(
            sessionId,
            request,
            cancellationToken);

        return ToActionResult(response, StatusCodes.Status201Created);
    }
}
