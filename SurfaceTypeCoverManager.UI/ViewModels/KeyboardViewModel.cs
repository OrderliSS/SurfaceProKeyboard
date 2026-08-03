using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class KeyboardViewModel : ObservableObject
    {
        private readonly IKeyboardService _keyboardService;

        [ObservableProperty]
        private string _currentKey = "None";

        [ObservableProperty]
        private string _pressedKeysText = "None";

        [ObservableProperty]
        private string _modifierState = "None";

        [ObservableProperty]
        private double _estimatedLatencyMs;

        [ObservableProperty]
        private int _pollingRateHz;

        [ObservableProperty]
        private bool _isGhostingDetected;

        [ObservableProperty]
        private int _maxRollover;

        [ObservableProperty]
        private string _stuckKeysText = "None";

        public ObservableCollection<string> VisualActiveKeys { get; } = new ObservableCollection<string>();

        public KeyboardViewModel(IKeyboardService keyboardService)
        {
            _keyboardService = keyboardService;
            _keyboardService.KeyPressed += OnKeyStroke;
            _keyboardService.KeyReleased += OnKeyStroke;
        }

        private void OnKeyStroke(object? sender, KeyStrokeInfo e)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                CurrentKey = _keyboardService.CurrentKey;
                ModifierState = _keyboardService.ModifierState;
                EstimatedLatencyMs = _keyboardService.EstimatedLatencyMs;
                PollingRateHz = _keyboardService.PollingRateHz;
                IsGhostingDetected = _keyboardService.IsGhostingDetected;
                MaxRollover = _keyboardService.MaxRolloverDetected;

                var pressed = _keyboardService.CurrentlyPressedKeys;
                PressedKeysText = pressed != null && pressed.Count > 0 ? string.Join(", ", pressed) : "None";

                VisualActiveKeys.Clear();
                if (pressed != null)
                {
                    foreach (var k in pressed)
                    {
                        VisualActiveKeys.Add(k.ToUpperInvariant());
                    }
                }

                var stuck = _keyboardService.StuckKeys;
                StuckKeysText = stuck != null && stuck.Count > 0 ? string.Join(", ", stuck) : "None";
            });
        }
    }
}
