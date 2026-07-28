using System;
using Microsoft.Toolkit.Uwp.Notifications;
using SurfaceTypeCoverManager.Core.Interfaces;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class NotificationService : INotificationService
    {
        public void ShowNotification(string title, string message)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .Show();
            }
            catch
            {
                // Fallback gracefully if notification is not permitted or fails
            }
        }
    }
}
