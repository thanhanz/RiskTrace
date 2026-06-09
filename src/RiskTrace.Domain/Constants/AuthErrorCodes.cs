namespace RiskTrace.Domain.Constants;

public static class AuthErrorCodes
{
    public const string EmailExists = "auth.email_exists";
    public const string InvalidCredentials = "auth.invalid_credentials";
    public const string InvalidRefreshToken = "auth.invalid_refresh_token";
    public const string Unauthorized = "auth.unauthorized";
    public const string UserNotFound = "auth.user_not_found";
}
