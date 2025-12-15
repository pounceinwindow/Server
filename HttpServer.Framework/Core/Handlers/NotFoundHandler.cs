using System.Net;
using HttpServer.Framework.core.Abstruct;
using static HttpServer.Framework.Server.HttpServer;

namespace HttpServer.Framework.core.handlers;

internal sealed class NotFoundHandler : Handler
{
    public override void HandleRequest(HttpListenerContext context)
    {
        SendStaticResponse(context, HttpStatusCode.NotFound, "404.html");
    }
}