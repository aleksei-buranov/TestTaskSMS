using System.Text;
using Serilog;

namespace FoodServerClient.ConsoleApp.Services;

internal sealed class ConsoleToSerilogTextWriter : TextWriter
{
    private readonly ILogger _log;
    private readonly TextWriter _original;

    public ConsoleToSerilogTextWriter(ILogger log, TextWriter original)
    {
        _log = log;
        _original = original;
    }

    public override Encoding Encoding => _original.Encoding;

    public override void Write(string? value)
    {
        _original.Write(value);
        if (!string.IsNullOrWhiteSpace(value))
            _log.Information("{Console}", value.TrimEnd());
    }

    public override void WriteLine(string? value)
    {
        _original.WriteLine(value);
        if (!string.IsNullOrWhiteSpace(value))
            _log.Information("{Console}", value);
    }
}