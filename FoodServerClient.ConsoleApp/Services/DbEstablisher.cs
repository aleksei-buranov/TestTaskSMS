using FoodServerClient.ConsoleApp.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

namespace FoodServerClient.ConsoleApp.Services;

public sealed class DbEstablisher
{
    private readonly PostgresOptions _opt;
    private readonly ILogger _log;

    public DbEstablisher(IOptions<PostgresOptions> opt, ILogger log)
    {
        _opt = opt.Value;
        _log = log;
    }

    public string GetAppConnectionString()
    {
        return string.Format(_opt.AppConnectionTemplate, _opt.Database);
    }

    public async Task CheckDatabaseAndTableExistAsync(CancellationToken ct = default)
    {
        await CheckDatabaseExistsAsync(ct);
        await CheckTableExistsAsync(GetAppConnectionString(), ct);
    }

    private async Task CheckDatabaseExistsAsync(CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_opt.AdminConnection);
        await conn.OpenAsync(ct);

        await using (var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name;", conn))
        {
            cmd.Parameters.AddWithValue("name", _opt.Database);
            var exists = await cmd.ExecuteScalarAsync(ct);
            if (exists != null)
            {
                _log.Information("Database '{Db}' already exists.", _opt.Database);
                return;
            }
        }

        var safeName = _opt.Database.Replace("\"", "\"\"");
        await using (var cmd = new NpgsqlCommand($@"CREATE DATABASE ""{safeName}"";", conn))
        {
            await cmd.ExecuteNonQueryAsync(ct);
            _log.Information("Database '{Db}' created.", _opt.Database);
        }
    }

    private async Task CheckTableExistsAsync(string appConnString, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(appConnString);
        await conn.OpenAsync(ct);

        var sql = """
                  CREATE TABLE IF NOT EXISTS menu_items (
                      id          TEXT PRIMARY KEY,
                      article     TEXT NOT NULL,
                      name        TEXT NOT NULL,
                      price       NUMERIC NULL,
                      is_weighted BOOLEAN NOT NULL,
                      full_path   TEXT NOT NULL,
                      barcodes    TEXT[] NOT NULL DEFAULT '{}',
                      updated_at  TIMESTAMPTZ NOT NULL DEFAULT now()
                  );
                  """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);

        _log.Information("Table menu_items is checked for existence.");
    }
}