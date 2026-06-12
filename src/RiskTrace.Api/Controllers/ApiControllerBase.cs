using Microsoft.AspNetCore.Mvc;
using RiskTrace.Core.Common;

namespace RiskTrace.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    //<summary>
    //
    // Converts an ApiResponse to an ActionResult, returning the appropriate status code and response body
    //
    // </summary>
    protected ActionResult<ApiResponse<T>> ToActionResult<T>(ApiResponse<T> response, int successStatusCode = StatusCodes.Status200OK)
    {
        if (response.IsSuccess)
        {
            return StatusCode(successStatusCode, response);
        }

        var statusCode = response.Error?.Code ?? StatusCodes.Status400BadRequest;
        return StatusCode(statusCode, new ErrorResponse(
            statusCode,
            response.Error?.Message ?? "Request failed."));
    }

    //For DELETE or requests that don't return data
    protected IActionResult ToNoContentResult(ApiResponse<object?> response)
    {
        if (response.IsSuccess)
        {
            return NoContent();
        }

        var statusCode = response.Error?.Code ?? StatusCodes.Status400BadRequest;
        return StatusCode(statusCode, new ErrorResponse(
            statusCode,
            response.Error?.Message ?? "Request failed."));
    }
}
