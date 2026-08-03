using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Enums;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private AppSettings _settings;

        [ObservableProperty]
        private string _statusMessage = "Settings up to date.";

        public SettingsViewModel(ISettingsService settingsService, INotificationService notificationService)
        {
            _settingsService = settingsService;
            _notificationService = notificationService;
            _settings = _settingsService.CurrentSettings ?? new AppSettings();
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            await _settingsService.SaveSettingsAsync(Settings);
            StatusMessage = "Settings saved successfully!";
            _notificationService.ShowNotification("Settings Updated", "Your preferences have been saved.");
        }

        [RelayCommand]
        private void SetDarkTheme()
        {
            Settings.Theme = ThemeMode.Dark;
            _ = SaveSettingsAsync();
        }

        [RelayCommand]
        private void SetLightTheme()
        {
            Settings.Theme = ThemeMode.Light;
            _ = SaveSettingsAsync();
        }
    }
}
