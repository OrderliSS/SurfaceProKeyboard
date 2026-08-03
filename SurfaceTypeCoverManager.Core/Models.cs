using System;
using System.Collections.Generic;
using SurfaceTypeCoverManager.Core.Enums;

namespace SurfaceTypeCoverManager.Core.Models
{
    public class SurfaceDeviceDetails
    {
        public bool IsConnected { get; set; }
        public string StatusText => IsConnected ? "Connected" : "Disconnected";
        public string ModelName { get; set; } = "Unavailable";
        public string HostModel { get; set; } = "Unavailable";
        public string HardwareId { get; set; } = "Unavailable";
        public string ConnectionType { get; set; } = "Unknown";
        public string VendorId { get; set; } = "Unavailable";
        public string ProductId { get; set; } = "Unavailable";
        public string FirmwareVersion { get; set; } = "Unavailable";
        public string SerialNumber { get; set; } = "Unavailable";
        public DateTime? ConnectionTime { get; set; }
        public int ReconnectCount { get; set; }
        public DateTime? LastDisconnectTime { get; set; }
        public TimeSpan ConnectionDuration => ConnectionTime.HasValue && IsConnected ? DateTime.Now - ConnectionTime.Value : TimeSpan.Zero;
        public LockState BacklightStatus { get; set; } = LockState.Unknown;
        public LockState CapsLock { get; set; } = LockState.Unknown;
        public LockState NumLock { get; set; } = LockState.Unknown;
        public LockState FnLock { get; set; } = LockState.Unknown;
        public string TouchpadStatus { get; set; } = "Unavailable";
        public bool TouchpadGesturesAvailable { get; set; }
        public string BatteryStatus { get; set; } = "Unavailable";

        // SetupAPI Extended Metadata
        public string FriendlyName { get; set; } = "Unavailable";
        public string Manufacturer { get; set; } = "Unavailable";
        public string ClassGuid { get; set; } = "Unavailable";
        public string InstanceId { get; set; } = "Unavailable";
        public string ContainerId { get; set; } = "Unavailable";
        public string LocationPath { get; set; } = "Unavailable";
        public string BusType { get; set; } = "Unavailable";
        public string PowerState { get; set; } = "Unavailable";
        public string RemovalPolicy { get; set; } = "Unavailable";
        public string DeviceCapabilities { get; set; } = "Unavailable";
        public string CompatibleIds { get; set; } = "Unavailable";
    }

    public class KeyStrokeInfo
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string KeyName { get; set; } = string.Empty;
        public int VirtualKey { get; set; }
        public bool IsDown { get; set; }
        public string Modifiers { get; set; } = string.Empty;
        public double LatencyMs { get; set; }
        public bool IsGhosted { get; set; }
        public bool IsStuck { get; set; }
    }

    public class TypingTestResult
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public double DurationSeconds { get; set; }
        public double Wpm { get; set; }
        public double AccuracyPercent { get; set; }
        public int TotalChars { get; set; }
        public int Errors { get; set; }
        public int DuplicatePresses { get; set; }
        public int DroppedEvents { get; set; }
        public double AverageLatencyMs { get; set; }
        public double MaxLatencyMs { get; set; }
        public string LatencyDataPointsJson { get; set; } = "[]";
    }

    public class DeviceConnectionEvent
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string EventType { get; set; } = "Unknown";
        public string DeviceName { get; set; } = "Unknown";
        public string HardwareId { get; set; } = "Unavailable";
        public string Details { get; set; } = string.Empty;
    }

    public class SetupApiPropertyItem
    {
        public string Category { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyValue { get; set; } = "Unavailable";
    }

    public class HidDeviceInfo
    {
        public string DevicePath { get; set; } = string.Empty;
        public ushort VendorId { get; set; }
        public ushort ProductId { get; set; }
        public ushort VersionNumber { get; set; }
        public string Manufacturer { get; set; } = "Unavailable";
        public string Product { get; set; } = "Unavailable";
        public string SerialNumber { get; set; } = "Unavailable";
        public ushort UsagePage { get; set; }
        public ushort Usage { get; set; }
        public int InputReportByteLength { get; set; }
        public int OutputReportByteLength { get; set; }
        public int FeatureReportByteLength { get; set; }
    }

    public class ServiceStatusInfo
    {
        public string ServiceName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Status { get; set; } = "Unknown";
    }

    public class DriverInfo
    {
        public string DriverName { get; set; } = string.Empty;
        public string Version { get; set; } = "Unavailable";
        public string Provider { get; set; } = "Unavailable";
        public string Date { get; set; } = "Unavailable";
    }

    public class DiagnosticReport
    {
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string WindowsVersion { get; set; } = "Unavailable";
        public List<ServiceStatusInfo> SurfaceServices { get; set; } = new();
        public List<HidDeviceInfo> HidDevices { get; set; } = new();
        public List<SetupApiPropertyItem> Keyboards { get; set; } = new();
        public List<SetupApiPropertyItem> SurfaceDevices { get; set; } = new();
        public List<DriverInfo> SurfaceDrivers { get; set; } = new();
        public List<LogEventEntry> RecentPnpEvents { get; set; } = new();
        public List<string> HealthCheckResults { get; set; } = new();
        public bool IsOverallHealthy { get; set; } = true;
    }

    public class DiagnosticReportSummary
    {
        public int Id { get; set; }
        public DateTime GeneratedAt { get; set; }
        public bool IsHealthy { get; set; }
        public int HidDeviceCount { get; set; }
        public int KeyboardCount { get; set; }
        public string SummaryText { get; set; } = string.Empty;
    }

    public class LogEventEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public DiagnosticLevel Level { get; set; } = DiagnosticLevel.Info;
        public string Source { get; set; } = "System";
        public string Message { get; set; } = string.Empty;
    }

    public class TouchpadInfo
    {
        public bool IsEnabled { get; set; }
        public bool HasPrecisionTouchpadSupport { get; set; }
        public bool HasGestureSupport { get; set; }
        public DateTime? LastActivity { get; set; }
        public string ActivityDetails { get; set; } = "No recent activity";
    }

    public class AppSettings
    {
        public ThemeMode Theme { get; set; } = ThemeMode.Dark;
        public bool AutoStart { get; set; }
        public bool MinimizeToTray { get; set; } = true;
        public bool BackgroundMonitoring { get; set; } = true;
        public bool EnableNotifications { get; set; } = true;
        public bool SupportThirdPartyKeyboards { get; set; } = true;
        public int LogRetentionDays { get; set; } = 30;
        public int HistoryRetentionDays { get; set; } = 90;
        public string ExportLocation { get; set; } = string.Empty;
    }
}
