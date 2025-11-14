namespace HttpServer.Framework.Utils;

public static class ContentType
{
    private static readonly Dictionary<string, string> FileTypes = new()
    {
        { ".html", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".webp", "image/webp" },
        { ".ico", "image/x-icon" },
        { ".svg", "image/svg+xml" },
        { ".json", "application/json" },
        { ".woff2", "font/woff2" }
    };

    public static string GetContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLower();

        return FileTypes[extension];
    }
}