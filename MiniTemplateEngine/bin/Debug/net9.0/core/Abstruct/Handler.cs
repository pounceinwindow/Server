using System.Net;

namespace CustomHttpServer.Core.Handlers;

internal abstract class Handler
{
    public Handler Successor { get; set; }

    public abstract void HandleRequest(HttpListenerContext condition);
}