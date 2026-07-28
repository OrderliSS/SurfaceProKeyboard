using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class ConnectionMonitorViewModel : ObservableObject
    {
        private readonly IDeviceWatcherService _deviceWatcher;
        private readonly IDatabaseService _databaseService;

        public ObservableCollection<DeviceConnectionEvent> ConnectionEvents { get; } = new ObservableCollection<DeviceConnectionEvent>();

        [ObservableProperty]
        private int _totalArrivals;

        [ObservableProperty]
        private int _totalRemovals;

        [ObservableProperty]
        private int _totalReconnects;

        public ConnectionMonitorViewModel(IDeviceWatcherService deviceWatcher, IDatabaseService databaseService)
        {
            _deviceWatcher = deviceWatcher;
            _databaseService = databaseService;
            _deviceWatcher.DeviceStateChanged += (s, e) => _ = LoadHistoryAsync();
            _ = LoadHistoryAsync();
        }

        [RelayCommand]
        private async Task LoadHistoryAsync()
        {
            var history = await _databaseService.GetConnectionHistoryAsync();
            ConnectionEvents.Clear();

            int arrivals = 0, removals = 0, reconnects = 0;

            foreach (var evt in history)
            {
                ConnectionEvents.Add(evt);
                if (evt.EventType.Equals("Arrival", StringComparison.OrdinalIgnoreCase)) arrivals++;
                else if (evt.EventType.Equals("Removal", StringComparison.OrdinalIgnoreCase)) removals++;
                else if (evt.EventType.Equals("Reconnect", StringComparison.OrdinalIgnoreCase)) reconnects++;
            }

            TotalArrivals = arrivals;
            TotalRemovals = removals;
            TotalReconnects = reconnects;
        }
    }
}
