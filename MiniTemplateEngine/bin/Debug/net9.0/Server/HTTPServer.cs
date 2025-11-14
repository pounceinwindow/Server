using System.Net;
using CustomHttpServer.Core.Handlers;
using MiniHttpServer.Core.Handlers;
using ContentType = HttpServer.Shared.ContentType;

namespace HttpServerApp;

public sealed class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly SettingsManager settingsManager = SettingsManager.Instance;

    public void Start()
    {
        var prefix = $"{settingsManager.Settings.Domain}:{settingsManager.Settings.Port}/";
        _listener.Prefixes.Add(prefix);
        _listener.Start();
        Console.WriteLine($"{prefix}bonx");
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

    private async void ListenerCallback(IAsyncResult result)
    {
        if (_listener.IsListening)
        {
            var context = _listener.EndGetContext(result);
            Handler staticFilesHandler = new StaticFilesHandler();
            Handler endpointsHandler = new EndpointsHandler();
            staticFilesHandler.Successor = endpointsHandler;
            staticFilesHandler.HandleRequest(context);

            if (_listener.IsListening)
                Receive();
        }
    }

    public static void SendStaticResponse(HttpListenerContext context, HttpStatusCode statusCode, string path)
    {
        var response = context.Response;
        var request = context.Request;

        response.StatusCode = (int)statusCode;
        response.ContentType = ContentType.GetContentType(path);

        var buffer = BufferManager.GetBytesFromFile(path);
        response.ContentLength64 = buffer.Length;

        using var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);


        if (response.StatusCode == 200)
            Console.WriteLine(
                $"Запрос обработан: {request.Url.AbsolutePath} {request.HttpMethod} - Status: {response.StatusCode}");
        else
            Console.WriteLine(
                $"Ошибка запроса: {request.Url.AbsolutePath} {request.HttpMethod} - Status: {response.StatusCode}");
    }
}