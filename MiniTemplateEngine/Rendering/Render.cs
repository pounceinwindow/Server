using System.Text;
using MiniTemplateEngine.Parsing;
using MiniTemplateEngine.Runtime;

namespace MiniTemplateEngine.Rendering;

/// <summary>Обход AST и генерация строки</summary>
internal static class Render
{
    /// <summary>Рендерит список узлов в строку</summary>
    public static string Run(List<Node> nodes, object model)
    {
        var ctx = new Context(model); // создаём контекст
        ctx.Set("root", model);
        var sb = new StringBuilder(); // аккумулятор вывода
        foreach (var n in nodes) // обходим все узлы
            WriteNode(n, ctx, sb); // и пишем их
        return sb.ToString(); // возвращаем результат
    }

    // Рендер одного узла
    private static void WriteNode(Node n, Context ctx, StringBuilder sb)
    {
        switch (n)
        {
            case TextNode t:
                sb.Append(t.Text); // просто текст
                break;

            case VarNode v:
                var val = Eval.ResolvePath(v.Expr, ctx);
                var raw = v.Expr.EndsWith("Html", StringComparison.OrdinalIgnoreCase);
                sb.Append(raw ? val?.ToString() ?? "" : System.Net.WebUtility.HtmlEncode(val?.ToString() ?? ""));
                break;


            case IfNode i:
                var ok = Eval.EvaluateCondition(i.Condition, ctx); // вычисляем условие
                var branch = ok ? i.Then : i.Else ?? new List<Node>();
                foreach (var child in branch) // печатаем нужную ветку
                    WriteNode(child, ctx, sb);
                break;

            case ForEachNode f:
                var en = Eval.ResolveEnumerable(f.SourceExpr, ctx); // берём перечислимый
                if (en == null) break; // нечего перебирать
                foreach (var item in en)
                {
                    ctx.PushScope(); // новый скоуп на итерацию
                    ctx.Set("this", item); // this - текущий элемент
                    ctx.Set(f.ItemVar, item); // имя переменной из заголовка
                    foreach (var child in f.Body) // рендерим тело
                        WriteNode(child, ctx, sb);
                    ctx.PopScope(); // закрываем скоуп
                }

                break;

            default:
                throw new NotSupportedException($"Unknown node: {n.GetType().Name}");
        }
    }
}