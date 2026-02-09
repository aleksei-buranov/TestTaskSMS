using Newtonsoft.Json;

namespace FoodServerClient.Http.Transport;

internal sealed class ApiRequest<TParams>
{
    [JsonProperty(nameof(Command))] public string Command { get; }

    [JsonProperty(nameof(CommandParameters))]
    public TParams CommandParameters { get; }

    public ApiRequest(string command, TParams commandParameters)
    {
        Command = command;
        CommandParameters = commandParameters;
    }
}