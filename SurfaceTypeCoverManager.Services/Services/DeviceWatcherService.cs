using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Interop;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;
using SurfaceTypeCoverManager.Services.Interop;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class DeviceWatcherService : IDeviceWatcherService
    {
        private readonly ISurfaceService _surfaceService;
        private readonly IDatabaseService _databaseService;
        private readonly INotificationService _notificationService;
        private readonly IEventLogService _eventLogService;

        private IntPtr _windowHandle;
        private IntPtr _hNotifyKeyboard;
        private IntPtr _hNotifyHid;
        private HwndSource? _hwndSource;

        public event EventHandler<DeviceConnectionEvent>? DeviceStateChanged;

        public SurfaceDeviceDetails CurrentDevice { get; private set; } = new SurfaceDeviceDetails();

        public DeviceWatcherService(
            ISurfaceService surfaceService,
            IDatabaseService databaseService,
            INotificationService notificationService,
            IEventLogService eventLogService)
        {
            _surfaceService = surfaceService;
            _databaseService = databaseService;
            _notificationService = notificationService;
            _eventLogService = eventLogService;
        }

        public void StartMonitoring(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;

            _windowHandle = windowHandle;
            _hwndSource = HwndSource.FromHwnd(windowHandle);
            _hwndSource?.AddHook(WndProc);

            RegisterNotifications();
            _ = RefreshAsync();
        }

        public void StopMonitoring()
        {
            if (_hNotifyKeyboard != IntPtr.Zero)
            {
                NativeInterop.UnregisterDeviceNotification(_hNotifyKeyboard);
                _hNotifyKeyboard = IntPtr.Zero;
            }

            if (_hNotifyHid != IntPtr.Zero)
            {
                NativeInterop.UnregisterDeviceNotification(_hNotifyHid);
                _hNotifyHid = IntPtr.Zero;
            }

            _hwndSource?.RemoveHook(WndProc);
            _hwndSource = null;
        }

        public async Task RefreshAsync()
        {
            bool previousState = CurrentDevice.IsConnected;
            var updated = await _surfaceService.DetectSurfaceDeviceAsync();

            if (previousState != updated.IsConnected)
            {
                if (updated.IsConnected)
                {
                    updated.ConnectionTime = DateTime.Now;
                    updated.ReconnectCount = CurrentDevice.ReconnectCount + (previousState ? 0 : 1);
                    updated.LastDisconnectTime = CurrentDevice.LastDisconnectTime;

                    var evt = new DeviceConnectionEvent
                    {
                        EventType = previousState ? "Reconnect" : "Arrival",
                        DeviceName = updated.ModelName,
                        HardwareId = updated.HardwareId,
                        Details = $"Surface Keyboard connected: {updated.ModelName}"
                    };

                    _notificationService.ShowNotification("Surface Type Cover Connected", $"Device: {updated.ModelName}");
                    _eventLogService.AddLog("DeviceWatcher", $"Connected: {updated.ModelName}");
                    _ = _databaseService.SaveConnectionEventAsync(evt);
                    DeviceStateChanged?.Invoke(this, evt);
                }
                else
                {
                    updated.LastDisconnectTime = DateTime.Now;
                    updated.ReconnectCount = CurrentDevice.ReconnectCount;
                    updated.ConnectionTime = null;

                    var evt = new DeviceConnectionEvent
                    {
                        EventType = "Removal",
                        DeviceName = CurrentDevice.ModelName,
                        HardwareId = CurrentDevice.HardwareId,
                        Details = "Surface Keyboard disconnected"
                    };

                    _notificationService.ShowNotification("Surface Type Cover Disconnected", "Surface Keyboard was detached.");
                    _eventLogService.AddLog("DeviceWatcher", "Surface Keyboard Disconnected", Core.Enums.DiagnosticLevel.Warning);
                    _ = _databaseService.SaveConnectionEventAsync(evt);
                    DeviceStateChanged?.Invoke(this, evt);
                }
            }

            CurrentDevice = updated;
        }

        private void RegisterNotifications()
        {
            if (_windowHandle == IntPtr.Zero) return;

            var notificationFilter = new NativeInterop.DEV_BROADCAST_DEVICEINTERFACE();
            notificationFilter.dbcc_size = Marshal.SizeOf(notificationFilter);
            notificationFilter.dbcc_devicetype = 0x00000005; // DBT_DEVTYP_DEVICEINTERFACE

            notificationFilter.dbcc_classguid = NativeInterop.GUID_DEVINTERFACE_KEYBOARD;
            IntPtr bufferKeyboard = Marshal.AllocHGlobal(notificationFilter.dbcc_size);
            Marshal.StructureToPtr(notificationFilter, bufferKeyboard, false);
            _hNotifyKeyboard = NativeInterop.RegisterDeviceNotification(_windowHandle, bufferKeyboard, NativeInterop.DEVICE_NOTIFY_WINDOW_HANDLE);
            Marshal.FreeHGlobal(bufferKeyboard);

            notificationFilter.dbcc_classguid = NativeInterop.GUID_DEVINTERFACE_HID;
            IntPtr bufferHid = Marshal.AllocHGlobal(notificationFilter.dbcc_size);
            Marshal.StructureToPtr(notificationFilter, bufferHid, false);
            _hNotifyHid = NativeInterop.RegisterDeviceNotification(_windowHandle, bufferHid, NativeInterop.DEVICE_NOTIFY_WINDOW_HANDLE);
            Marshal.FreeHGlobal(bufferHid);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeInterop.WM_DEVICECHANGE)
            {
                int eventType = wParam.ToInt32();
                if (eventType == NativeInterop.DBT_DEVICEARRIVAL || eventType == NativeInterop.DBT_DEVICEREMOVECOMPLETE)
                {
                    _ = RefreshAsync();
                }
            }

            return IntPtr.Zero;
        }
    }
}
