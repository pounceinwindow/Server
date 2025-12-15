using System.Net;

namespace HttpServer.Framework.Core.HttpResponse;

public interface IResponseResult
{
    void Execute(HttpListenerContext context);
}