using System.Net.Http.Headers;
using System.Text;
using FoodServerClient.Abstractions;
using FoodServerClient.ConsoleApp.Options;
using FoodServerClient.ConsoleApp.Services;
using FoodServerClient.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace FoodServerClient.ConsoleApp;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddJsonFile("appsettings.json", false, false);

        Directory.CreateDirectory("logs");
        var logFilePath = Path.Combine("logs", $"test-sms-console-app-{DateTime.Now:yyyyMMdd}.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logFilePath,
                encoding: new UTF8Encoding(true),
                shared: true,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Services.AddSingleton(Log.Logger);

        Console.SetOut(new ConsoleToSerilogTextWriter(Log.Logger, Console.Out));
        Console.SetError(new ConsoleToSerilogTextWriter(Log.Logger, Console.Error));

        builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
        builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Postgres"));

        builder.Services.AddSingleton<DbEstablisher>();
        builder.Services.AddSingleton<MenuRepository>();

        builder.Services.AddHttpClient<IApiClient, ApiClient>((sp, http) =>
        {
            var api = sp.GetRequiredService<IOptions<ApiOptions>>().Value;

            http.BaseAddress = new Uri(api.EndpointUrl);

            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{api.Username}:{api.Password}"));
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", token);

            http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            http.Timeout = TimeSpan.FromSeconds(30);
        });


        builder.Services.AddSingleton<AppRunner>();

        using var host = builder.Build();

        try
        {
            var runner = host.Services.GetRequiredService<AppRunner>();
            await runner.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
            return -1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
