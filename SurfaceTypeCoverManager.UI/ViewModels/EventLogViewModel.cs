using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class EventLogViewModel : ObservableObject
    {
        private readonly IEventLogService _eventLogService;

        [ObservableProperty]
        private string _searchQuery = "";

        public ObservableCollection<LogEventEntry> LogEntries { get; } = new ObservableCollection<LogEventEntry>();

        public EventLogViewModel(IEventLogService eventLogService)
        {
            _eventLogService = eventLogService;
            _eventLogService.EventLogged += (s, e) => AddLogEntry(e);
            _ = LoadInitialLogsAsync();
        }

        partial void OnSearchQueryChanged(string value)
        {
            RefreshLogView();
        }

        [RelayCommand]
        private async Task FetchSystemLogsAsync()
        {
            var pnpLogs = await _eventLogService.FetchSystemPnpEventsAsync(50);
            foreach (var log in pnpLogs)
            {
                AddLogEntry(log);
            }
        }

        private async Task LoadInitialLogsAsync()
        {
            await FetchSystemLogsAsync();
            RefreshLogView();
        }

        private void AddLogEntry(LogEventEntry entry)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                if (MatchesFilter(entry))
                {
                    LogEntries.Insert(0, entry);
                    if (LogEntries.Count > 300) LogEntries.RemoveAt(LogEntries.Count - 1);
                }
            });
        }

        private void RefreshLogView()
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                LogEntries.Clear();
                var all = _eventLogService.RecentLogs;
                if (all != null)
                {
                    foreach (var item in all.Where(MatchesFilter).OrderByDescending(x => x.Timestamp))
                    {
                        LogEntries.Add(item);
                    }
                }
            });
        }

        private bool MatchesFilter(LogEventEntry entry)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return true;
            return (entry.Message ?? "").Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                   (entry.Source ?? "").Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
        }
    }
}
