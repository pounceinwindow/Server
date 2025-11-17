namespace HttpServer.Services;

public static class AuthService
{
    public static bool Validate(string email, string password)
    {
        return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
    }
}