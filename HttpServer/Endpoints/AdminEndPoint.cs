using System.Globalization;
using System.Net;
using System.Text;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Services;

[Endpoint]
public sealed class AdminEndPoint : BaseEndpoint
{
    private bool IsAuth() => Context.Request.Cookies?["auth"]?.Value == "1";

    [HttpGet("/admin")]
    public IResponseResult Index()
    {
        if (!IsAuth()) return Redirect("/auth");
        try
        {
            var vm = new { Items = new TourService().All() };
            return Page("admin/index.html", vm);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ADMIN] Index error: " + ex);
            return Html("<h1>Admin error</h1><p>See server log.</p>", statusCode:500);
        }
    }

    [HttpPost("/admin/tours/create")]
    public void Create()
    {
        if (!IsAuth()) { RedirectNow("/auth"); return; }

        try
        {
            var form = ParseForm(Body());
            var title = form.GetValueOrDefault("title","").Trim();
            var city  = form.GetValueOrDefault("city","").Trim();
            var cat   = form.GetValueOrDefault("category","Activities").Trim();
            var priceText = form.GetValueOrDefault("price","0");
            var desc = form.GetValueOrDefault("description","").Trim();
            var hero = form.GetValueOrDefault("hero_url","").Trim();
            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)
                && !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(city))
            {
                new TourService().Create(title, city, cat, price, hero, desc);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ADMIN] Create error: " + ex);
        }

        Context.Response.KeepAlive = false;
        RedirectNow("/admin");
    }

    [HttpPost("/admin/tours/delete")]
    public void Delete()
    {
        if (!IsAuth()) { RedirectNow("/auth"); return; }

        try
        {
            var form = ParseForm(Body());
            var slug = form.GetValueOrDefault("slug","").Trim();
            var idStr = form.GetValueOrDefault("id","").Trim();

            // на всякий случай поддержим ?slug=... или ?id=...
            if (string.IsNullOrEmpty(slug) && string.IsNullOrEmpty(idStr) && Context?.Request?.Url is not null)
            {
                var qs = System.Web.HttpUtility.ParseQueryString(Context.Request.Url.Query);
                slug = qs.Get("slug") ?? "";
                idStr = qs.Get("id") ?? "";
            }

            var svc = new TourService();
            if (!string.IsNullOrEmpty(slug)) svc.DeleteBySlug(slug);
            else if (int.TryParse(idStr, out var id) && id > 0) svc.Delete(id);
            else Console.WriteLine("[ADMIN] Delete: no slug/id");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ADMIN] Delete error: " + ex);
        }

        Context.Response.KeepAlive = false;
        RedirectNow("/admin");
    }

    [HttpGet("/admin/logout")]
    public void Logout()
    {
        Context.Response.Cookies.Add(new Cookie("auth","x"){ Expires = DateTime.UtcNow.AddDays(-1), Path="/" });
        Context.Response.KeepAlive = false;
        RedirectNow("/auth");
    }

    private string Body()
    {
        using var sr = new StreamReader(Context.Request.InputStream, Encoding.UTF8, leaveOpen:false);
        return sr.ReadToEnd();
    }

    private static Dictionary<string,string> ParseForm(string body)
    {
        var d = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(body)) return d;
        foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            var k = Uri.UnescapeDataString((kv[0] ?? "").Replace('+',' '));
            var v = kv.Length > 1 ? Uri.UnescapeDataString(kv[1].Replace('+',' ')) : "";
            d[k] = v;
        }
        return d;
    }

    private sealed class RedirectResult : IResponseResult
    {
        private readonly string _url;
        public RedirectResult(string url) => _url = url;
        public void Execute(HttpListenerContext c)
        {
            c.Response.StatusCode = 302;
            c.Response.RedirectLocation = _url;
            c.Response.OutputStream.Close();
        }
    }
    private IResponseResult Redirect(string url) => new RedirectResult(url);

    private void RedirectNow(string url)
    {
        Context.Response.StatusCode = 302;
        Context.Response.RedirectLocation = url;
        Context.Response.OutputStream.Close();
    }

    private IResponseResult Html(string html, int statusCode = 200, string contentType = "text/html; charset=utf-8")
        => new RawHtmlResult(html, statusCode, contentType);

    private sealed class RawHtmlResult : IResponseResult
    {
        private readonly string _html; private readonly int _code; private readonly string _ct;
        public RawHtmlResult(string html, int code, string ct){ _html=html; _code=code; _ct=ct; }
        public void Execute(HttpListenerContext c)
        {
            var b = Encoding.UTF8.GetBytes(_html);
            c.Response.StatusCode = _code;
            c.Response.ContentType = _ct;
            c.Response.ContentLength64 = b.Length;
            using var o = c.Response.OutputStream;
            o.Write(b, 0, b.Length);
        }
    }
}
