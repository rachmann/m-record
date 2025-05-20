using m_record.Services;
using m_record.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace m_record
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            // because I'm using RollinfInterval.Day, the log file will result in:
            // Logs/app-20250520.log
            // Logs/app-20250521.log
            // Logs/app-20250522.log

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders(); // Remove default providers
                    builder.AddSerilog();     // Add Serilog as the logging provider
                    builder.AddConsole();     // Log to console (optional)
                })
                .AddSingleton<ScreenCaptureService>() // Register services as singletons or transients as needed
                .AddSingleton<InputLoggingService>()
                .AddSingleton<NotificationService>(sp =>
                {
                    // Provide a default no-op action or a placeholder.
                    // This will be replaced in MainViewModel after construction.
                    return new NotificationService(_ => { });
                })
                .AddTransient<InputHookService>() // Usually transient due to event subscriptions
                .AddTransient<MainViewModel>() // Register ViewModels
                .BuildServiceProvider();

            base.OnStartup(e);
        }
    }
}