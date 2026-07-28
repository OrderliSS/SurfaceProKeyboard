using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;
using SurfaceTypeCoverManager.Services.Interop;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class HidService : IHidService
    {
        public IReadOnlyList<HidDeviceInfo> EnumerateHidDevices()
        {
            var list = new List<HidDeviceInfo>();
            Guid hidGuid = NativeInterop.GUID_DEVINTERFACE_HID;

            IntPtr deviceInfoSet = NativeInterop.SetupDiGetClassDevsW(
                ref hidGuid,
                null,
                IntPtr.Zero,
                NativeInterop.DIGCF_PRESENT | NativeInterop.DIGCF_DEVICEINTERFACE);

            if (deviceInfoSet == IntPtr.Zero || deviceInfoSet == (IntPtr)(-1))
            {
                return list;
            }

            try
            {
                var devInfoData = new NativeInterop.SP_DEVINFO_DATA();
                devInfoData.cbSize = Marshal.SizeOf(typeof(NativeInterop.SP_DEVINFO_DATA));

                uint index = 0;
                while (NativeInterop.SetupDiEnumDeviceInfo(deviceInfoSet, index, ref devInfoData))
                {
                    index++;
                    var info = GetHidDeviceInfo(deviceInfoSet, devInfoData);
                    if (info != null)
                    {
                        list.Add(info);
                    }
                }
            }
            finally
            {
                NativeInterop.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return list;
        }

        public HidDeviceInfo? GetDeviceDetails(string devicePath)
        {
            // Fallback lookup or path-based inspection
            var all = EnumerateHidDevices();
            foreach (var dev in all)
            {
                if (dev.DevicePath.Equals(devicePath, StringComparison.OrdinalIgnoreCase))
                {
                    return dev;
                }
            }
            return null;
        }

        private HidDeviceInfo? GetHidDeviceInfo(IntPtr deviceInfoSet, NativeInterop.SP_DEVINFO_DATA devInfoData)
        {
            var info = new HidDeviceInfo();

            // Hardware IDs
            string hwIds = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_HardwareIds);
            info.DevicePath = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_InstanceId);

            // Extract VID & PID from Hardware IDs or Instance ID
            if (!string.IsNullOrEmpty(hwIds))
            {
                ParseVidPid(hwIds, out ushort vid, out ushort pid);
                info.VendorId = vid;
                info.ProductId = pid;
            }
            else if (!string.IsNullOrEmpty(info.DevicePath))
            {
                ParseVidPid(info.DevicePath, out ushort vid, out ushort pid);
                info.VendorId = vid;
                info.ProductId = pid;
            }

            info.Manufacturer = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_Manufacturer);
            info.Product = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_FriendlyName);
            if (string.IsNullOrEmpty(info.Product) || info.Product == "Unavailable")
            {
                info.Product = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_DeviceDesc);
            }

            return info;
        }

        private static string GetStringProperty(IntPtr deviceInfoSet, NativeInterop.SP_DEVINFO_DATA devInfoData, NativeInterop.DEVPROPKEY key)
        {
            byte[] buffer = new byte[1024];
            if (NativeInterop.SetupDiGetDevicePropertyW(deviceInfoSet, ref devInfoData, ref key, out uint propType, buffer, (uint)buffer.Length, out uint requiredSize, 0))
            {
                string result = Encoding.Unicode.GetString(buffer, 0, (int)requiredSize).TrimEnd('\0');
                return string.IsNullOrWhiteSpace(result) ? "Unavailable" : result;
            }
            return "Unavailable";
        }

        private static void ParseVidPid(string input, out ushort vid, out ushort pid)
        {
            vid = 0;
            pid = 0;
            if (string.IsNullOrEmpty(input)) return;

            string upper = input.ToUpperInvariant();
            int vidIdx = upper.IndexOf("VID_");
            if (vidIdx >= 0 && vidIdx + 8 <= upper.Length)
            {
                ushort.TryParse(upper.Substring(vidIdx + 4, 4), System.Globalization.NumberStyles.HexNumber, null, out vid);
            }

            int pidIdx = upper.IndexOf("PID_");
            if (pidIdx >= 0 && pidIdx + 8 <= upper.Length)
            {
                ushort.TryParse(upper.Substring(pidIdx + 4, 4), System.Globalization.NumberStyles.HexNumber, null, out pid);
            }
        }
    }
}
