namespace MiniTemplateEngine.Abstraction;

/// <summary>
///     Рендерер HTML-шаблонов с поддержкой переменных, условий и циклов
/// </summary>
public interface IHtmlTemplateRenderer
{
    /// <summary>
    ///     Рендерит HTML из строки шаблона и объекта данных
    /// </summary>
    /// <param name="htmlTemplate">Текст шаблона</param>
    /// <param name="dataModel">Объект с данными (root)</param>
    /// <returns>Готовый HTML</returns>
    string RenderFromString(string htmlTemplate, object dataModel);

    /// <summary>
    ///     Рендерит HTML, считывая шаблон из файла
    /// </summary>
    /// <param name="filePath">Путь к файлу с шаблоном</param>
    /// <param name="dataModel">Объект с данными</param>
    /// <returns>Готовый HTML</returns>
    string RenderFromFile(string filePath, object dataModel);

    /// <summary>
    ///     Рендерит HTML из входного файла и записывает результат в выходной файл
    /// </summary>
    /// <param name="inputFilePath">Путь к входному шаблону</param>
    /// <param name="outputFilePath">Куда сохранить результат</param>
    /// <param name="dataModel">Объект с данными</param>
    /// <returns>Полный путь выходного файла</returns>
    string RenderToFile(string inputFilePath, string outputFilePath, object dataModel);
}