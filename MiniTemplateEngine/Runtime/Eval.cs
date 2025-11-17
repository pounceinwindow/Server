using System.Collections;
using System.Globalization;
using System.Reflection;

namespace MiniTemplateEngine.Runtime;

/// <summary>
///     мини-вычислитель выражений
/// </summary>
internal static class Eval
{
    private static readonly object NotFound = new();

    /// <summary>
    ///     разрешает выражение вида "a.b.c" относительно контекста
    ///     поддерживает: скоупы, свойства/поля объекта, IDictionary(string→object).
    ///     спец имена: this - текущий скоуп/элемент, root - корневая модель.
    /// </summary>
    public static object? ResolvePath(string expr, Context ctx)
    {
        if (string.IsNullOrWhiteSpace(expr)) return null;

        var parts = expr.Split('.', StringSplitOptions.RemoveEmptyEntries);

        object? cur;

        if (!ctx.TryResolveName(parts[0], out cur))
        {
            if (parts.Length == 1 && ctx.TryResolveName("this", out var curThis))
                return curThis;

            cur = GetMember(ctx.Root, parts[0]);
            if (cur == NotFound)
            {
                if (string.Equals(parts[0], "root", StringComparison.OrdinalIgnoreCase))
                    cur = ctx.Root;
                else
                    return null;
            }
        }

        for (var i = 1; i < parts.Length; i++)
        {
            if (cur == null) return null;
            cur = GetMember(cur, parts[i]);
            if (cur == NotFound) return null;
        }

        return cur;
    }

    private static object GetMember(object target, string name)
    {
        if (target is IDictionary<string, object?> dict)
            return dict.TryGetValue(name, out var v) ? v! : NotFound;

        var t = target.GetType();

        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (p != null) return p.GetValue(target)!;

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (f != null) return f.GetValue(target)!;

        return NotFound;
    }

    public static bool IsTrue(object? v)
    {
        if (v is bool b) return b;
        if (v == null) return false;
        if (v is string s) return !string.IsNullOrWhiteSpace(s);
        if (v is IEnumerable e)
        {
            var en = e.GetEnumerator();
            try
            {
                return en.MoveNext();
            }
            finally
            {
                (en as IDisposable)?.Dispose();
            }
        }

        if (v is IConvertible c)
            try
            {
                var d = Convert.ToDecimal(c, CultureInfo.InvariantCulture);
                return d != 0m;
            }
            catch
            {
                return true;
            }

        return true;
    }

    /// <summary>
    ///     вычисляет условие: путь, сравнение ==, или вызов метода Contains
    /// </summary>
    public static bool EvaluateCondition(string expr, Context ctx)
    {
        // Проверка на Contains: "collection.Contains(item)"
        var containsIdx = expr.IndexOf(".Contains(", StringComparison.Ordinal);
        if (containsIdx >= 0)
        {
            var collectionPath = expr.Substring(0, containsIdx).Trim();
            var itemStart = containsIdx + ".Contains(".Length;
            var itemEnd = expr.IndexOf(')', itemStart);
            if (itemEnd < 0) throw new Exception("Malformed Contains expression");

            var itemPath = expr.Substring(itemStart, itemEnd - itemStart).Trim();

            var collection = ResolvePath(collectionPath, ctx);
            var item = ResolveValue(itemPath, ctx);

            if (collection is IEnumerable enumerable)
                foreach (var el in enumerable)
                    if (string.Equals(Convert.ToString(el, CultureInfo.InvariantCulture),
                            Convert.ToString(item, CultureInfo.InvariantCulture),
                            StringComparison.OrdinalIgnoreCase))
                        return true;

            return false;
        }

        // Проверка на ==
        var idx = expr.IndexOf("==", StringComparison.Ordinal);
        if (idx >= 0)
        {
            var left = expr[..idx].Trim();
            var right = expr[(idx + 2)..].Trim();

            var l = ResolveValue(left, ctx);
            var r = ResolveValue(right, ctx);

            return string.Equals(Convert.ToString(l, CultureInfo.InvariantCulture),
                Convert.ToString(r, CultureInfo.InvariantCulture),
                StringComparison.OrdinalIgnoreCase);
        }

        return IsTrue(ResolvePath(expr.Trim(), ctx));
    }

    private static object? ResolveValue(string token, Context ctx)
    {
        if ((token.StartsWith("'") && token.EndsWith("'")) ||
            (token.StartsWith("\"") && token.EndsWith("\"")))
            return token[1..^1];

        if (decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            return num;

        return ResolvePath(token, ctx);
    }

    public static IEnumerable? ResolveEnumerable(string expr, Context ctx)
    {
        var v = ResolvePath(expr, ctx);
        return v as IEnumerable;
    }
}