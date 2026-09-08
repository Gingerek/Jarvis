using Jarvis.Core;
using Microsoft.Data.Sqlite;

namespace Jarvis.Learning;

public sealed class LearningDatabase
{
    private readonly string _connectionString;

    public LearningDatabase(string? databasePath = null)
    {
        JarvisPaths.EnsureCreated();
        databasePath ??= Path.Combine(JarvisPaths.DataDirectory, "learning.db");
        _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = Schema;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private const string Schema = """
        CREATE TABLE IF NOT EXISTS aliases (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            phrase TEXT NOT NULL UNIQUE,
            target_intent TEXT NOT NULL,
            created_utc TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS workflows (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL UNIQUE,
            definition_json TEXT NOT NULL,
            created_utc TEXT NOT NULL
        );
        """;
}
