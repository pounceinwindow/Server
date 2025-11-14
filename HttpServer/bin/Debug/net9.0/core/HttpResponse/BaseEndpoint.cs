using System.Net;

namespace HttpServer.Framework.Core.HttpResponse;

public abstract class BaseEndpoint
{
    protected HttpListenerContext Context { get; private set; }

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected IResponseResult Page(string pathTemplate, object data)
    {
        return new PageResult(pathTemplate, data);
    }
}