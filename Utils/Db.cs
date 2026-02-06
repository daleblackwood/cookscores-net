using System.Reflection;
using System.Text.Json;
using Npgsql;

namespace CookScores;

public class Row : Dictionary<string, object>
{
}

public class Db
{
    readonly NpgsqlDataSource source;

    public Db(string connectionString) => source = NpgsqlDataSource.Create(connectionString);

    NpgsqlCommand Command(string sql, object? args)
    {
        var cmd = source.CreateCommand(sql);
        foreach (var p in args?.GetType().GetProperties() ?? Array.Empty<PropertyInfo>())
            cmd.Parameters.AddWithValue(p.Name, p.GetValue(args) ?? DBNull.Value);
        return cmd;
    }

    public async Task Exec(string sql, object? args = null)
    {
        await using var cmd = Command(sql, args);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<T> Scalar<T>(string sql, object? args = null)
    {
        await using var cmd = Command(sql, args);
        return (T)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<List<Row>> All(string sql, object? args = null)
    {
        await using var cmd = Command(sql, args);
        await using var reader = await cmd.ExecuteReaderAsync();
        var rows = new List<Row>();
        while (await reader.ReadAsync())
        {
            var row = new Row();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i)) continue;
                row[reader.GetName(i)] = reader.GetDataTypeName(i) == "jsonb"
                    ? JsonDocument.Parse(reader.GetString(i)).RootElement
                    : reader.GetValue(i);
            }
            rows.Add(row);
        }
        return rows;
    }

    public async Task<Row?> One(string sql, object? args = null) => (await All(sql, args)).FirstOrDefault();
}
