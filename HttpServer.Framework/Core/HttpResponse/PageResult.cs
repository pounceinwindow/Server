using System.Net;
using System.Text;
using HttpServer.Framework.Settings;
using MiniTemplateEngine;

namespace HttpServer.Framework.Core.HttpResponse;

internal class PageResult : IResponseResult
{
    private readonly object _data;
    private readonly string _pathTemplate;

    public PageResult(string pathTemplate, object data)
    {
        _pathTemplate = pathTemplate;
        _data = data;
    }

    public void Execute(HttpListenerContext context)
    {
        var root = SettingsManager.Instance.Settings.StaticDirectoryPath!;
        var input = Path.Combine(root, _pathTemplate.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var html = new HtmlTemplateRenderer().RenderFromFile(input, _data);
        var bytes = Encoding.UTF8.GetBytes(html);

        var resp = context.Response;
        resp.StatusCode = 200;
        resp.ContentType = "text/html; charset=utf-8";
        resp.ContentLength64 = bytes.Length;
        using var s = resp.OutputStream;
        s.Write(bytes, 0, bytes.Length);
    }
}