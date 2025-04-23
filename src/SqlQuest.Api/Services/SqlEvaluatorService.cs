using Microsoft.Data.Sqlite;
using SqlQuest.Api.Models;
using SqlQuest.Api.DTOs;

namespace SqlQuest.Api.Services;

public class SqlEvaluatorService
{
    public async Task<AttemptResult> EvaluateAsync(Challenge challenge, string userSql)
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        // Seed
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = challenge.SeedSql;
            await cmd.ExecuteNonQueryAsync();
        }

        var expected = await RunQueryAsync(conn, challenge.SolutionSql);

        List<Dictionary<string, object>> userRows;
        try
        {
            userRows = await RunQueryAsync(conn, userSql);
        }
        catch (Exception ex)
        {
            return new AttemptResult(false, $"Erro ao executar seu SQL: {ex.Message}");
        }

        bool passed = expected.Count == userRows.Count &&
                      expected.Zip(userRows).All(p => p.First.SequenceEqual(p.Second));

        string msg = passed ? "Parabéns! Desafio concluído." : "Resultados diferentes, tente novamente.";
        return new AttemptResult(passed, msg, expected, passed ? null : userRows);
    }

    private static async Task<List<Dictionary<string, object>>> RunQueryAsync(SqliteConnection c, string sql)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        using var rdr = await cmd.ExecuteReaderAsync();

        var list = new List<Dictionary<string, object>>();
        while (await rdr.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < rdr.FieldCount; i++)
                row[rdr.GetName(i)] = rdr.GetValue(i);
            list.Add(row);
        }
        return list;
    }
}
