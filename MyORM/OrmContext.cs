using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace MyORM;

public sealed class OrmContext : IDisposable
{
    private readonly string _connectionString;
    private readonly NpgsqlDataSource _ds;

    public OrmContext(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _ds = NpgsqlDataSource.Create(_connectionString);
    }

    public void Dispose()
    {
        _ds.Dispose();
    }

    private static string GetTableName<T>(string? tableName = null)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
            return tableName.ToLowerInvariant();

        var name = typeof(T).Name.ToLowerInvariant().Replace("model", "");
        if (!name.EndsWith("s"))
            name += "s";
        return name;
    }

    public T Create<T>(T entity, string? tableName = null) where T : class, new()
    {
        var table = GetTableName<T>(tableName);
        var colMap = BuildColumnMap<T>(_ds, table);

        if (colMap.Count == 0)
            throw new InvalidOperationException($"No matching columns found for table '{table}'");

        var cols = colMap.Values.ToList();

        var sql =
            $"INSERT INTO {table} ({string.Join(",", cols)}) " +
            $"VALUES ({string.Join(",", cols.Select(c => "@" + c))}) RETURNING *;";

        using var cmd = _ds.CreateCommand(sql);

        foreach (var kv in colMap)
        {
            var prop = typeof(T).GetProperty(kv.Key)!;
            var col = kv.Value;
            var val = prop.GetValue(entity) ?? DBNull.Value;

            if (val is decimal d)
                cmd.Parameters.Add(new NpgsqlParameter(col, NpgsqlDbType.Numeric) { Value = d });
            else
                cmd.Parameters.AddWithValue(col, val);
        }

        using var r = cmd.ExecuteReader();
        return r.Read() ? Map<T>(r) : entity;
    }

    public T ReadById<T>(int id, string? tableName = null) where T : class, new()
    {
        var table = GetTableName<T>(tableName);

        using var cmd = _ds.CreateCommand($"SELECT * FROM {table} WHERE id = @id LIMIT 1");
        cmd.Parameters.AddWithValue("id", id);

        using var r = cmd.ExecuteReader();
        return r.Read() ? Map<T>(r) : null;
    }

    public List<T> ReadAll<T>(string? tableName = null) where T : class, new()
    {
        var table = GetTableName<T>(tableName);

        using var cmd = _ds.CreateCommand($"SELECT * FROM {table}");
        using var r = cmd.ExecuteReader();

        var list = new List<T>();
        while (r.Read())
            list.Add(Map<T>(r));

        return list;
    }

    public void Update<T>(int id, T entity, string? tableName = null) where T : class, new()
    {
        var table = GetTableName<T>(tableName);
        var colMap = BuildColumnMap<T>(_ds, table);
        if (colMap.Count == 0) return;

        var sets = string.Join(",", colMap.Select(kv => $"{kv.Value}=@{kv.Value}"));
        var sql = $"UPDATE {table} SET {sets} WHERE id=@id";

        using var cmd = _ds.CreateCommand(sql);

        foreach (var kv in colMap)
        {
            var prop = typeof(T).GetProperty(kv.Key)!;
            var col = kv.Value;
            var val = prop.GetValue(entity) ?? DBNull.Value;

            if (val is decimal d)
                cmd.Parameters.Add(new NpgsqlParameter(col, NpgsqlDbType.Numeric) { Value = d });
            else
                cmd.Parameters.AddWithValue(col, val);
        }

        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    public void Delete<T>(int id, string? tableName = null) where T : class, new()
    {
        var table = GetTableName<T>(tableName);

        using var cmd = _ds.CreateCommand($"DELETE FROM {table} WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string? tableName = null) where T : class, new()
    {
        return Where(predicate, tableName).FirstOrDefault();
    }

    public IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, string? tableName = null) where T : class, new()
    {
        var table = GetTableName<T>(tableName);

        var built = BuildSqlPredicate(predicate.Body);
        var sql = $"SELECT * FROM {table} WHERE {built.Sql}";

        using var cmd = _ds.CreateCommand(sql);
        foreach (var p in built.Params)
            cmd.Parameters.Add(p);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            yield return Map<T>(reader);
    }

    public List<T> ExecuteQueryMultiple<T>(string sql, params NpgsqlParameter[] parameters) where T : class, new()
    {
        using var cmd = _ds.CreateCommand(sql);
        foreach (var p in parameters) cmd.Parameters.Add(p);

        using var r = cmd.ExecuteReader();
        var list = new List<T>();
        while (r.Read()) list.Add(Map<T>(r));
        return list;
    }

    public T ExecuteQuerySingle<T>(string sql, params NpgsqlParameter[] parameters) where T : class, new()
    {
        return ExecuteQueryMultiple<T>(sql, parameters).FirstOrDefault();
    }

    private static BuiltPredicate BuildSqlPredicate(Expression expression)
    {
        var ps = new List<NpgsqlParameter>(8);
        var idx = 0;
        var sql = BuildSqlQuery(expression, ps, ref idx);
        return new BuiltPredicate { Sql = sql, Params = ps };
    }

    private static string BuildSqlQuery(Expression expression, List<NpgsqlParameter> parameters, ref int idx)
    {
        switch (expression)
        {
            case UnaryExpression u when u.NodeType == ExpressionType.Convert:
                return BuildSqlQuery(u.Operand, parameters, ref idx);

            case UnaryExpression u when u.NodeType == ExpressionType.Not:
                return $"NOT ({BuildSqlQuery(u.Operand, parameters, ref idx)})";

            case BinaryExpression b:
                if ((b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual) &&
                    (IsNullConstant(b.Left) || IsNullConstant(b.Right)))
                {
                    var colExpr = IsNullConstant(b.Left) ? b.Right : b.Left;
                    var colSql = BuildSqlQuery(colExpr, parameters, ref idx);
                    return b.NodeType == ExpressionType.Equal
                        ? $"{colSql} IS NULL"
                        : $"{colSql} IS NOT NULL";
                }

                return
                    $"({BuildSqlQuery(b.Left, parameters, ref idx)} {GetSqlOperator(b.NodeType)} {BuildSqlQuery(b.Right, parameters, ref idx)})";

            case MemberExpression m when m.Expression is ParameterExpression:
                return ToSnakeCase(m.Member.Name);

            case MemberExpression m:
                return AddParam(GetValue(m), parameters, ref idx);

            case ConstantExpression c:
                return AddParam(c.Value, parameters, ref idx);

            case MethodCallExpression mc:
                return HandleMethodCall(mc, parameters, ref idx);

            default:
                throw new NotSupportedException($"Unsupported expression: {expression.GetType().Name}");
        }
    }

    private static string HandleMethodCall(MethodCallExpression mc, List<NpgsqlParameter> parameters, ref int idx)
    {
        if (mc.Object != null &&
            mc.Method.DeclaringType == typeof(string) &&
            mc.Arguments.Count == 1)
        {
            var obj = BuildSqlQuery(mc.Object, parameters, ref idx);
            var raw = GetValue(mc.Arguments[0])?.ToString() ?? string.Empty;

            return mc.Method.Name switch
            {
                "Contains" => $"{obj} LIKE {AddParam($"%{raw}%", parameters, ref idx)}",
                "StartsWith" => $"{obj} LIKE {AddParam($"{raw}%", parameters, ref idx)}",
                "EndsWith" => $"{obj} LIKE {AddParam($"%{raw}", parameters, ref idx)}",
                _ => throw new NotSupportedException($"Method {mc.Method.Name} not supported")
            };
        }

        throw new NotSupportedException($"Method {mc.Method.Name} not supported");
    }

    private static bool IsNullConstant(Expression expr)
    {
        if (expr is ConstantExpression c) return c.Value == null;

        if (expr is UnaryExpression u && u.NodeType == ExpressionType.Convert)
            return IsNullConstant(u.Operand);

        if (expr is MemberExpression m && m.Expression is not ParameterExpression)
            try
            {
                return GetValue(m) == null;
            }
            catch
            {
                return false;
            }

        return false;
    }

    private static string AddParam(object? value, List<NpgsqlParameter> parameters, ref int idx)
    {
        var name = "p" + idx++;
        var placeholder = "@" + name;

        var val = value ?? DBNull.Value;

        if (value is decimal dec)
            parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Numeric) { Value = dec });
        else
            parameters.Add(new NpgsqlParameter(name, val));

        return placeholder;
    }

    private static string GetSqlOperator(ExpressionType type)
    {
        return type switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Operator {type} not supported")
        };
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var sb = new StringBuilder(name.Length + 5);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static object? GetValue(Expression expr)
    {
        return Expression.Lambda(expr).Compile().DynamicInvoke();
    }

    private static T Map<T>(NpgsqlDataReader r) where T : class, new()
    {
        var obj = new T();
        var props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        var cols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < r.FieldCount; i++)
            cols[r.GetName(i)] = i;

        foreach (var p in props)
        {
            if (!cols.TryGetValue(p.Name, out var idx) &&
                !cols.TryGetValue(p.Name.ToLowerInvariant(), out idx) &&
                !cols.TryGetValue(ToSnakeCase(p.Name), out idx))
                continue;

            if (r.IsDBNull(idx)) continue;

            var val = r.GetValue(idx);
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

            try
            {
                if (t.IsEnum)
                {
                    p.SetValue(obj, Enum.ToObject(t, val));
                }
                else
                {
                    var converted = Convert.ChangeType(val, t);
                    p.SetValue(obj, converted);
                }
            }
            catch
            {
            }
        }

        return obj;
    }

    private static IEnumerable<string> GetColumns(NpgsqlDataSource ds, string table)
    {
        using var cmd = ds.CreateCommand(
            "select column_name from information_schema.columns where table_name = @t");
        cmd.Parameters.AddWithValue("t", table.ToLowerInvariant());

        using var r = cmd.ExecuteReader();
        while (r.Read())
            yield return r.GetString(0);
    }

    private static Dictionary<string, string> BuildColumnMap<T>(NpgsqlDataSource ds, string table)
    {
        var existing = new HashSet<string>(GetColumns(ds, table), StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanRead || string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
                continue;

            var c1 = p.Name;
            var c2 = p.Name.ToLowerInvariant();
            var c3 = ToSnakeCase(p.Name);

            if (existing.Contains(c1)) map[p.Name] = c1;
            else if (existing.Contains(c2)) map[p.Name] = c2;
            else if (existing.Contains(c3)) map[p.Name] = c3;
        }

        return map;
    }

    private sealed class BuiltPredicate
    {
        public required string Sql { get; init; }
        public required List<NpgsqlParameter> Params { get; init; }
    }
}