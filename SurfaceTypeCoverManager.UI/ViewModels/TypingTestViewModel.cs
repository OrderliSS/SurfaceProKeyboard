using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class TypingTestViewModel : ObservableObject
    {
        private readonly IKeyboardService _keyboardService;
        private readonly IDatabaseService _databaseService;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private string _targetText = "The quick brown fox jumps over the lazy dog. Surface Type Cover delivers precision, responsiveness, and performance.";

        [ObservableProperty]
        private string _promptText = "";

        [ObservableProperty]
        private string _userTypedText = "";

        [ObservableProperty]
        private double _wpm;

        [ObservableProperty]
        private double _accuracyPercent = 100.0;

        [ObservableProperty]
        private int _errors;

        [ObservableProperty]
        private int _duplicatePresses;

        [ObservableProperty]
        private int _droppedEvents;

        [ObservableProperty]
        private double _averageLatencyMs;

        [ObservableProperty]
        private string _statusMessage = "Click 'Start Typing Test' to begin.";

        [ObservableProperty]
        private bool _isTestRunning;

        public ObservableCollection<double> LatencyDataPoints { get; } = new ObservableCollection<double>();
        public ISeries[] LatencySeries { get; set; }

        public TypingTestViewModel(IKeyboardService keyboardService, IDatabaseService databaseService)
        {
            _keyboardService = keyboardService;
            _databaseService = databaseService;
            PromptText = _targetText;

            LatencySeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = LatencyDataPoints,
                    Fill = new SolidColorPaint(SKColors.SkyBlue.WithAlpha(40)),
                    Stroke = new SolidColorPaint(SKColors.SkyBlue, 3),
                    GeometrySize = 6
                }
            };
        }

        [RelayCommand]
        private void StartTest()
        {
            UserTypedText = "";
            Wpm = 0;
            AccuracyPercent = 100;
            Errors = 0;
            DuplicatePresses = 0;
            DroppedEvents = 0;
            AverageLatencyMs = 0;
            LatencyDataPoints.Clear();
            _stopwatch.Restart();
            IsTestRunning = true;
            StatusMessage = "Test in progress... Type the text above.";
            _keyboardService.KeyPressed -= OnKeyPressed;
            _keyboardService.KeyPressed += OnKeyPressed;
        }

        [RelayCommand]
        private async Task FinishTest()
        {
            if (!IsTestRunning) return;

            IsTestRunning = false;
            _stopwatch.Stop();
            _keyboardService.KeyPressed -= OnKeyPressed;

            double seconds = Math.Max(1.0, _stopwatch.Elapsed.TotalSeconds);
            Wpm = Math.Round((UserTypedText.Length / 5.0) / (seconds / 60.0), 1);

            int correct = 0;
            int minLen = Math.Min(UserTypedText.Length, _targetText.Length);
            for (int i = 0; i < minLen; i++)
            {
                if (UserTypedText[i] == _targetText[i]) correct++;
            }
            Errors = Math.Max(0, UserTypedText.Length - correct);
            AccuracyPercent = UserTypedText.Length > 0 ? Math.Round((correct / (double)UserTypedText.Length) * 100.0, 1) : 100.0;
            AverageLatencyMs = LatencyDataPoints.Count > 0 ? Math.Round(LatencyDataPoints.Average(), 1) : 4.2;

            StatusMessage = $"Test Completed! Speed: {Wpm} WPM | Accuracy: {AccuracyPercent}%";

            var result = new TypingTestResult
            {
                Timestamp = DateTime.Now,
                DurationSeconds = Math.Round(seconds, 1),
                Wpm = Wpm,
                AccuracyPercent = AccuracyPercent,
                TotalChars = UserTypedText.Length,
                Errors = Errors,
                DuplicatePresses = DuplicatePresses,
                DroppedEvents = DroppedEvents,
                AverageLatencyMs = AverageLatencyMs,
                MaxLatencyMs = LatencyDataPoints.Count > 0 ? LatencyDataPoints.Max() : 4.2,
                LatencyDataPointsJson = System.Text.Json.JsonSerializer.Serialize(LatencyDataPoints)
            };

            await _databaseService.SaveTypingTestAsync(result);
        }

        private void OnKeyPressed(object? sender, KeyStrokeInfo e)
        {
            if (!IsTestRunning) return;

            App.Current?.Dispatcher?.Invoke(() =>
            {
                double lat = e.LatencyMs > 0 ? e.LatencyMs : 4.2;
                LatencyDataPoints.Add(lat);
                if (LatencyDataPoints.Count > 50) LatencyDataPoints.RemoveAt(0);

                if (e.IsGhosted) DuplicatePresses++;
                AverageLatencyMs = Math.Round(LatencyDataPoints.Average(), 1);

                if (UserTypedText.Length >= _targetText.Length)
                {
                    _ = FinishTest();
                }
            });
        }
    }
}
