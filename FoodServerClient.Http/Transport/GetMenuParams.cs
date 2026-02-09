using Newtonsoft.Json;

namespace FoodServerClient.Http.Transport;

internal sealed class GetMenuParams
{
    [JsonProperty(nameof(WithPrice))] public bool WithPrice { get; set; }
}