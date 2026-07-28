using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.UI.ViewModels
{
    public partial class TouchpadViewModel : ObservableObject
    {
        private readonly ITouchpadService _touchpadService;

        [ObservableProperty]
        private TouchpadInfo _touchpad = new TouchpadInfo();

        public TouchpadViewModel(ITouchpadService touchpadService)
        {
            _touchpadService = touchpadService;
            _touchpadService.TouchpadActivityDetected += (s, e) => Refresh();
            Refresh();
        }

        [RelayCommand]
        private void Refresh()
        {
            Touchpad = _touchpadService.GetTouchpadInfo();
        }

        [RelayCommand]
        private void SimulateActivity()
        {
            _touchpadService.RegisterTouchpadActivity();
            Refresh();
        }
    }
}
