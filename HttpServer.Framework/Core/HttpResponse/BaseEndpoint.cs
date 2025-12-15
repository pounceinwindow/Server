using System.Net;
using System.Text;
using System.Text.Json;
using HttpServer.Framework.Core.HttpResponse;

namespace HttpServer.Framework.core.HttpResponse;

public abstract class BaseEndpoint
{
    protected HttpListenerContext Context { get; private set; } = default!;

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }


    protected IResponseResult Page(string pathTemplate, object? data = null)
    {
        return new PageResult(pathTemplate, data ?? new { });
    }

    protected IResponseResult NotFound(string path = "404.html")
    {
        return new StaticFileResult(path, HttpStatusCode.NotFound);
    }


    protected IResponseResult Redirect(string url, int statusCode = 302)
    {
        return new RedirectResult(url, statusCode);
    }

    protected void RedirectNow(string url, int statusCode = 302)
    {
        var resp = Context.Response;
        resp.StatusCode = statusCode;
        resp.RedirectLocation = url;
        resp.OutputStream.Close();
    }


    protected IResponseResult Html(string html, int statusCode = 200, string contentType = "text/html; charset=utf-8")
    {
        return new TextResult(html, statusCode, contentType);
    }

    protected IResponseResult Text(string text, int statusCode = 200, string contentType = "text/plain; charset=utf-8")
    {
        return new TextResult(text, statusCode, contentType);
    }

    protected IResponseResult Json(object? data, int statusCode = 200)
    {
        return new JsonResult(data, statusCode);
    }


    protected string Body()
    {
        using var sr = new StreamReader(Context.Request.InputStream, Encoding.UTF8, leaveOpen: false);
        return sr.ReadToEnd();
    }

    protected Dictionary<string, string> Form()
    {
        return ParseForm(Body());
    }

    protected static Dictionary<string, string> ParseForm(string body)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(body)) return d;

        foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            var k = Uri.UnescapeDataString((kv[0] ?? "").Replace('+', ' '));
            var v = kv.Length > 1 ? Uri.UnescapeDataString(kv[1].Replace('+', ' ')) : "";
            d[k] = v;
        }

        return d;
    }


    private sealed class RedirectResult : IResponseResult
    {
        private readonly int _code;
        private readonly string _url;

        public RedirectResult(string url, int code)
        {
            _url = url;
            _code = code;
        }

        public void Execute(HttpListenerContext c)
        {
            c.Response.StatusCode = _code;
            c.Response.RedirectLocation = _url;
            c.Response.OutputStream.Close();
        }
    }

    private sealed class TextResult : IResponseResult
    {
        private readonly int _code;
        private readonly string _ct;
        private readonly string _text;

        public TextResult(string text, int code, string ct)
        {
            _text = text ?? "";
            _code = code;
            _ct = ct;
        }

        public void Execute(HttpListenerContext c)
        {
            var b = Encoding.UTF8.GetBytes(_text);
            c.Response.StatusCode = _code;
            c.Response.ContentType = _ct;
            c.Response.ContentLength64 = b.Length;
            using var o = c.Response.OutputStream;
            o.Write(b, 0, b.Length);
        }
    }

    private sealed class JsonResult : IResponseResult
    {
        private readonly int _code;
        private readonly object? _data;

        public JsonResult(object? data, int code)
        {
            _data = data;
            _code = code;
        }

        public void Execute(HttpListenerContext c)
        {
            var json = JsonSerializer.Serialize(_data);
            var b = Encoding.UTF8.GetBytes(json);
            c.Response.StatusCode = _code;
            c.Response.ContentType = "application/json; charset=utf-8";
            c.Response.ContentLength64 = b.Length;
            using var o = c.Response.OutputStream;
            o.Write(b, 0, b.Length);
        }
    }

    private sealed class StaticFileResult : IResponseResult
    {
        private readonly HttpStatusCode _code;
        private readonly string _path;

        public StaticFileResult(string path, HttpStatusCode code)
        {
            _path = path;
            _code = code;
        }

        public void Execute(HttpListenerContext c)
        {
            Server.HttpServer.SendStaticResponse(c, _code, _path);
        }
    }
}