using FoodServerClient.Abstractions.Models;
using FoodServerClient.Http.Transport;

namespace FoodServerClient.Http.Mapping;

internal static class MenuItemMapper
{
    public static IReadOnlyList<MenuItem> ToDomain(IEnumerable<MenuItemDto> items)
    {
        return items.Select(ToDomain).ToList();
    }

    public static MenuItem ToDomain(MenuItemDto dto)
    {
        return new MenuItem
        {
            Id = dto.Id,
            Article = dto.Article,
            Name = dto.Name,
            Price = dto.Price,
            IsWeighted = dto.IsWeighted,
            FullPath = dto.FullPath,
            Barcodes = dto.Barcodes
        };
    }
}