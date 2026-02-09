using Newtonsoft.Json;

namespace FoodServerClient.Http.Transport;

internal sealed class SendOrderParams
{
    [JsonProperty(nameof(OrderId))] public string OrderId { get; set; } = "";

    [JsonProperty(nameof(MenuItems))] public List<SendOrderItemDto> MenuItems { get; set; } = new();
}