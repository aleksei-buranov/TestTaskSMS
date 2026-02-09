using Newtonsoft.Json;

namespace FoodServerClient.Http.Transport;

internal sealed class SendOrderItemDto
{
    [JsonProperty(nameof(Id))] public string Id { get; set; } = "";
    [JsonProperty(nameof(Quantity))] public string Quantity { get; set; } = "";
}