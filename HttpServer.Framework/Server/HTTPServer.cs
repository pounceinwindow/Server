using System.Net;
using System.Text;
using HttpServer.Framework.core.Abstruct;
using HttpServer.Framework.core.handlers;
using HttpServer.Framework.Settings;
using HttpServer.Framework.Utils;

namespace HttpServer.Framework.Server;

public sealed class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly Handler _pipeline;
    private readonly SettingsManager settingsManager = SettingsManager.Instance;

    public HttpServer()
    {
        Handler staticFilesHandler = new StaticFilesHandler();
        Handler endpointsHandler = new EndpointsHandler();
        Handler notFoundHandler = new NotFoundHandler();

        staticFilesHandler.Successor = endpointsHandler;
        endpointsHandler.Successor = notFoundHandler;

        _pipeline = staticFilesHandler;
    }

    public void Start()
    {
        var prefix = $"{settingsManager.Settings.Domain}:{settingsManager.Settings.Port}/";
        _listener.Prefixes.Add(prefix);
        _listener.Start();
        Console.WriteLine($"http://localhost:{settingsManager.Settings.Port}/auth");
        Console.WriteLine("Сервер ожидает...");
        Receive();
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Сервер остановлен...");
    }

    private void Receive()
    {
        try
        {
            _listener.BeginGetContext(ListenerCallback, _listener);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (HttpListenerException)
        {
        }
    }

    private void ListenerCallback(IAsyncResult result)
    {
        if (!_listener.IsListening) return;

        var context = _listener.EndGetContext(result);
        try
        {
            _pipeline.HandleRequest(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SERVER] Unhandled error: " + ex);
            try
            {
                SendTextResponse(context, HttpStatusCode.InternalServerError, "Internal Server Error");
            }
            catch
            {
            }
        }

        if (_listener.IsListening) Receive();
    }

    public static void SendStaticResponse(HttpListenerContext context, HttpStatusCode statusCode, string path)
    {
        var buffer = BufferManager.GetBytesFromFile(path, out var found, out var resolvedPath);

        var finalStatus = !found && statusCode == HttpStatusCode.OK
            ? HttpStatusCode.NotFound
            : statusCode;

        var response = context.Response;
        response.StatusCode = (int)finalStatus;
        response.ContentType = ContentType.GetContentType(resolvedPath);
        response.ContentLength64 = buffer.Length;

        using var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
    }

    public static void SendTextResponse(HttpListenerContext context, HttpStatusCode statusCode, string text,
        string contentType = "text/plain; charset=utf-8")
    {
        var response = context.Response;
        var buffer = Encoding.UTF8.GetBytes(text ?? string.Empty);
        response.StatusCode = (int)statusCode;
        response.ContentType = contentType;
        response.ContentLength64 = buffer.Length;

        using var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
    }
}