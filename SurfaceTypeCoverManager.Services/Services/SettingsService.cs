using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        public AppSettings CurrentSettings { get; private set; } = new AppSettings();

        public event EventHandler<AppSettings>? SettingsChanged;

        public SettingsService()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SurfaceTypeCoverManager");
            Directory.CreateDirectory(appData);
            _settingsFilePath = Path.Combine(appData, "settings.json");
            LoadSettings();
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            CurrentSettings = settings;
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
            SettingsChanged?.Invoke(this, settings);
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                    {
                        CurrentSettings = loaded;
                    }
                }
            }
            catch
            {
                CurrentSettings = new AppSettings();
            }
        }
    }
}
