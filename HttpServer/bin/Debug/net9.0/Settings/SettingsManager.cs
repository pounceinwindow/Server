using System.Text.Json;

namespace HttpServer.Framework.Settings;

public class SettingsManager
{
    private static SettingsManager _instance;

    private static readonly object _lock = new();

    private SettingsManager()
    {
        LoadSettings();
    }

    public SettingsModel Settings { get; private set; }

    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new SettingsManager();
                }

            return _instance;
        }
    }

    private void LoadSettings()
    {
        try
        {
            var settingsFile = File.ReadAllText("settings.json");
            Settings = JsonSerializer.Deserialize<SettingsModel>(settingsFile)
                       ?? throw new InvalidOperationException("Десериализация провалилась");

            if (string.IsNullOrEmpty(Settings.StaticDirectoryPath))
                throw new InvalidOperationException("Поле 'StaticFilesPath' не было заполнено из settings.json");

            if (string.IsNullOrEmpty(Settings.Domain))
                throw new InvalidOperationException("Поле 'Domain' не было заполнено из settings.json");

            if (string.IsNullOrEmpty(Settings.Port))
                throw new InvalidOperationException("Поле 'Port' не было заполнено из settings.json");
            if (string.IsNullOrEmpty(Settings.ConnectionString))
                throw new InvalidOperationException("Поле 'ConnectionString' не было заполнено из settings.json");

            Console.WriteLine("Настройки упешно загружены");
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException("Файл settings.json не был найден");
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new DirectoryNotFoundException("Директория с файлом settings.json не была найдена");
        }
    }
}