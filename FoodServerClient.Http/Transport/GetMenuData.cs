using Newtonsoft.Json;

namespace FoodServerClient.Http.Transport;

internal sealed class GetMenuData
{
    [JsonProperty(nameof(MenuItems))] public List<MenuItemDto> MenuItems { get; set; } = [];
}