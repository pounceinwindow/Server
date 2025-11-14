using System.Net;
using CustomHttpServer.Core.Handlers;
using static HttpServerApp.HttpServer;

namespace MiniHttpServer.Core.Handlers;

internal class StaticFilesHandler : Handler
{
    public override void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var isGetMethod = request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
        var isStaticFile = request.Url.AbsolutePath.Split('/').Any(x => x.Contains("."));

        if (isGetMethod && isStaticFile)
        {
            var path = request.Url.AbsolutePath.Trim('/');

            SendStaticResponse(context, HttpStatusCode.OK, path);
        }
        else if (Successor != null)
        {
            Successor.HandleRequest(context);
        }
    }
}