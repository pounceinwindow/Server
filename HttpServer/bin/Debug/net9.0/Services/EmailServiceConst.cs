using System.Text.Json;

namespace HttpServer.Services;

public sealed class EmailConfig
{
    private static readonly Lazy<EmailConfig> _instance = new(() => Load());
    public static EmailConfig Instance => _instance.Value;

    public string SmtpHost { get; init; }
    public int SmtpPort { get; init; }
    public string SmtpUser { get; init; }
    public string SmtpPass { get; init; }
    public string FromAddr { get; init; }
    public string FromName { get; init; }

    private static EmailConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "email.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Не найден файл конфигурации почты: {path}");

        var cfg = JsonSerializer.Deserialize<EmailConfig>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (cfg is null) throw new InvalidOperationException("email.json повреждён");
        if (string.IsNullOrWhiteSpace(cfg.SmtpUser)) throw new InvalidOperationException("SmtpUser пуст в email.json");
        if (string.IsNullOrWhiteSpace(cfg.SmtpPass)) throw new InvalidOperationException("SmtpPass пуст в email.json");
        if (string.IsNullOrWhiteSpace(cfg.FromAddr)) throw new InvalidOperationException("FromAddr пуст в email.json");
        return cfg;
    }
}