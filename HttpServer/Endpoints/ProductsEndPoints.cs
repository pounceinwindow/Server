using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.core.HttpResponse;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Services;

[Endpoint]
public sealed class ProductEndpoint : BaseEndpoint
{
    private readonly TourService _svc = new();

    [HttpGet("/product")]
    public IResponseResult Detail()
    {
        var slug = Context.Request.QueryString["slug"];
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var vm = _svc.BuildProduct(slug);
        return vm == null ? NotFound() : Page("sem/product.html", vm);
    }
}