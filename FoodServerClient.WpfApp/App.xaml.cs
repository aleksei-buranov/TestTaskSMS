using System.IO;
using System.Text;
using System.Windows;
using FoodServerClient.WpfApp.Options;
using FoodServerClient.WpfApp.Services;
using FoodServerClient.WpfApp.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace FoodServerClient.WpfApp;

public partial class App : Application
{
    public static IHost HostInstance { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var builder = Host.CreateApplicationBuilder();

        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, false);

        Directory.CreateDirectory("logs");
        var logPath = Path.Combine("logs", $"test-sms-wpf-app-{DateTime.Now:yyyyMMdd}.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logPath,
                shared: true,
                encoding: new UTF8Encoding(true),
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Services.AddSingleton(Log.Logger);

        builder.Services.Configure<EnvVarConfig>(builder.Configuration);

        builder.Services.AddSingleton(sp =>
        {
            var cfg = sp.GetRequiredService<IOptions<EnvVarConfig>>().Value;
            var target = string.Equals(cfg.Storage.Target, "Machine", StringComparison.OrdinalIgnoreCase)
                ? EnvironmentVariableTarget.Machine
                : EnvironmentVariableTarget.User;

            return new EnvVarService(sp.GetRequiredService<ILogger>(), target);
        });

        builder.Services.AddSingleton<MainViewModel>();

        builder.Services.AddSingleton<MainWindow>(sp =>
        {
            var vm = sp.GetRequiredService<MainViewModel>();
            var wnd = new MainWindow { DataContext = vm };
            return wnd;
        });

        HostInstance = builder.Build();

        var mainWindow = HostInstance.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        Log.Information("WPF app started");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            Log.Information("WPF app stopping...");
            await HostInstance.StopAsync(TimeSpan.FromSeconds(2));
            HostInstance.Dispose();
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            base.OnExit(e);
        }
    }
}