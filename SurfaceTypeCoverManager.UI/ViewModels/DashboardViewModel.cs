using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IDeviceWatcherService _deviceWatcher;
        private readonly DispatcherTimer _timer;

        [ObservableProperty]
        private SurfaceDeviceDetails _device = new SurfaceDeviceDetails();

        [ObservableProperty]
        private string _connectionDurationText = "00:00:00";

        public DashboardViewModel(IDeviceWatcherService deviceWatcher)
        {
            _deviceWatcher = deviceWatcher;
            _deviceWatcher.DeviceStateChanged += (s, e) => UpdateDevice();
            UpdateDevice();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateDuration();
            _timer.Start();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Refresh()
        {
            await _deviceWatcher.RefreshAsync();
            UpdateDevice();
        }

        private void UpdateDevice()
        {
            Device = _deviceWatcher.CurrentDevice;
            UpdateDuration();
        }

        private void UpdateDuration()
        {
            var dur = Device.ConnectionDuration;
            ConnectionDurationText = $"{dur.Hours:D2}:{dur.Minutes:D2}:{dur.Seconds:D2}";
        }
    }
}
