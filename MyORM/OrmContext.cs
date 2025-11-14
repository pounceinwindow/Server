using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Npgsql;

namespace MyORM;

public sealed class OrmContext : IDisposable
{
    private readonly string _connectionString;

    public OrmContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Dispose() { }

    private static string GetTableName<T>(string tableName = null)
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
        using var ds = NpgsqlDataSource.Create(_connectionString);
        var table  = GetTableName<T>(tableName);
        var colMap = BuildColumnMap<T>(ds, table);       

        if (colMap.Count == 0)
            throw new InvalidOperationException($"No matching columns found for table '{table}'");

        var cols = colMap.Values.ToList();
        var sql  = $"INSERT INTO {table} ({string.Join(",", cols)}) " +
                   $"VALUES ({string.Join(",", cols.Select(c => "@" + c))}) RETURNING *;";

        using var cmd = ds.CreateCommand(sql);
        foreach (var kv in colMap)
        {
            var prop = typeof(T).GetProperty(kv.Key)!;
            var col  = kv.Value;
            var val  = prop.GetValue(entity) ?? DBNull.Value;

            if (val is decimal d)
                cmd.Parameters.Add(new Npgsql.NpgsqlParameter(col, NpgsqlTypes.NpgsqlDbType.Numeric){ Value = d });
            else
                cmd.Parameters.AddWithValue(col, val);
        }

        using var r = cmd.ExecuteReader();
        return r.Read() ? Map<T>(r) : entity;
    }


    public T ReadById<T>(int id, string tableName = null) where T : class, new()
    {
        using var ds = NpgsqlDataSource.Create(_connectionString);
        var table = GetTableName<T>(tableName);
        using var cmd = ds.CreateCommand($"SELECT * FROM {table} WHERE id = @id LIMIT 1");
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map<T>(r) : null;
    }

    public List<T> ReadAll<T>(string tableName = null) where T : class, new()
    {
        using var ds = NpgsqlDataSource.Create(_connectionString);
        var table = GetTableName<T>(tableName);
        using var cmd = ds.CreateCommand($"SELECT * FROM {table}");
        using var r = cmd.ExecuteReader();
        var list = new List<T>();
        while (r.Read()) list.Add(Map<T>(r));
        return list;
    }

    public void Update<T>(int id, T entity, string tableName = null) where T : class, new()
    {
        using var ds = NpgsqlDataSource.Create(_connectionString);
        var table  = GetTableName<T>(tableName);
        var colMap = BuildColumnMap<T>(ds, table);
        if (colMap.Count == 0) return;

        var sets = string.Join(",", colMap.Select(kv => $"{kv.Value}=@{kv.Value}"));
        var sql  = $"UPDATE {table} SET {sets} WHERE id=@id";

        using var cmd = ds.CreateCommand(sql);
        foreach (var kv in colMap)
        {
            var prop = typeof(T).GetProperty(kv.Key)!;
            var col  = kv.Value;
            var val  = prop.GetValue(entity) ?? DBNull.Value;

            if (val is decimal d)
                cmd.Parameters.Add(new Npgsql.NpgsqlParameter(col, NpgsqlTypes.NpgsqlDbType.Numeric){ Value = d });
            else
                cmd.Parameters.AddWithValue(col, val);
        }
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }


    public void Delete<T>(int id, string tableName = null) where T : class, new()
    {
        using var ds = NpgsqlDataSource.Create(_connectionString);
        var table = GetTableName<T>(tableName);
        using var cmd = ds.CreateCommand($"DELETE FROM {table} WHERE id = @id");
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string tableName = null) where T : class, new()
        => Where(predicate, tableName).FirstOrDefault();

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

    private static string BuildSqlQuery(Expression expression) =>
        expression switch
        {
            BinaryExpression b => $"({BuildSqlQuery(b.Left)} {GetSqlOperator(b.NodeType)} {BuildSqlQuery(b.Right)})",
            MemberExpression m when m.Expression is ParameterExpression => ToSnakeCase(m.Member.Name),
            MemberExpression m => FormatConstant(GetValue(m)),
            ConstantExpression c => FormatConstant(c.Value),
            UnaryExpression u when u.NodeType == ExpressionType.Convert => BuildSqlQuery(u.Operand),
            MethodCallExpression mc => HandleMethodCall(mc),
            _ => throw new NotSupportedException($"Unsupported expression: {expression.GetType().Name}")
        };

    private static string HandleMethodCall(MethodCallExpression mc)
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

    private static string GetSqlOperator(ExpressionType type) => type switch
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
            else sb.Append(c);
        }
        return sb.ToString();
    }

    private static string FormatConstant(object value) => value switch
    {
        null => "NULL",
        string s => $"'{s.Replace("'", "''")}'",
        bool b => b ? "TRUE" : "FALSE",
        _ => value.ToString()
    };

    private static object GetValue(Expression expr) => Expression.Lambda(expr).Compile().DynamicInvoke();

    private static T Map<T>(NpgsqlDataReader r) where T : class, new()
    {
        var obj = new T();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite).ToList();
        var cols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < r.FieldCount; i++) cols[r.GetName(i)] = i;

        foreach (var p in props)
        {
            if (!cols.TryGetValue(p.Name, out var idx) &&
                !cols.TryGetValue(p.Name.ToLower(), out idx) &&
                !cols.TryGetValue(ToSnakeCase(p.Name), out idx))
                continue;

            if (r.IsDBNull(idx)) continue;

            var val = r.GetValue(idx);
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            var converted = Convert.ChangeType(val, t);
            p.SetValue(obj, converted);
        }
        return obj;
    }
    private static IEnumerable<string> GetColumns(NpgsqlDataSource ds, string table)
    {
        using var cmd = ds.CreateCommand(
            "select column_name from information_schema.columns where table_name = @t");
        cmd.Parameters.AddWithValue("t", table.ToLowerInvariant());
        using var r = cmd.ExecuteReader();
        while (r.Read()) yield return r.GetString(0);
    }

    private Dictionary<string,string> BuildColumnMap<T>(NpgsqlDataSource ds, string table)
    {
        var existing = new HashSet<string>(GetColumns(ds, table), StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance))
        {
            if (!p.CanRead || string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)) continue;

            var c1 = p.Name;                   
            var c2 = p.Name.ToLowerInvariant();  
            var c3 = ToSnakeCase(p.Name);        

            if (existing.Contains(c1)) map[p.Name] = c1;
            else if (existing.Contains(c2)) map[p.Name] = c2;
            else if (existing.Contains(c3)) map[p.Name] = c3;
        }
        return map;
    }

}
