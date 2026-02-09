namespace FoodServerClient.WpfApp.Options;

public sealed class EnvVarConfig
{
    public string[] EnvironmentVariables { get; set; } = [];
    public StorageConfig Storage { get; set; } = new();

    public sealed class StorageConfig
    {
        public string Target { get; set; } = "User";
    }
}