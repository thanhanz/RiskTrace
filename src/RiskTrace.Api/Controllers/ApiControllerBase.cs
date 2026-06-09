using Microsoft.AspNetCore.Mvc;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Constants;

namespace RiskTrace.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> ToActionResult<T>(
        ApiResponse<T> response,
        int successStatusCode = StatusCodes.Status200OK)
    {
        if (response.IsSuccess)
        {
            return StatusCode(successStatusCode, response);
        }

        return StatusCode(MapErrorCodeToStatusCode(response.Error?.Code), response);
    }

    protected IActionResult ToNoContentResult(ApiResponse<object?> response)
    {
        if (response.IsSuccess)
        {
            return NoContent();
        }

        return StatusCode(MapErrorCodeToStatusCode(response.Error?.Code), response);
    }

    private static int MapErrorCodeToStatusCode(string? errorCode)
    {
        return errorCode switch
        {
            AuthErrorCodes.InvalidCredentials => StatusCodes.Status401Unauthorized,
            AuthErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
            AuthErrorCodes.EmailExists => StatusCodes.Status400BadRequest,
            AuthErrorCodes.InvalidRefreshToken => StatusCodes.Status400BadRequest,
            AuthErrorCodes.UserNotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
