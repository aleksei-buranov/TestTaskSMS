using FoodServerClient.Abstractions.Models;

namespace FoodServerClient.Grpc.Mapping;

internal static class MenuItemMapper
{
    public static IReadOnlyList<MenuItem> ToDomain(IEnumerable<Sms.Test.MenuItem> items)
    {
        return items.Select(ToDomain).ToList();
    }

    public static MenuItem ToDomain(Sms.Test.MenuItem dto)
    {
        return new MenuItem
        {
            Id = dto.Id,
            Article = dto.Article,
            Name = dto.Name,
            Price = dto.Price,
            IsWeighted = dto.IsWeighted,
            FullPath = dto.FullPath,
            Barcodes = dto.Barcodes.ToArray()
        };
    }
}