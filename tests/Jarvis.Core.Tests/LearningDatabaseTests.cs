using Jarvis.Learning;
using Microsoft.Data.Sqlite;

namespace Jarvis.Core.Tests;

public sealed class LearningDatabaseTests
{
    [Fact]
    public async Task Initialize_Creates_Required_Tables()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jarvis-learning-{Guid.NewGuid():N}.db");
        try
        {
            var database = new LearningDatabase(path);
            await database.InitializeAsync();

            var names = new List<string>();
            await using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name IN ('aliases','workflows') ORDER BY name;";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) names.Add(reader.GetString(0));
            }

            Assert.Equal(["aliases", "workflows"], names);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
