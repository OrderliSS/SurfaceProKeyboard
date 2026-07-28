using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class DiagnosticsViewModel : ObservableObject
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly IReportExporterService _reportExporter;
        private readonly INotificationService _notificationService;
        private readonly IDeviceWatcherService _deviceWatcher;
        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private DiagnosticReport _report = new DiagnosticReport();

        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private string _statusMessage = "Ready to run system diagnostic health scan.";

        public ObservableCollection<string> HealthCheckItems { get; } = new ObservableCollection<string>();

        public DiagnosticsViewModel(
            IDiagnosticsService diagnosticsService,
            IReportExporterService reportExporter,
            INotificationService notificationService,
            IDeviceWatcherService deviceWatcher,
            IDatabaseService databaseService)
        {
            _diagnosticsService = diagnosticsService;
            _reportExporter = reportExporter;
            _notificationService = notificationService;
            _deviceWatcher = deviceWatcher;
            _databaseService = databaseService;
        }

        [RelayCommand]
        private async Task RunDiagnosticsAsync()
        {
            IsRunning = true;
            StatusMessage = "Running Surface diagnostic tests...";
            HealthCheckItems.Clear();

            try
            {
                Report = await _diagnosticsService.RunDiagnosticsAsync();
                foreach (var h in Report.HealthCheckResults)
                {
                    HealthCheckItems.Add(h);
                }

                StatusMessage = $"Scan Complete! Overall Health: {(Report.IsOverallHealthy ? "Healthy" : "Warnings Detected")}";
                _notificationService.ShowNotification("Surface Diagnostics Complete", StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Diagnostic Error: {ex.Message}";
            }
            finally
            {
                IsRunning = false;
            }
        }

        [RelayCommand]
        private async Task ExportJsonAsync()
        {
            if (Report == null) return;
            string exportDir = GetExportDirectory();
            string path = Path.Combine(exportDir, $"Surface_Diagnostic_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            await _reportExporter.ExportToJsonAsync(Report, path);
            StatusMessage = $"Exported JSON report to: {path}";
        }

        [RelayCommand]
        private async Task ExportHtmlAsync()
        {
            if (Report == null) return;
            string exportDir = GetExportDirectory();
            string path = Path.Combine(exportDir, $"Surface_Diagnostic_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            await _reportExporter.ExportToHtmlAsync(Report, path);
            StatusMessage = $"Exported HTML report to: {path}";
        }

        [RelayCommand]
        private async Task ExportZipBundleAsync()
        {
            if (Report == null) return;
            string exportDir = GetExportDirectory();
            var history = await _databaseService.GetConnectionHistoryAsync();
            string zipPath = await _reportExporter.ExportZipBundleAsync(Report, _deviceWatcher.CurrentDevice, history, exportDir);
            StatusMessage = $"Exported ZIP bundle to: {zipPath}";
            _notificationService.ShowNotification("Diagnostic Bundle Exported", $"Package created at: {zipPath}");
        }

        private static string GetExportDirectory()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string target = Path.Combine(desktop, "SurfaceDiagnostics");
            Directory.CreateDirectory(target);
            return target;
        }
    }
}
