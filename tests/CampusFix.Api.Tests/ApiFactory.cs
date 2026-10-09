using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CampusFix.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string StaffEmail = "staff@example.test";
    public const string StaffPassword = "Test-only-password!123";
    private readonly string sqliteFile = Path.Combine(Path.GetTempPath(), $"campusfix-test-{Guid.NewGuid():N}.db");
    private readonly string? postgresAdminConnection = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION");
    private readonly string postgresDatabase = $"campusfix_test_{Guid.NewGuid():N}";
    private bool postgresCreated;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connection = $"Data Source={sqliteFile};Pooling=False";
        var provider = "Sqlite";
        if (!string.IsNullOrWhiteSpace(postgresAdminConnection))
        {
            // The supplied CI connection must have CREATE DATABASE permission.
            // Only this uniquely named test database is removed during cleanup.
            using var admin = new NpgsqlConnection(postgresAdminConnection);
            admin.Open();
            using var command = new NpgsqlCommand($"CREATE DATABASE \"{postgresDatabase}\"", admin);
            command.ExecuteNonQuery();
            postgresCreated = true;
            connection = new NpgsqlConnectionStringBuilder(postgresAdminConnection) { Database = postgresDatabase }.ConnectionString;
            provider = "Postgres";
        }
        builder.UseEnvironment("Testing");
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "Test-only-signing-key-never-use-in-production-123456789",
            ["Jwt:Issuer"] = "CampusFix",
            ["Jwt:Audience"] = "CampusFix",
            ["DbProvider"] = provider,
            ["ConnectionStrings:CampusFix"] = connection,
            ["Seed:StaffEmail"] = StaffEmail,
            ["Seed:StaffPassword"] = StaffPassword,
            ["Logging:LogLevel:Default"] = "Warning"
        };
        foreach (var setting in settings) builder.UseSetting(setting.Key, setting.Value);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        if (postgresCreated)
        {
            NpgsqlConnection.ClearAllPools();
            using var admin = new NpgsqlConnection(postgresAdminConnection);
            admin.Open();
            using var command = new NpgsqlCommand($"DROP DATABASE \"{postgresDatabase}\" WITH (FORCE)", admin);
            command.ExecuteNonQuery();
        }
        else if (File.Exists(sqliteFile)) File.Delete(sqliteFile);
    }
}
