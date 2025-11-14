using System.Text;

namespace MiniTemplateEngine.Parsing;

/// <summary>Рекурсивный парсер шаблона в AST</summary>
internal sealed class Parser
{
    private readonly string _src; // исходный шаблон
    private int _pos; // текущая позиция

    private Parser(string src)
    {
        _src = src;
        _pos = 0;
    }

    /// <summary>Точка входа</summary>
    public static List<Node> Parse(string src)
    {
        return new Parser(src).ParseBlock(null);
    }

    // Разбирает блок до встреченного стоп-токена (else/endif/endfor) или конца файла
    private List<Node> ParseBlock(string? stopToken)
    {
        var nodes = new List<Node>(); // накопление узлов
        while (!Eof())
        {
            if (Peek("$else") && stopToken == "$endif") break; // $else завершает then-блок
            if (Peek("$endif") && (stopToken == "$endif" || stopToken == null)) break;
            if (Peek("$endfor") && (stopToken == "$endfor" || stopToken == null)) break;

            if (Peek("${"))
                nodes.Add(ParseVar()); // переменная
            else if (Peek("$if("))
                nodes.Add(ParseIf()); // условие
            else if (Peek("$foreach("))
                nodes.Add(ParseFor()); // цикл
            else
                nodes.Add(ParseText(stopToken)); // обычный текст до следующего директивного $
        }

        return nodes;
    }

    // Разбор узлов

    private VarNode ParseVar()
    {
        if (Peek("${"))
        {
            Expect("${");
            var expr = ReadUntil('}').Trim();
            Expect("}");
            return new VarNode(expr);
        }

        // вариант $(...)
        Expect("$(");
        var expr2 = ReadUntil(')').Trim();
        Expect(")");
        return new VarNode(expr2);
    }

    private IfNode ParseIf()
    {
        Expect("$if("); // начало if
        var cond = ReadUntil(')').Trim(); // выражение в скобках
        Expect(")"); // закрыли ')'

        var thenPart = ParseBlock("$endif"); // парсим then до $else/$endif

        List<Node>? elsePart = null; // по умолчанию else нет
        if (Peek("$else"))
        {
            Expect("$else"); // съедаем $else
            elsePart = ParseBlock("$endif"); // парсим else до $endif
        }

        Expect("$endif"); // закрывающий маркер
        return new IfNode(cond, thenPart, elsePart); // собираем узел
    }

    private ForEachNode ParseFor()
    {
        Expect("$foreach("); // начало foreach
        var inside = ReadUntil(')'); // вся сигнатура до ')'
        Expect(")"); // закрыли ')'

        // Поддерживаем "var item in expr" и "item in expr"
        var sig = inside.Trim();
        if (sig.StartsWith("var ", StringComparison.Ordinal))
            sig = sig.Substring(4);

        var parts = sig.Split(new[] { " in " }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) throw new Exception("Malformed foreach header. Use: $foreach(var item in object.Items)");

        var item = parts[0].Trim();
        var srcExpr = parts[1].Trim();

        var body = ParseBlock("$endfor"); // тело до $endfor
        Expect("$endfor"); // конец цикла

        return new ForEachNode(item, srcExpr, body); // собираем узел
    }

    private TextNode ParseText(string? stopToken)
    {
        var sb = new StringBuilder();
        while (!Eof())
        {
            // начинаются директивы - выходим
            if (Peek("${") || Peek("$if(") || Peek("$foreach(")) break;

            // достигли явного стоп-токена (например, $endif / $endfor)
            if (stopToken != null && Peek(stopToken)) break;

            // >>> ВАЖНО: внутри if-блока $else тоже должен завершать текст
            if (stopToken == "$endif" && Peek("$else")) break;

            sb.Append(_src[_pos]);
            _pos++;
        }

        return new TextNode(sb.ToString());
    }

    private bool Eof()
    {
        return _pos >= _src.Length;
        // достигли конца
    }

    private bool Peek(string s) // проверка, что впереди строка s
    {
        if (_pos + s.Length > _src.Length) return false;
        for (var i = 0; i < s.Length; i++)
            if (_src[_pos + i] != s[i])
                return false;
        return true;
    }

    private void Expect(string s)
    {
        if (!Peek(s)) throw new Exception($"Expected '{s}' at {_pos}");
        _pos += s.Length;
    }

    private string ReadUntil(char end) // читать до символа end (не включая)
    {
        var start = _pos; // запоминаем старт
        while (!Eof() && _src[_pos] != end) _pos++; // идём до end
        return _src.Substring(start, _pos - start); // возвращаем подстроку
    }
}