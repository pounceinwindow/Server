using System.Text.RegularExpressions;
using HttpServer.Framework.Settings;
using MyORM;

namespace HttpServer.Services;


public static class AuthService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static bool Validate(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return false;

        if (!EmailRegex.IsMatch(email))
            return false;

        var connectionString = SettingsManager.Instance.Settings.ConnectionString;
        using var db = new OrmContext(connectionString);

        var user = db.FirstOrDefault<UserModel>(u => u.Email == email && u.Password == password, "users");

        return user != null;
    }
}