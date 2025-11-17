using System.Net;
using System.Text;
using HttpServer.Framework.core.Attributes;
using HttpServer.Services;

namespace HttpServer.Endpoints;

[Endpoint]
internal sealed class AuthEndPoints
{
    [HttpGet("/auth")]
    public string LoginPage()
    {
        return "auth/login.html";
    }

    [HttpPost("/auth/login")]
    public Task Login(HttpListenerContext ctx)
    {
        return HandleLogin(ctx);
    }

    [HttpPost("/auth/sendEmail")]
    public Task LegacyLogin(HttpListenerContext ctx)
    {
        return HandleLogin(ctx);
    }

    private static async Task HandleLogin(HttpListenerContext ctx)
    {
        var form = await ReadForm(ctx.Request);
        var email = form.TryGetValue("email", out var e) ? e.Trim() : "";
        var password = form.TryGetValue("password", out var p) ? p.Trim() : "";

        if (!AuthService.Validate(email, password))
        {
            await Write(ctx, "email and password are required", 400);
            return;
        }

        try
        {
            await EmailService.SendAsync(email, "Login", $"User: {WebUtility.HtmlEncode(email)}");
        }
        catch
        {
        }

        ctx.Response.Cookies.Add(new Cookie("auth", "1")
        {
            Path = "/",
            HttpOnly = true
        });
        ctx.Response.StatusCode = 302;
        ctx.Response.RedirectLocation = "/admin";
        ctx.Response.OutputStream.Close();
    }

    private static async Task<Dictionary<string, string>> ReadForm(HttpListenerRequest req)
    {
        using var sr = new StreamReader(req.InputStream, Encoding.UTF8, leaveOpen: false);
        var body = await sr.ReadToEndAsync();
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(body)) return dict;
        foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            var k = Uri.UnescapeDataString((kv[0] ?? "").Replace('+', ' '));
            var v = kv.Length > 1 ? Uri.UnescapeDataString(kv[1].Replace('+', ' ')) : "";
            dict[k] = v;
        }

        return dict;
    }

    private static async Task Write(HttpListenerContext c, string s, int status)
    {
        var b = Encoding.UTF8.GetBytes(s);
        c.Response.StatusCode = status;
        c.Response.ContentType = "text/plain; charset=utf-8";
        c.Response.ContentLength64 = b.Length;
        await using var o = c.Response.OutputStream;
        await o.WriteAsync(b, 0, b.Length);
        await o.FlushAsync();
        o.Close();
    }
}