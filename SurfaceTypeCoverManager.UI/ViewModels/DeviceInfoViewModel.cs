using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class DeviceInfoViewModel : ObservableObject
    {
        private readonly ISurfaceService _surfaceService;
        private List<SetupApiPropertyItem> _allItems = new List<SetupApiPropertyItem>();

        [ObservableProperty]
        private string _searchFilter = "";

        public ObservableCollection<SetupApiPropertyItem> DeviceProperties { get; } = new ObservableCollection<SetupApiPropertyItem>();

        public DeviceInfoViewModel(ISurfaceService surfaceService)
        {
            _surfaceService = surfaceService;
            _ = LoadPropertiesAsync();
        }

        partial void OnSearchFilterChanged(string value)
        {
            FilterProperties();
        }

        [RelayCommand]
        private async Task LoadPropertiesAsync()
        {
            _allItems = await Task.Run(() => _surfaceService.GetSetupApiProperties().ToList());
            App.Current?.Dispatcher?.Invoke(() => FilterProperties());
        }

        private void FilterProperties()
        {
            DeviceProperties.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchFilter)
                ? _allItems
                : _allItems.Where(p => (p.PropertyName ?? "").Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)
                                     || (p.PropertyValue ?? "").Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)
                                     || (p.Category ?? "").Contains(SearchFilter, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered)
            {
                DeviceProperties.Add(item);
            }
        }
    }
}
