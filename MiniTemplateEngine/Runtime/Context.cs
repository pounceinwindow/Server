namespace MiniTemplateEngine.Runtime;

/// <summary>Выполняемый контекст - root-модель и стек скоупов</summary>
internal sealed class Context
{
    // Стек словарей имён => значений верхушка - самый внутренний скоуп
    private readonly Stack<Dictionary<string, object?>> _scopes = new();

    /// <summary>Создаёт контекст, сразу добавляя пустой скоуп</summary>
    public Context(object root)
    {
        Root = root; // сохраняем ссылку на корневую модель
        _scopes.Push(new Dictionary<string, object?>()); // как минимум один скоуп всегда есть
    }

    // Корневой объект-модель (root в выражениях)
    public object Root { get; }

    /// <summary>Положить имя/значение в верхний скоуп</summary>
    public void Set(string name, object? value)
    {
        _scopes.Peek()[name] = value;
    }

    /// <summary>Новый скоуп (для foreach/if)</summary>
    public void PushScope()
    {
        _scopes.Push(new Dictionary<string, object?>());
    }

    /// <summary>Закрыть текущий скоуп</summary>
    public void PopScope()
    {
        if (_scopes.Count > 1) _scopes.Pop();
    }

    /// <summary>Попробовать найти имя в скоупах</summary>
    public bool TryResolveName(string name, out object? value)
    {
        foreach (var scope in _scopes) // ищем от внутреннего к внешнему
            if (scope.TryGetValue(name, out value))
                return true;
        value = null;
        return false;
    }
}