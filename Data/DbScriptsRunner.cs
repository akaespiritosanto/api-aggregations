namespace api_aggregations.Data;

using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

public static class DbScriptsRunner
{
    private const string HistoryTableName = "[dbo].[__DbScriptsHistory]";
    private static readonly Regex GoRegex = new(@"^\s*GO(?:\s+(?<count>\d+))?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static async Task ApplyPendingAsync(AppDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            logger.LogInformation("DbScripts: skipped (non-relational provider).");
            return;
        }

        var scriptsDir = Path.Combine(AppContext.BaseDirectory, "DbScripts");
        if (!Directory.Exists(scriptsDir))
        {
            logger.LogInformation("DbScripts: folder not found at {ScriptsDir}. Skipping.", scriptsDir);
            return;
        }

        var scriptPaths = Directory.GetFiles(scriptsDir, "*.sql", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (scriptPaths.Length == 0)
        {
            logger.LogInformation("DbScripts: no .sql files found in {ScriptsDir}.", scriptsDir);
            return;
        }

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await EnsureHistoryTableAsync(dbContext, cancellationToken);

            var applied = await LoadAppliedScriptsAsync(dbContext, cancellationToken);

            foreach (var scriptPath in scriptPaths)
            {
                var scriptName = Path.GetFileName(scriptPath);
                var content = await File.ReadAllTextAsync(scriptPath, cancellationToken);
                var hash = ComputeSha256Hex(content);

                if (applied.TryGetValue(scriptName, out var existingHash))
                {
                    if (!string.Equals(existingHash, hash, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"DbScripts: script '{scriptName}' was already applied, but its contents changed. " +
                            "Create a new script file instead of modifying an applied one.");
                    }

                    continue;
                }

                logger.LogInformation("DbScripts: applying {ScriptName}...", scriptName);
                await ApplySingleScriptAsync(dbContext, scriptName, content, hash, cancellationToken);
                applied[scriptName] = hash;
                logger.LogInformation("DbScripts: applied {ScriptName}.", scriptName);
            }
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }

    private static async Task EnsureHistoryTableAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var createTableSql = $@"
IF OBJECT_ID(N'{HistoryTableName}', N'U') IS NULL
BEGIN
    CREATE TABLE {HistoryTableName}
    (
        [ScriptName] NVARCHAR(260) NOT NULL,
        [ScriptHash] CHAR(64) NOT NULL,
        [AppliedAtUtc] DATETIME2 NOT NULL,
        CONSTRAINT [PK___DbScriptsHistory] PRIMARY KEY CLUSTERED ([ScriptName] ASC)
    );
END";

        await ExecuteNonQueryAsync(dbContext, createTableSql, cancellationToken);
    }

    private static async Task<Dictionary<string, string>> LoadAppliedScriptsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var conn = dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = $"SELECT [ScriptName], [ScriptHash] FROM {HistoryTableName};";

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(0);
            var hash = reader.GetString(1);
            results[name] = hash;
        }

        return results;
    }

    private static async Task ApplySingleScriptAsync(
        AppDbContext dbContext,
        string scriptName,
        string content,
        string hash,
        CancellationToken cancellationToken)
    {
        var conn = dbContext.Database.GetDbConnection();
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var batch in SplitSqlBatches(content))
            {
                if (string.IsNullOrWhiteSpace(batch.Sql))
                {
                    continue;
                }

                for (var i = 0; i < batch.Repeat; i++)
                {
                    await using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = batch.Sql;
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            await using (var insert = conn.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandType = CommandType.Text;
                insert.CommandText = $@"
INSERT INTO {HistoryTableName} ([ScriptName], [ScriptHash], [AppliedAtUtc])
VALUES (@name, @hash, SYSUTCDATETIME());";

                var pName = insert.CreateParameter();
                pName.ParameterName = "@name";
                pName.Value = scriptName;
                insert.Parameters.Add(pName);

                var pHash = insert.CreateParameter();
                pHash.ParameterName = "@hash";
                pHash.Value = hash;
                insert.Parameters.Add(pHash);

                await insert.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task ExecuteNonQueryAsync(AppDbContext dbContext, string sql, CancellationToken cancellationToken)
    {
        var conn = dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string ComputeSha256Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static IEnumerable<(string Sql, int Repeat)> SplitSqlBatches(string sql)
    {
        var sb = new StringBuilder();

        using var reader = new StringReader(sql);
        while (true)
        {
            var line = reader.ReadLine();
            if (line is null)
            {
                break;
            }

            var match = GoRegex.Match(line);
            if (match.Success)
            {
                var batchSql = sb.ToString().Trim();
                sb.Clear();

                var repeat = 1;
                if (match.Groups["count"].Success && int.TryParse(match.Groups["count"].Value, out var parsed) && parsed > 0)
                {
                    repeat = parsed;
                }

                yield return (batchSql, repeat);
                continue;
            }

            sb.AppendLine(line);
        }

        var finalBatch = sb.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(finalBatch))
        {
            yield return (finalBatch, 1);
        }
    }
}

