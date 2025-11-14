using System.Net;
using HttpServer.Framework.core.Abstruct;
using static HttpServer.Framework.Server.HttpServer;

namespace HttpServer.Framework.core.handlers;

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