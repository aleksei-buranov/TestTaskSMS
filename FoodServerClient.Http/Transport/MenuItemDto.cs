using Newtonsoft.Json;

namespace FoodServerClient.Http.Transport;

internal sealed class MenuItemDto
{
    [JsonProperty(nameof(Id))] public string Id { get; set; } = "";

    [JsonProperty(nameof(Article))] public string Article { get; set; } = "";

    [JsonProperty(nameof(Name))] public string Name { get; set; } = "";

    [JsonProperty(nameof(Price))] public double? Price { get; set; }

    [JsonProperty(nameof(IsWeighted))] public bool IsWeighted { get; set; }

    [JsonProperty(nameof(FullPath))] public string FullPath { get; set; } = "";

    [JsonProperty(nameof(Barcodes))] public List<string> Barcodes { get; set; } = [];
}