namespace RiskTrace.Core.Common;

public sealed record ApiResponse<T>(
    bool IsSuccess,
    T? Data,
    ErrorResponse? Error)
{
    public static ApiResponse<T> Success(T? data)
    {
        return new ApiResponse<T>(
            IsSuccess: true,
            Data: data,
            Error: null);
    }

    public static ApiResponse<T> Failure(int code, string message)
    {
        return new ApiResponse<T>(
            IsSuccess: false,
            Data: default,
            Error: new ErrorResponse(code, message));
    }
}
