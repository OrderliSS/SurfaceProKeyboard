using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService;

        public ObservableCollection<DeviceConnectionEvent> ConnectionHistory { get; } = new ObservableCollection<DeviceConnectionEvent>();
        public ObservableCollection<TypingTestResult> TypingHistory { get; } = new ObservableCollection<TypingTestResult>();
        public ObservableCollection<DiagnosticReportSummary> DiagnosticHistory { get; } = new ObservableCollection<DiagnosticReportSummary>();

        public HistoryViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _ = RefreshHistoryAsync();
        }

        [RelayCommand]
        private async Task RefreshHistoryAsync()
        {
            var connections = await _databaseService.GetConnectionHistoryAsync();
            var typings = await _databaseService.GetTypingHistoryAsync();
            var diagnostics = await _databaseService.GetDiagnosticHistoryAsync();

            App.Current?.Dispatcher?.Invoke(() =>
            {
                ConnectionHistory.Clear();
                foreach (var c in connections) ConnectionHistory.Add(c);

                TypingHistory.Clear();
                foreach (var t in typings) TypingHistory.Add(t);

                DiagnosticHistory.Clear();
                foreach (var d in diagnostics) DiagnosticHistory.Add(d);
            });
        }
    }
}
