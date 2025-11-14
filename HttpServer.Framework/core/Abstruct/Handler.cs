using System.Net;

namespace HttpServer.Framework.core.Abstruct;

internal abstract class Handler
{
    public Handler Successor { get; set; }

    public abstract void HandleRequest(HttpListenerContext condition);
}