using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Npgsql;

namespace MyORM;

public class OrmContext
{
    private readonly string _connectionString;

    public OrmContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    private string GetTableName<T>(string tableName = null)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
            return tableName.ToLower();
        var name = typeof(T).Name.ToLower().Replace("model", "");
        if (!name.EndsWith("s"))
            name += "s";
        return name;
    }

    public T Create<T>(T entity, string tableName = null) where T : class, new()
    {
        using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var props = typeof(T).GetProperties().ToList();
        var cols = props.Skip(1).Select(p => p.Name.ToLower()).ToList();
        var table = GetTableName<T>(tableName);
        var sql =
            $"INSERT INTO {table} ({string.Join(",", cols)}) VALUES ({string.Join(",", cols.Select(c => "@" + c))}) RETURNING *;";
        var cmd = dataSource.CreateCommand(sql);
        foreach (var p in props.Skip(1))
            cmd.Parameters.AddWithValue(p.Name.ToLower(), p.GetValue(entity) ?? DBNull.Value);
        using var r = cmd.ExecuteReader();
        if (r.Read())
            return Map<T>(r);
        return entity;
    }

    public T ReadById<T>(int id, string tableName = null) where T : class, new()
    {
        using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var table = GetTableName<T>(tableName);
        var sql = $"SELECT * FROM {table} WHERE id = @id LIMIT 1";
        var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (r.Read())
            return Map<T>(r);
        return null;
    }

    public List<T> ReadAll<T>(string tableName = null) where T : class, new()
    {
        using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var table = GetTableName<T>(tableName);
        var cmd = dataSource.CreateCommand($"SELECT * FROM {table}");
        using var r = cmd.ExecuteReader();
        var list = new List<T>();
        while (r.Read())
            list.Add(Map<T>(r));
        return list;
    }

    public void Update<T>(int id, T entity, string tableName = null) where T : class, new()
    {
        using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var props = typeof(T).GetProperties().ToList();
        var table = GetTableName<T>(tableName);
        var sets = string.Join(",", props.Skip(1).Select(p => $"{p.Name.ToLower()}=@{p.Name.ToLower()}"));
        var sql = $"UPDATE {table} SET {sets} WHERE id=@id";
        var cmd = dataSource.CreateCommand(sql);
        foreach (var p in props.Skip(1))
            cmd.Parameters.AddWithValue(p.Name.ToLower(), p.GetValue(entity) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void Delete<T>(int id, string tableName = null) where T : class, new()
    {
        using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var table = GetTableName<T>(tableName);
        var sql = $"DELETE FROM {table} WHERE id = @id";
        var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string tableName = null) where T : class, new()
    {
        return Where(predicate, tableName).FirstOrDefault();
    }

    public IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, string tableName = null) where T : class, new()
    {
        var where = BuildSqlQuery(predicate.Body);
        var table = GetTableName<T>(tableName);
        var sql = $"SELECT * FROM {table} WHERE {where}";
        using var conn = new NpgsqlConnection(_connectionString);
        using var cmd = new NpgsqlCommand(sql, conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            yield return Map<T>(reader);
    }

    private string BuildSqlQuery(Expression expression)
    {
        switch (expression)
        {
            case BinaryExpression b:
                return $"({BuildSqlQuery(b.Left)} {GetSqlOperator(b.NodeType)} {BuildSqlQuery(b.Right)})";
            case MemberExpression m when m.Expression is ParameterExpression:
                return ToSnakeCase(m.Member.Name);
            case MemberExpression m:
                return FormatConstant(GetValue(m));
            case ConstantExpression c:
                return FormatConstant(c.Value);
            case UnaryExpression u when u.NodeType == ExpressionType.Convert:
                return BuildSqlQuery(u.Operand);
            case MethodCallExpression mc:
                return HandleMethodCall(mc);
            default:
                throw new NotSupportedException($"Unsupported expression: {expression.GetType().Name}");
        }
    }

    private string HandleMethodCall(MethodCallExpression mc)
    {
        if (mc.Object != null && mc.Method.DeclaringType == typeof(string) && mc.Arguments.Count == 1)
        {
            var obj = BuildSqlQuery(mc.Object);
            var arg = FormatConstant(GetValue(mc.Arguments[0]));
            return mc.Method.Name switch
            {
                "Contains" => $"{obj} LIKE '%' || {arg} || '%'",
                "StartsWith" => $"{obj} LIKE {arg} || '%'",
                "EndsWith" => $"{obj} LIKE '%' || {arg}",
                _ => throw new NotSupportedException($"Method {mc.Method.Name} not supported")
            };
        }

        throw new NotSupportedException($"Method {mc.Method.Name} not supported");
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
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder(name.Length + 5);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static string FormatConstant(object value)
    {
        return value switch
        {
            null => "NULL",
            string s => $"'{s.Replace("'", "''")}'",
            bool b => b ? "TRUE" : "FALSE",
            _ => value.ToString()
        };
    }

    private static object GetValue(Expression expr)
    {
        return Expression.Lambda(expr).Compile().DynamicInvoke();
    }

    private static T Map<T>(NpgsqlDataReader r) where T : class, new()
    {
        var obj = new T();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite)
            .ToList();
        var cols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < r.FieldCount; i++)
            cols[r.GetName(i)] = i;
        foreach (var p in props)
        {
            if (!cols.TryGetValue(p.Name, out var idx) &&
                !cols.TryGetValue(p.Name.ToLower(), out idx) &&
                !cols.TryGetValue(ToSnakeCase(p.Name), out idx))
                continue;
            if (r.IsDBNull(idx))
                continue;
            var val = r.GetValue(idx);
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            var converted = Convert.ChangeType(val, t);
            p.SetValue(obj, converted);
        }

        return obj;
    }
}