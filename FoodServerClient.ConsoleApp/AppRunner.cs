using System.Globalization;
using FoodServerClient.Abstractions;
using FoodServerClient.Abstractions.Models;
using FoodServerClient.ConsoleApp.Services;
using Serilog;

namespace FoodServerClient.ConsoleApp;

public sealed class AppRunner
{
    private readonly DbEstablisher _db;
    private readonly MenuRepository _repo;
    private readonly IApiClient _api;
    private readonly ILogger _log;

    public AppRunner(DbEstablisher db, MenuRepository repo, IApiClient api, ILogger log)
    {
        _db = db;
        _repo = repo;
        _api = api;
        _log = log;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        await _db.CheckDatabaseAndTableExistAsync(ct);
        var appConn = _db.GetAppConnectionString();

        IReadOnlyList<MenuItem> menu;
        try
        {
            menu = await _api.GetMenuAsync(true, ct);
        }
        catch (ApiErrorException ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        await _repo.UpsertMenuAsync(appConn, menu, ct);

        foreach (var item in menu)
            Console.WriteLine($"{item.Name} – {item.Article} – {item.Price:0.##}");

        var order = new Order { OrderId = Guid.NewGuid(), Items = [] };

        var menuByArticle = menu.ToDictionary(m => m.Article, StringComparer.OrdinalIgnoreCase);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Введите: Код1:Количество1;Код2:Количество2;...");

            var line = Console.ReadLine()?.Trim() ?? "";

            if (TryParseOrderLine(line, menuByArticle, out var items, out var error))
            {
                order = new Order
                {
                    OrderId = order.OrderId,
                    Items = items
                };

                break;
            }

            Console.WriteLine($"Некорректный ввод: {error}");
            Console.WriteLine("Повторите ввод.");
        }

        try
        {
            await _api.SendOrderAsync(order, ct);
            Console.WriteLine("УСПЕХ");
        }
        catch (ApiErrorException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Unexpected error while sending order");
            Console.WriteLine("Произошла непредвиденная ошибка при отправке заказа.");
        }

        Console.WriteLine("Нажмите любую клавишу...");
        Console.ReadKey();
    }

    private static bool TryParseOrderLine(
        string input,
        Dictionary<string, MenuItem> menuByArticle,
        out IReadOnlyList<OrderItem> items,
        out string error)
    {
        items = [];
        error = "";

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Пустая строка.";
            return false;
        }

        var parts = input.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            error = "Не найдены позиции.";
            return false;
        }

        var list = new List<OrderItem>();

        foreach (var part in parts)
        {
            var pair = part.Split(':', StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
            {
                error = $"Неверный формат элемента '{part}'. Ожидается Код:Количество.";
                return false;
            }

            var article = pair[0];
            var qtyText = pair[1];

            if (!menuByArticle.TryGetValue(article, out var menuItem))
            {
                error = $"Код (артикул) '{article}' не существует.";
                return false;
            }

            if (!TryParsePositiveDouble(qtyText, out var qty))
            {
                error = $"Количество '{qtyText}' некорректно или <= 0 для '{article}'.";
                return false;
            }

            list.Add(new OrderItem { Id = menuItem.Id, Quantity = qty });
        }

        items = list;
        return true;
    }

    private static bool TryParsePositiveDouble(string text, out double value)
    {
        value = 0;

        var normalized = text.Replace(',', '.');

        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            return false;

        if (v <= 0)
            return false;

        value = v;
        return true;
    }
}