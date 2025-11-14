using MiniTemplateEngine.Abstraction;
using MiniTemplateEngine.Parsing;
using MiniTemplateEngine.Rendering;

namespace MiniTemplateEngine;

/// <summary>реализация IHtmlTemplateRenderer на базе собственного парсера</summary>
public sealed class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    /// <inheritdoc />
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        var ast = Parser.Parse(htmlTemplate); // парсим шаблон в асд
        return Render.Run(ast, dataModel); // рендерим
    }

    /// <inheritdoc />
    public string RenderFromFile(string filePath, object dataModel)
    {
        var text = File.ReadAllText(filePath); // читаем шаблон
        return RenderFromString(text, dataModel); // делегируем
    }

    /// <inheritdoc />
    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        var result = RenderFromFile(inputFilePath, dataModel); // получаем результат
        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? "."); // гарантируем папку
        File.WriteAllText(outputFilePath, result); // пишем файл
        return result; // также возвращаем строку
    }
}