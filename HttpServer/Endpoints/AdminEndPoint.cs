using System.Globalization;
using System.Net;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.core.HttpResponse;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Services;

[Endpoint]
public sealed class AdminEndPoint : BaseEndpoint
{
    private readonly TourService _svc = new();

    [HttpGet("/admin")]
    public IResponseResult Index()
    {
        if (!IsAuth())
            return Redirect("/auth");

        var vm = new { Items = _svc.All() };
        return Page("admin/index.html", vm);
    }

    [HttpPost("/admin/tours/create")]
    public IResponseResult Create()
    {
        if (!IsAuth())
            return Redirect("/auth");

        try
        {
            var form = Form();

            var title = form.GetValueOrDefault("title", "").Trim();
            var city = form.GetValueOrDefault("city", "").Trim();
            var cat = form.GetValueOrDefault("category", "ACTIVITIES").Trim();
            var priceText = form.GetValueOrDefault("price", "0").Trim();
            var desc = form.GetValueOrDefault("description", "").Trim();
            var hero = form.GetValueOrDefault("hero_url", "").Trim();

            if (!string.IsNullOrWhiteSpace(title) &&
                !string.IsNullOrWhiteSpace(city) &&
                decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                _svc.Create(title, city, cat, price, hero, desc);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ADMIN] Create error: " + ex);
        }

        return Redirect("/admin");
    }

    [HttpPost("/admin/tours/delete")]
    public IResponseResult Delete()
    {
        if (!IsAuth())
            return Redirect("/auth");

        try
        {
            var form = Form();
            var slug = form.GetValueOrDefault("slug", "").Trim();
            var idStr = form.GetValueOrDefault("id", "").Trim();

            if (!string.IsNullOrEmpty(slug))
                _svc.DeleteBySlug(slug);
            else if (int.TryParse(idStr, out var id) && id > 0)
                _svc.Delete(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ADMIN] Delete error: " + ex);
        }

        return Redirect("/admin");
    }

    [HttpGet("/admin/logout")]
    public IResponseResult Logout()
    {
        Context.Response.Cookies.Add(new Cookie("auth", "")
        {
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1),
            HttpOnly = true
        });

        return Redirect("/auth");
    }

    private bool IsAuth()
    {
        try
        {
            var cookies = Context?.Request?.Cookies;
            var authCookie = cookies?["auth"];
            return authCookie != null && authCookie.Value == "1";
        }
        catch
        {
            return false;
        }
    }
}