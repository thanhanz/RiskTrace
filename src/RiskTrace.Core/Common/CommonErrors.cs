using Microsoft.AspNetCore.Http;

namespace RiskTrace.Core.Common;

public static class CommonErrors
{
    public static ErrorResponse BadRequest(string message = "Bad request.")
    {
        return new ErrorResponse(StatusCodes.Status400BadRequest, message);
    }

    public static ErrorResponse Unauthorized(string message = "Unauthorized.")
    {
        return new ErrorResponse(StatusCodes.Status401Unauthorized, message);
    }

    public static ErrorResponse Forbidden(string message = "Forbidden.")
    {
        return new ErrorResponse(StatusCodes.Status403Forbidden, message);
    }

    public static ErrorResponse NotFound(string message = "Not found.")
    {
        return new ErrorResponse(StatusCodes.Status404NotFound, message);
    }

    public static ErrorResponse Conflict(string message = "Conflict.")
    {
        return new ErrorResponse(StatusCodes.Status409Conflict, message);
    }

    public static ErrorResponse FieldRequired(string fieldName)
    {
        return BadRequest($"{fieldName} is required.");
    }
}
