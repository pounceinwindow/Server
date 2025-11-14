// добавь это:
using System.Net;
using System.Reflection;
using CustomHttpServer.Core.Handlers;
using HttpServer.Core.Attributes;
using static HttpServerApp.HttpServer;

internal class EndpointsHandler : Handler
{
    public override void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var path = Normalize(request.Url?.LocalPath);
        var httpAttrName = $"Http{request.HttpMethod}";

        var assembly = Assembly.GetExecutingAssembly();
        var endpointTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
            .ToList();

        foreach (var type in endpointTypes)
        foreach (var method in type.GetMethods())
        {
            var attr = method.GetCustomAttributes(true)
                .FirstOrDefault(a => string.Equals(a.GetType().Name, httpAttrName, StringComparison.OrdinalIgnoreCase));
            if (attr is null) continue;

            var route = attr.GetType().GetProperty("Route")?.GetValue(attr) as string;
            if (!string.IsNullOrWhiteSpace(route) && Normalize(route!) == path)
            {
                InvokeAndWrite(context, type, method);
                return;
            }
        }

        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var slug = parts.Length > 0 ? parts[0] : string.Empty;
        var tail = parts.Length > 1 ? string.Join('/', parts.Skip(1)) : string.Empty;

        var endpointType = endpointTypes.FirstOrDefault(t => IsMatch(t.Name, slug));
        if (endpointType == null)
        {
            Successor?.HandleRequest(context);
            return;
        }

        var methods = endpointType.GetMethods()
            .Where(m => m.GetCustomAttributes(true)
                .Any(a => string.Equals(a.GetType().Name, httpAttrName, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (methods.Count == 0)
        {
            Successor?.HandleRequest(context);
            return;
        }

        var chosen = methods.FirstOrDefault(m =>
        {
            var a = m.GetCustomAttributes(true)
                .First(x => string.Equals(x.GetType().Name, httpAttrName, StringComparison.OrdinalIgnoreCase));
            var route = (a.GetType().GetProperty("Route")?.GetValue(a) as string)?.Trim('/') ?? string.Empty;
            return string.IsNullOrEmpty(route)
                ? string.IsNullOrEmpty(tail)
                : string.Equals(route, tail, StringComparison.OrdinalIgnoreCase);
        }) ?? methods.First();

        InvokeAndWrite(context, endpointType, chosen);
    }

    private static void InvokeAndWrite(HttpListenerContext context, Type endpointType, MethodInfo method)
    {
        var instance = Activator.CreateInstance(endpointType);

        try
        {
            object? result;
            var ps = method.GetParameters();

            if (ps.Length == 1 && ps[0].ParameterType == typeof(HttpListenerContext))
                result = method.Invoke(instance, new object[] { context });
            else
                result = method.Invoke(instance, null);

            if (result is Task t)
            {
                t.GetAwaiter().GetResult();
                var tt = t.GetType();
                if (tt.IsGenericType)
                    result = tt.GetProperty("Result")?.GetValue(t);
                else
                    result = null;
            }

            if (result is string s)
            {
                var path = s.StartsWith("/") ? s.TrimStart('/') : s;
                SendStaticResponse(context, HttpStatusCode.OK, path);
            }
        }
        catch
        {
            context.Response.StatusCode = 500;
        }
    }

    private static string Normalize(string? p)
    {
        var s = string.IsNullOrEmpty(p) ? "/" : p!;
        if (!s.StartsWith("/")) s = "/" + s;
        if (s.Length > 1 && s.EndsWith("/")) s = s[..^1];
        return s;
    }

    private static bool IsMatch(string typeName, string slug)
    {
        var baseName = typeName.EndsWith("Endpoint", StringComparison.OrdinalIgnoreCase)
            ? typeName[..^"Endpoint".Length]
            : typeName;
        return slug.Equals(baseName, StringComparison.OrdinalIgnoreCase) ||
               slug.Equals(typeName, StringComparison.OrdinalIgnoreCase);
    }
}