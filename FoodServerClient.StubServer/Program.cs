using System.Globalization;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string EndpointPath = "/api";
const string User = "user";
const string Pass = "pass";

var menu = new[]
{
    new
    {
        Id = "5979224",
        Article = "A1004292",
        Name = "Каша гречневая",
        Price = 50,
        IsWeighted = false,
        FullPath = "ПРОИЗВОДСТВО\\Гарниры",
        Barcodes = new[] { "57890975627974236429" }
    },
    new
    {
        Id = "9084246",
        Article = "A1004293",
        Name = "Конфеты Коровка",
        Price = 300,
        IsWeighted = true,
        FullPath = "ДЕСЕРТЫ\\Развес",
        Barcodes = Array.Empty<string>()
    }
};

app.MapPost(EndpointPath, async (HttpRequest req) =>
{
    if (!TryCheckBasicAuth(req, User, Pass))

        return Results.Json(new
        {
            Command = "",
            Success = false,
            ErrorMessage = "Unauthorized"
        });

    JsonDocument doc;
    try
    {
        doc = await JsonDocument.ParseAsync(req.Body);
    }
    catch
    {
        return Results.Json(new
        {
            Command = "",
            Success = false,
            ErrorMessage = "Invalid JSON"
        });
    }

    var root = doc.RootElement;

    if (!root.TryGetProperty("Command", out var cmdEl) || cmdEl.ValueKind != JsonValueKind.String)
        return Results.Json(new
        {
            Command = "",
            Success = false,
            ErrorMessage = "Missing Command"
        });

    var command = cmdEl.GetString() ?? "";

    if (command == "GetMenu")
    {
        var withPrice = true;
        if (root.TryGetProperty("CommandParameters", out var p) &&
            p.TryGetProperty("WithPrice", out var wp) &&
            wp.ValueKind is JsonValueKind.True or JsonValueKind.False)
            withPrice = wp.GetBoolean();

        var menuResult = menu.Select(m => new
        {
            m.Id,
            m.Article,
            m.Name,
            Price = withPrice ? (double?)m.Price : null,
            m.IsWeighted,
            m.FullPath,
            m.Barcodes
        });


        return Results.Json(new
        {
            Command = "GetMenu",
            Success = true,
            ErrorMessage = "",
            Data = new
            {
                MenuItems = menuResult
            }
        });
    }

    if (command == "SendOrder")
    {
        if (!root.TryGetProperty("CommandParameters", out var cp))
            return Results.Json(new
            {
                Command = "SendOrder",
                Success = false,
                ErrorMessage = "Missing CommandParameters"
            });

        if (!cp.TryGetProperty("OrderId", out var orderIdEl) || orderIdEl.ValueKind != JsonValueKind.String)
            return Results.Json(new
            {
                Command = "SendOrder",
                Success = false,
                ErrorMessage = "Missing OrderId"
            });

        if (!cp.TryGetProperty("MenuItems", out var itemsEl) || itemsEl.ValueKind != JsonValueKind.Array)
            return Results.Json(new
            {
                Command = "SendOrder",
                Success = false,
                ErrorMessage = "Missing MenuItems"
            });

        var allowedIds = menu.Select(m => m.Id).ToHashSet(StringComparer.Ordinal);

        foreach (var it in itemsEl.EnumerateArray())
        {
            if (!it.TryGetProperty("Id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
                return Results.Json(new { Command = "SendOrder", Success = false, ErrorMessage = "Item without Id" });

            var id = idEl.GetString() ?? "";

            if (!allowedIds.Contains(id))
                return Results.Json(new { Command = "SendOrder", Success = false, ErrorMessage = $"Unknown Id: {id}" });

            if (!it.TryGetProperty("Quantity", out var qEl) || qEl.ValueKind != JsonValueKind.String)
                return Results.Json(new
                    { Command = "SendOrder", Success = false, ErrorMessage = $"Item {id}: Quantity must be string" });


            if (!double.TryParse(qEl.GetString(), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var q) || q <= 0)
                return Results.Json(new
                    { Command = "SendOrder", Success = false, ErrorMessage = $"Item {id}: invalid quantity" });
        }

        if (string.Equals(orderIdEl.GetString(), "FAIL", StringComparison.OrdinalIgnoreCase))
            return Results.Json(new
            {
                Command = "SendOrder",
                Success = false,
                ErrorMessage = "Forced failure"
            });

        return Results.Json(new
        {
            Command = "SendOrder",
            Success = true,
            ErrorMessage = ""
        });
    }

    return Results.Json(new
    {
        Command = command,
        Success = false,
        ErrorMessage = $"Unknown Command: {command}"
    });
});

app.Run("http://localhost:5057");


static bool TryCheckBasicAuth(HttpRequest req, string user, string pass)
{
    if (!req.Headers.TryGetValue("Authorization", out var auth))
        return false;

    var header = auth.ToString();
    if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        return false;

    var encoded = header["Basic ".Length..].Trim();
    string decoded;
    try
    {
        decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
    }
    catch
    {
        return false;
    }

    // формат user:pass
    var idx = decoded.IndexOf(':');
    if (idx <= 0) return false;

    var u = decoded[..idx];
    var p = decoded[(idx + 1)..];

    return u == user && p == pass;
}
