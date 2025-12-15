using System.Net;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.core.HttpResponse;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Framework.Settings;
using HttpServer.Services;
using MyORM;

namespace HttpServer.Endpoints;

[Endpoint]
internal sealed class AuthEndPoints : BaseEndpoint
{
    [HttpGet("/auth")]
    public IResponseResult LoginPage()
    {
        return Page("auth/login.html", new { });
    }

    [HttpPost("/auth/login")]
    public Task<IResponseResult> Login()
    {
        return HandleLogin();
    }

    [HttpPost("/auth/sendEmail")]
    public Task<IResponseResult> LegacyLogin()
    {
        return HandleLogin();
    }

    private async Task<IResponseResult> HandleLogin()
    {
        var form = Form();

        static string Get(Dictionary<string, string> f, string key)
        {
            return f.TryGetValue(key, out var v) ? v ?? "" : "";
        }

        var email = Get(form, "email").Trim();
        var password = Get(form, "password").Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Text("email and password are required", 400);

        var isAdmin = email.Equals("admin@test.com", StringComparison.OrdinalIgnoreCase)
                      && password == "admin";

        if (isAdmin)
        {
            Context.Response.Cookies.Add(new Cookie("auth", "1")
            {
                Path = "/",
                HttpOnly = true
            });

            return Redirect("/admin");
        }

        var cs = SettingsManager.Instance.Settings.ConnectionString!;
        using var db = new OrmContext(cs);

        var user = db.FirstOrDefault<UserModel>(u => u.Email == email, "users");
        var isNewUser = false;

        if (user == null)
        {
            user = new UserModel { Email = email, Password = password };
            db.Create(user, "users");
            isNewUser = true;
        }
        else
        {
            if (user.Password != password)
                return Text("invalid password", 400);
        }

        return Redirect("/tours");
    }
}