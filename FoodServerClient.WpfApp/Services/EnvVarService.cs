using Serilog;

namespace FoodServerClient.WpfApp.Services;

public sealed class EnvVarService
{
    private readonly ILogger _log;
    private readonly EnvironmentVariableTarget _target;

    public EnvVarService(ILogger log, EnvironmentVariableTarget target)
    {
        _log = log;
        _target = target;
    }

    public string? Get(string name)
    {
        return Environment.GetEnvironmentVariable(name, _target);
    }

    public string EnsureExists(string name, string defaultValue)
    {
        var current = Get(name);
        if (current is not null)
            return current;

        Set(name, defaultValue, "initialized (default)");
        return defaultValue;
    }

    public void Set(string name, string value, string reason = "changed")
    {
        var before = Get(name);

        Environment.SetEnvironmentVariable(name, value, _target);

        _log.Information("ENV[{Target}] '{Name}' {Reason}. BeforeLen={BeforeLen} AfterLen={AfterLen}",
            _target, name, reason, before?.Length ?? 0, value?.Length ?? 0);
    }
}