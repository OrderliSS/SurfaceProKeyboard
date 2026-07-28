using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDeviceWatcherService _deviceWatcher;

        public DashboardViewModel DashboardVM { get; }
        public KeyboardViewModel KeyboardVM { get; }
        public TypingTestViewModel TypingTestVM { get; }
        public ConnectionMonitorViewModel ConnectionMonitorVM { get; }
        public TouchpadViewModel TouchpadVM { get; }
        public DiagnosticsViewModel DiagnosticsVM { get; }
        public DeviceInfoViewModel DeviceInfoVM { get; }
        public EventLogViewModel EventLogVM { get; }
        public HistoryViewModel HistoryVM { get; }
        public SettingsViewModel SettingsVM { get; }

        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private SurfaceDeviceDetails _deviceStatus = new SurfaceDeviceDetails();

        public MainViewModel(
            IDeviceWatcherService deviceWatcher,
            DashboardViewModel dashboardVM,
            KeyboardViewModel keyboardVM,
            TypingTestViewModel typingTestVM,
            ConnectionMonitorViewModel connectionMonitorVM,
            TouchpadViewModel touchpadVM,
            DiagnosticsViewModel diagnosticsVM,
            DeviceInfoViewModel deviceInfoVM,
            EventLogViewModel eventLogVM,
            HistoryViewModel historyVM,
            SettingsViewModel settingsVM)
        {
            _deviceWatcher = deviceWatcher;
            DashboardVM = dashboardVM;
            KeyboardVM = keyboardVM;
            TypingTestVM = typingTestVM;
            ConnectionMonitorVM = connectionMonitorVM;
            TouchpadVM = touchpadVM;
            DiagnosticsVM = diagnosticsVM;
            DeviceInfoVM = deviceInfoVM;
            EventLogVM = eventLogVM;
            HistoryVM = historyVM;
            SettingsVM = settingsVM;

            _currentView = DashboardVM;
            _deviceWatcher.DeviceStateChanged += (s, e) => DeviceStatus = _deviceWatcher.CurrentDevice;
            DeviceStatus = _deviceWatcher.CurrentDevice;
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            CurrentView = viewName switch
            {
                "Dashboard" => DashboardVM,
                "Keyboard" => KeyboardVM,
                "TypingTest" => TypingTestVM,
                "ConnectionMonitor" => ConnectionMonitorVM,
                "Touchpad" => TouchpadVM,
                "Diagnostics" => DiagnosticsVM,
                "DeviceInfo" => DeviceInfoVM,
                "EventLog" => EventLogVM,
                "History" => HistoryVM,
                "Settings" => SettingsVM,
                _ => DashboardVM
            };
        }
    }
}
