using Newtonsoft.Json;

namespace FoodServerClient.Http.Transport;

internal sealed class ApiResponse<TData>
{
    [JsonProperty(nameof(Command))] public string Command { get; set; } = "";

    [JsonProperty(nameof(Success))] public bool Success { get; set; }

    [JsonProperty(nameof(ErrorMessage))] public string ErrorMessage { get; set; } = "";

    [JsonProperty(nameof(Data))] public TData? Data { get; set; }
}