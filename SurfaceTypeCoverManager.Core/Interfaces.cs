using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.Core.Interfaces
{
    public interface IDeviceWatcherService
    {
        event EventHandler<DeviceConnectionEvent>? DeviceStateChanged;
        SurfaceDeviceDetails CurrentDevice { get; }
        void StartMonitoring(IntPtr windowHandle);
        void StopMonitoring();
        Task RefreshAsync();
    }

    public interface IHidService
    {
        IReadOnlyList<HidDeviceInfo> EnumerateHidDevices();
        HidDeviceInfo? GetDeviceDetails(string devicePath);
    }

    public interface ISurfaceService
    {
        Task<SurfaceDeviceDetails> DetectSurfaceDeviceAsync();
        IReadOnlyList<ServiceStatusInfo> GetSurfaceServicesStatus();
        IReadOnlyList<DriverInfo> GetSurfaceDrivers();
        IReadOnlyList<SetupApiPropertyItem> GetSetupApiProperties();
    }

    public interface IKeyboardService
    {
        event EventHandler<KeyStrokeInfo>? KeyPressed;
        event EventHandler<KeyStrokeInfo>? KeyReleased;
        IReadOnlySet<string> CurrentlyPressedKeys { get; }
        string CurrentKey { get; }
        string ModifierState { get; }
        double EstimatedLatencyMs { get; }
        int PollingRateHz { get; }
        bool IsGhostingDetected { get; }
        IReadOnlyList<string> StuckKeys { get; }
        int MaxRolloverDetected { get; }
        void StartKeyHook(IntPtr windowHandle);
        void StopKeyHook();
        void ProcessKeyDown(int vkCode);
        void ProcessKeyUp(int vkCode);
    }

    public interface ITouchpadService
    {
        event EventHandler? TouchpadActivityDetected;
        TouchpadInfo GetTouchpadInfo();
        void RegisterTouchpadActivity();
    }

    public interface IDiagnosticsService
    {
        Task<DiagnosticReport> RunDiagnosticsAsync();
    }

    public interface IEventLogService
    {
        event EventHandler<LogEventEntry>? EventLogged;
        IReadOnlyList<LogEventEntry> RecentLogs { get; }
        Task<IReadOnlyList<LogEventEntry>> FetchSystemPnpEventsAsync(int maxEvents = 50);
        void StartListening();
        void StopListening();
        void AddLog(string source, string message, Enums.DiagnosticLevel level = Enums.DiagnosticLevel.Info);
    }

    public interface IDatabaseService
    {
        Task InitializeAsync();
        Task SaveConnectionEventAsync(DeviceConnectionEvent evt);
        Task<IReadOnlyList<DeviceConnectionEvent>> GetConnectionHistoryAsync();
        Task SaveTypingTestAsync(TypingTestResult result);
        Task<IReadOnlyList<TypingTestResult>> GetTypingHistoryAsync();
        Task SaveDiagnosticReportAsync(DiagnosticReport report);
        Task<IReadOnlyList<DiagnosticReportSummary>> GetDiagnosticHistoryAsync();
    }

    public interface INotificationService
    {
        void ShowNotification(string title, string message);
    }

    public interface IReportExporterService
    {
        Task ExportToJsonAsync(DiagnosticReport report, string filePath);
        Task ExportToHtmlAsync(DiagnosticReport report, string filePath);
        Task ExportToMarkdownAsync(DiagnosticReport report, string filePath);
        Task<string> ExportZipBundleAsync(DiagnosticReport report, SurfaceDeviceDetails device, IEnumerable<DeviceConnectionEvent> history, string outputDirectory);
    }

    public interface ISettingsService
    {
        AppSettings CurrentSettings { get; }
        Task SaveSettingsAsync(AppSettings settings);
        event EventHandler<AppSettings>? SettingsChanged;
    }
}
