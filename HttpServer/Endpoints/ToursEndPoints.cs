using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.core.HttpResponse;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Services;

[Endpoint]
public sealed class ToursEndpoint : BaseEndpoint
{
    private readonly TourService _svc = new();

    [HttpGet("/tours")]
    public IResponseResult List()
    {
        return Page("sem/index.html", _svc.BuildListing(Context.Request.QueryString));
    }

    [HttpGet("/tours/partial")]
    public IResponseResult Partial()
    {
        return Page("sem/partials/grid.html", _svc.BuildListing(Context.Request.QueryString));
    }
}