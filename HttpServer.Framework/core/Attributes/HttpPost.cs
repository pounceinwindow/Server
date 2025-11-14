namespace HttpServer.Framework.core.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HttpPost : Attribute
{
    public HttpPost()
    {
    }

    public HttpPost(string? route)
    {
        Route = route;
    }

    public string? Route { get; }
}