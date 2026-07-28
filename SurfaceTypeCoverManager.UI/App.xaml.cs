using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Services.Services;
using SurfaceTypeCoverManager.UI.ViewModels;

namespace SurfaceTypeCoverManager.UI
{
    public partial class App : Application
    {
        private IHost? _host;

        public static new App? Current => Application.Current as App;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register Services
                    services.AddSingleton<IHidService, HidService>();
                    services.AddSingleton<ISurfaceService, SurfaceService>();
                    services.AddSingleton<IDatabaseService, DatabaseService>();
                    services.AddSingleton<IEventLogService, EventLogService>();
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<IReportExporterService, ReportExporterService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<ITouchpadService, TouchpadService>();
                    services.AddSingleton<IDiagnosticsService, DiagnosticsService>();
                    services.AddSingleton<IDeviceWatcherService, DeviceWatcherService>();
                    services.AddSingleton<IKeyboardService, KeyboardService>();

                    // Register ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<DashboardViewModel>();
                    services.AddSingleton<KeyboardViewModel>();
                    services.AddSingleton<TypingTestViewModel>();
                    services.AddSingleton<ConnectionMonitorViewModel>();
                    services.AddSingleton<TouchpadViewModel>();
                    services.AddSingleton<DiagnosticsViewModel>();
                    services.AddSingleton<DeviceInfoViewModel>();
                    services.AddSingleton<EventLogViewModel>();
                    services.AddSingleton<HistoryViewModel>();
                    services.AddSingleton<SettingsViewModel>();

                    // Register Windows
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            var db = _host.Services.GetRequiredService<IDatabaseService>();
            await db.InitializeAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
