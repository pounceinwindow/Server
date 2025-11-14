namespace MiniTemplateEngine.Parsing;

/// <summary>Базовый узел AST</summary>
internal abstract class Node
{
}

/// <summary>Обычный текст</summary>
internal sealed class TextNode : Node
{
    public string Text; // сырой текст

    public TextNode(string text)
    {
        Text = text;
    }
}

/// <summary>${expr}</summary>
internal sealed class VarNode : Node
{
    public string Expr; // выражение пути

    public VarNode(string expr)
    {
        Expr = expr;
    }
}

/// <summary>$if(cond) . [$else ..] $endif</summary>
internal sealed class IfNode : Node
{
    public string Condition; // выражение условия
    public List<Node>? Else; // тело else (опц)
    public List<Node> Then; // тело then

    public IfNode(string condition, List<Node> thenPart, List<Node>? elsePart)
    {
        Condition = condition;
        Then = thenPart;
        Else = elsePart;
    }
}

/// <summary>$foreach(..)</summary>
internal sealed class ForEachNode : Node
{
    public List<Node> Body; // тело цикла
    public string ItemVar; // имя переменной элемента
    public string SourceExpr; // выражение источника (dotted path)

    public ForEachNode(string itemVar, string sourceExpr, List<Node> body)
    {
        ItemVar = itemVar;
        SourceExpr = sourceExpr;
        Body = body;
    }
}