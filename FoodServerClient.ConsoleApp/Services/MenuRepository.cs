using FoodServerClient.Abstractions.Models;
using Npgsql;

namespace FoodServerClient.ConsoleApp.Services;

public sealed class MenuRepository
{
    public async Task UpsertMenuAsync(string connectionString, IReadOnlyList<MenuItem> menu,
        CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        await using var tx = await conn.BeginTransactionAsync(ct);

        const string sql = """
                           INSERT INTO menu_items (id, article, name, price, is_weighted, full_path, barcodes, updated_at)
                           VALUES (@id, @article, @name, @price, @is_weighted, @full_path, @barcodes, now())
                           ON CONFLICT (id) DO UPDATE SET
                               article = EXCLUDED.article,
                               name = EXCLUDED.name,
                               price = EXCLUDED.price,
                               is_weighted = EXCLUDED.is_weighted,
                               full_path = EXCLUDED.full_path,
                               barcodes = EXCLUDED.barcodes,
                               updated_at = now();
                           """;

        foreach (var item in menu)
        {
            await using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("id", item.Id);
            cmd.Parameters.AddWithValue("article", item.Article);
            cmd.Parameters.AddWithValue("name", item.Name);
            cmd.Parameters.AddWithValue("price", (object?)item.Price ?? DBNull.Value);
            cmd.Parameters.AddWithValue("is_weighted", item.IsWeighted);
            cmd.Parameters.AddWithValue("full_path", item.FullPath);

            var barcodes = item.Barcodes.ToArray();
            cmd.Parameters.AddWithValue("barcodes", barcodes);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
    }
}