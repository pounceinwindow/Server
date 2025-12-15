namespace HttpServer.Framework.core.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HttpGet : Attribute
{
    public HttpGet()
    {
    }

    public HttpGet(string? route)
    {
        Route = route;
    }

    public string? Route { get; }
}