namespace FoodServerClient.Abstractions;

public sealed class ApiErrorException(string command, string message) : Exception(message)
{
    public string Command { get; } = command;
}