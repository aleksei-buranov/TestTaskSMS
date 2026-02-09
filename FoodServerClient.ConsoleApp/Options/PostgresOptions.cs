namespace FoodServerClient.ConsoleApp.Options;

public sealed class PostgresOptions
{
    public string AdminConnection { get; set; } = "";
    public string Database { get; set; } = "";
    public string AppConnectionTemplate { get; set; } = "";
}
