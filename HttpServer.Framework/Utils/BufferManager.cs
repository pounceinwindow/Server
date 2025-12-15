using HttpServer.Framework.Settings;

namespace HttpServer.Framework.Utils;

public static class BufferManager
{
    public static byte[] GetBytesFromFile(string path)
    {
        return GetBytesFromFile(path, out _, out _);
    }

    public static byte[] GetBytesFromFile(string path, out bool found, out string resolvedPath)
    {
        var normalized = path;
        if (!Path.HasExtension(normalized))
            normalized = normalized.TrimEnd('/') + "/index.html";

        resolvedPath = ResolveFilePath(normalized, out found);
        return File.ReadAllBytes(resolvedPath);
    }

    public static string ResolveFilePath(string path, out bool found)
    {
        found = false;
        try
        {
            var targetPath = Path.Combine(path.Split("/"));

            var root = SettingsManager.Instance.Settings.StaticDirectoryPath!;
            var file = Directory.EnumerateFiles(root, $"{Path.GetFileName(path)}", SearchOption.AllDirectories)
                .FirstOrDefault(f => f.EndsWith(targetPath, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(file))
            {
                found = true;
                return file;
            }
        }
        catch
        {
        }

        return Path.Combine(SettingsManager.Instance.Settings.StaticDirectoryPath!, "404.html");
    }
}