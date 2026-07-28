using System;
using Microsoft.Win32;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class TouchpadService : ITouchpadService
    {
        private DateTime? _lastActivity;
        public event EventHandler? TouchpadActivityDetected;

        public TouchpadInfo GetTouchpadInfo()
        {
            var info = new TouchpadInfo
            {
                IsEnabled = IsTouchpadEnabledInRegistry(),
                HasPrecisionTouchpadSupport = CheckPrecisionTouchpadSupport(),
                HasGestureSupport = true,
                LastActivity = _lastActivity,
                ActivityDetails = _lastActivity.HasValue ? $"Active at {_lastActivity.Value:HH:mm:ss}" : "No recent activity recorded"
            };

            return info;
        }

        public void RegisterTouchpadActivity()
        {
            _lastActivity = DateTime.Now;
            TouchpadActivityDetected?.Invoke(this, EventArgs.Empty);
        }

        private static bool IsTouchpadEnabledInRegistry()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad");
                if (key != null)
                {
                    object? val = key.GetValue("TouchPadOff");
                    if (val is int intVal)
                    {
                        return intVal == 0;
                    }
                }
            }
            catch
            {
                // Fallback graceful
            }
            return true;
        }

        private static bool CheckPrecisionTouchpadSupport()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad");
                return key != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
