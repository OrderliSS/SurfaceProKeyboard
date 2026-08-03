using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Core.Enums;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;
using SurfaceTypeCoverManager.Services.Interop;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class SurfaceService : ISurfaceService
    {
        private const ushort MICROSOFT_VID = 0x045E;

        private static readonly Dictionary<ushort, string> SurfaceProductMap = new Dictionary<ushort, string>
        {
            { 0x07DC, "Surface Pro 3 Type Cover" },
            { 0x07E4, "Surface Pro Type Cover" },
            { 0x07E5, "Surface Pro 4 Type Cover" },
            { 0x07E8, "Surface Pro Signature Type Cover" },
            { 0x09C0, "Surface Signature Keyboard" },
            { 0x09B0, "Surface Pro 8 Signature Keyboard" },
            { 0x09B1, "Surface Pro 9 Signature Keyboard" },
            { 0x09B2, "Surface Pro 10 Signature Keyboard" },
            { 0x09B5, "Surface Pro Flex Keyboard" },
            { 0x09B6, "Surface Pro Flex Wireless Keyboard" },
            { 0x09C1, "Surface Keyboard" },
            { 0x07E6, "Surface Laptop Keyboard" },
            // Third-Party & Bluetooth Type Covers
            { 0x8502, "Third-Party Bluetooth Keyboard (Broadcom)" }, // PID 8502 (VID 0A5C)
            { 0x8514, "Third-Party Wireless Type Cover (Telink)" }   // PID 8514 (VID 248A)
        };

        public Task<SurfaceDeviceDetails> DetectSurfaceDeviceAsync()
        {
            return Task.Run(() =>
            {
                var details = new SurfaceDeviceDetails();
                Guid keyboardGuid = NativeInterop.GUID_DEVINTERFACE_KEYBOARD;

                IntPtr deviceInfoSet = NativeInterop.SetupDiGetClassDevsW(
                    ref keyboardGuid,
                    null,
                    IntPtr.Zero,
                    NativeInterop.DIGCF_PRESENT);

                if (deviceInfoSet == IntPtr.Zero || deviceInfoSet == (IntPtr)(-1))
                {
                    // Fallback to searching all present devices
                    deviceInfoSet = NativeInterop.SetupDiGetClassDevsW(
                        IntPtr.Zero,
                        null,
                        IntPtr.Zero,
                        NativeInterop.DIGCF_PRESENT | NativeInterop.DIGCF_ALLCLASSES);
                }

                if (deviceInfoSet == IntPtr.Zero || deviceInfoSet == (IntPtr)(-1))
                {
                    PopulateLockStates(details);
                    return details;
                }

                try
                {
                    var devInfoData = new NativeInterop.SP_DEVINFO_DATA();
                    devInfoData.cbSize = Marshal.SizeOf(typeof(NativeInterop.SP_DEVINFO_DATA));

                    uint index = 0;
                    bool surfaceFound = false;

                    while (NativeInterop.SetupDiEnumDeviceInfo(deviceInfoSet, index, ref devInfoData))
                    {
                        index++;

                        string hwIds = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_HardwareIds);
                        string instanceId = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_InstanceId);
                        string friendlyName = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_FriendlyName);
                        string desc = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_DeviceDesc);
                        string mfg = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_Manufacturer);

                        string combined = $"{hwIds} {instanceId} {friendlyName} {desc} {mfg}".ToUpperInvariant();

                        bool isSurface = combined.Contains("VID_045E") || combined.Contains("SURFACE") || combined.Contains("TYPE COVER") || combined.Contains("MSHW");
                        bool isThirdParty = combined.Contains("0A5C") || combined.Contains("248A") || (combined.Contains("BLUETOOTH") && combined.Contains("KEYBOARD"));

                        if (isSurface || isThirdParty)
                        {
                            surfaceFound = true;
                            details.IsConnected = true;
                            
                            ParseVidPid(hwIds.Length > 0 ? hwIds : instanceId, out ushort vid, out ushort pid);
                            
                            // If ParseVidPid fails due to "VID&" formatting, we manually check
                            if (vid == 0 && combined.Contains("0A5C")) vid = 0x0A5C;
                            if (vid == 0 && combined.Contains("248A")) vid = 0x248A;
                            if (pid == 0 && combined.Contains("8502")) pid = 0x8502;
                            if (pid == 0 && combined.Contains("8514")) pid = 0x8514;

                            details.VendorId = vid > 0 ? vid.ToString("X4") : (isThirdParty ? "Unknown" : "045E");

                            if (pid > 0)
                            {
                                details.ProductId = pid.ToString("X4");
                                if (SurfaceProductMap.TryGetValue(pid, out string? model))
                                {
                                    details.ModelName = model;
                                }
                                else if (isThirdParty)
                                {
                                    details.ModelName = $"Third-Party Keyboard (PID: {details.ProductId})";
                                }
                                else
                                {
                                    details.ModelName = $"Surface Keyboard (PID: {details.ProductId})";
                                }
                            }
                            else if (!string.IsNullOrEmpty(friendlyName) && friendlyName != "Unavailable")
                            {
                                details.ModelName = friendlyName;
                            }
                            else
                            {
                                details.ModelName = isThirdParty ? "Third-Party Keyboard" : "Surface Type Cover";
                            }

                            details.HardwareId = string.IsNullOrEmpty(hwIds) ? "Unavailable" : hwIds;
                            details.FriendlyName = friendlyName;
                            details.Manufacturer = string.IsNullOrEmpty(mfg) ? "Microsoft Corporation" : mfg;
                            details.InstanceId = instanceId;
                            details.ClassGuid = devInfoData.ClassGuid.ToString();
                            details.LocationPath = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_LocationPaths);
                            details.ContainerId = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_ContainerId);
                            details.FirmwareVersion = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_FirmwareVersion);
                            if (details.FirmwareVersion == "Unavailable")
                            {
                                details.FirmwareVersion = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_DriverVersion);
                            }

                            break; // Priority to primary Surface device
                        }
                    }

                    if (!surfaceFound)
                    {
                        try
                        {
                            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%keyboard%'");
                            foreach (ManagementObject obj in searcher.Get())
                            {
                                string name = obj["Name"]?.ToString() ?? "";
                                string hwIdObj = "";
                                if (obj["HardwareID"] is string[] hwIdsArray) hwIdObj = string.Join(" ", hwIdsArray);
                                string deviceId = obj["DeviceID"]?.ToString() ?? "";
                                string mfg = obj["Manufacturer"]?.ToString() ?? "";

                                string combinedWmi = $"{name} {hwIdObj} {deviceId} {mfg}".ToUpperInvariant();
                                bool isSurfaceWmi = combinedWmi.Contains("VID_045E") || combinedWmi.Contains("SURFACE") || combinedWmi.Contains("TYPE COVER") || combinedWmi.Contains("MSHW");
                                bool isThirdPartyWmi = combinedWmi.Contains("0A5C") || combinedWmi.Contains("248A") || (combinedWmi.Contains("BLUETOOTH") && combinedWmi.Contains("KEYBOARD"));

                                if (isSurfaceWmi || isThirdPartyWmi)
                                {
                                    surfaceFound = true;
                                    details.IsConnected = true;
                                    ParseVidPid(hwIdObj.Length > 0 ? hwIdObj : deviceId, out ushort vid, out ushort pid);
                                    if (vid == 0 && combinedWmi.Contains("0A5C")) vid = 0x0A5C;
                                    if (vid == 0 && combinedWmi.Contains("248A")) vid = 0x248A;
                                    if (pid == 0 && combinedWmi.Contains("8502")) pid = 0x8502;
                                    if (pid == 0 && combinedWmi.Contains("8514")) pid = 0x8514;
                                    details.VendorId = vid > 0 ? vid.ToString("X4") : (isThirdPartyWmi ? "Unknown" : "045E");
                                    
                                    if (pid > 0)
                                    {
                                        details.ProductId = pid.ToString("X4");
                                        if (SurfaceProductMap.TryGetValue(pid, out string? model))
                                        {
                                            details.ModelName = model;
                                        }
                                        else if (isThirdPartyWmi)
                                        {
                                            details.ModelName = $"Third-Party Keyboard (PID: {details.ProductId})";
                                        }
                                        else
                                        {
                                            details.ModelName = $"Surface Keyboard (PID: {details.ProductId})";
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(name) && name != "Unavailable")
                                    {
                                        details.ModelName = name;
                                    }
                                    else
                                    {
                                        details.ModelName = isThirdPartyWmi ? "Third-Party Keyboard" : "Surface Type Cover";
                                    }

                                    details.HardwareId = string.IsNullOrEmpty(hwIdObj) ? "Unavailable" : hwIdObj;
                                    details.FriendlyName = name;
                                    details.Manufacturer = string.IsNullOrEmpty(mfg) ? "Microsoft Corporation" : mfg;
                                    details.InstanceId = deviceId;
                                    break;
                                }
                            }
                        }
                        catch { }

                        if (!surfaceFound)
                        {
                            details.IsConnected = false;
                            details.ModelName = "Unavailable";
                        }
                    }
                }
                finally
                {
                    NativeInterop.SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }

                PopulateLockStates(details);

                details.BacklightStatus = LockState.Unknown; // Hardware state requires specific MS driver, graceful fallback
                details.TouchpadStatus = "Enabled";
                details.TouchpadGesturesAvailable = true;
                
                if (details.IsConnected)
                {
                    details.BatteryStatus = GetBatteryLevel(details.ModelName.Contains("Third-Party") || details.ModelName.Contains("Bluetooth"));
                }
                else
                {
                    details.BatteryStatus = "Unavailable";
                }

                details.HostModel = GetHostModel();
                
                if (details.IsConnected)
                {
                    string instIdUpper = (details.InstanceId ?? "").ToUpperInvariant();
                    string hwIdUpper = (details.HardwareId ?? "").ToUpperInvariant();
                    string modelUpper = (details.ModelName ?? "").ToUpperInvariant();
                    string allContext = $"{instIdUpper} {hwIdUpper} {modelUpper}";
                    
                    if (allContext.Contains("BTHENUM") || allContext.Contains("{00001124") || allContext.Contains("BLUETOOTH"))
                    {
                        details.ConnectionType = "Bluetooth";
                    }
                    else if (allContext.Contains("MSHW") || allContext.Contains("SPI") || allContext.Contains("I2C") || allContext.Contains("SURFACE"))
                    {
                        details.ConnectionType = "Surface Connect";
                    }
                    else
                    {
                        details.ConnectionType = "USB";
                    }
                }
                
                return details;
            });
        }

        private static string GetHostModel()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                if (key != null)
                {
                    var val = key.GetValue("SystemProductName") as string;
                    if (!string.IsNullOrEmpty(val)) return val;
                }
            }
            catch { }
            return "Unknown Host";
        }

        public IReadOnlyList<ServiceStatusInfo> GetSurfaceServicesStatus()
        {
            var list = new List<ServiceStatusInfo>();
            string[] targetServices = new[]
            {
                "SurfaceService",
                "SurfaceIntegrationService",
                "SurfaceSystemTelemetry",
                "SurfaceDTX",
                "SurfaceTouch",
                "SurfaceHotplug"
            };

            foreach (var sName in targetServices)
            {
                try
                {
                    using var sc = new ServiceController(sName);
                    list.Add(new ServiceStatusInfo
                    {
                        ServiceName = sName,
                        DisplayName = sc.DisplayName,
                        Status = sc.Status.ToString()
                    });
                }
                catch
                {
                    list.Add(new ServiceStatusInfo
                    {
                        ServiceName = sName,
                        DisplayName = sName,
                        Status = "Not Installed"
                    });
                }
            }

            return list;
        }

        public IReadOnlyList<DriverInfo> GetSurfaceDrivers()
        {
            var list = new List<DriverInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceName LIKE '%Surface%' OR Description LIKE '%Surface%'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    list.Add(new DriverInfo
                    {
                        DriverName = obj["DeviceName"]?.ToString() ?? obj["Description"]?.ToString() ?? "Unknown Surface Driver",
                        Provider = obj["DriverProviderName"]?.ToString() ?? "Unavailable",
                        Version = obj["DriverVersion"]?.ToString() ?? "Unavailable",
                        Date = obj["DriverDate"]?.ToString() ?? "Unavailable"
                    });
                }
            }
            catch
            {
                // Fallback graceful handling
            }

            if (list.Count == 0)
            {
                list.Add(new DriverInfo
                {
                    DriverName = "Surface Type Cover Filter Driver",
                    Provider = "Microsoft Corporation",
                    Version = "Unavailable",
                    Date = "Unavailable"
                });
            }

            return list;
        }

        public IReadOnlyList<SetupApiPropertyItem> GetSetupApiProperties()
        {
            var list = new List<SetupApiPropertyItem>();
            Guid keyboardGuid = NativeInterop.GUID_DEVINTERFACE_KEYBOARD;

            IntPtr deviceInfoSet = NativeInterop.SetupDiGetClassDevsW(
                ref keyboardGuid,
                null,
                IntPtr.Zero,
                NativeInterop.DIGCF_PRESENT);

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
                    string friendlyName = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_FriendlyName);
                    if (friendlyName == "Unavailable") friendlyName = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_DeviceDesc);

                    list.Add(new SetupApiPropertyItem { Category = friendlyName, PropertyName = "Friendly Name", PropertyValue = friendlyName });
                    list.Add(new SetupApiPropertyItem { Category = friendlyName, PropertyName = "Manufacturer", PropertyValue = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_Manufacturer) });
                    list.Add(new SetupApiPropertyItem { Category = friendlyName, PropertyName = "Hardware IDs", PropertyValue = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_HardwareIds) });
                    list.Add(new SetupApiPropertyItem { Category = friendlyName, PropertyName = "Instance ID", PropertyValue = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_InstanceId) });
                    list.Add(new SetupApiPropertyItem { Category = friendlyName, PropertyName = "Location Paths", PropertyValue = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_LocationPaths) });
                    list.Add(new SetupApiPropertyItem { Category = friendlyName, PropertyName = "Container ID", PropertyValue = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_ContainerId) });
                    list.Add(new SetupApiPropertyItem { Category = friendlyName, PropertyName = "Driver Version", PropertyValue = GetStringProperty(deviceInfoSet, devInfoData, NativeInterop.DEVPKEY_Device_DriverVersion) });
                }
            }
            finally
            {
                NativeInterop.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return list;
        }

        private static void PopulateLockStates(SurfaceDeviceDetails details)
        {
            details.CapsLock = (NativeInterop.GetKeyState(0x14) & 0x0001) != 0 ? LockState.On : LockState.Off;
            details.NumLock = (NativeInterop.GetKeyState(0x90) & 0x0001) != 0 ? LockState.On : LockState.Off;
            details.FnLock = LockState.Unknown; // Fn Lock is handled internal to keyboard firmware
        }

        private static string GetBatteryLevel(bool isThirdParty)
        {
            // For genuine type covers, they don't have batteries, they draw from the host.
            // For third-party Bluetooth keyboards, we attempt to read system/device battery.
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT EstimatedChargeRemaining FROM Win32_Battery");
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["EstimatedChargeRemaining"] != null)
                    {
                        return $"{obj["EstimatedChargeRemaining"]}%" + (isThirdParty ? " (Host/System Battery Fallback)" : "");
                    }
                }
            }
            catch
            {
                // Ignore WMI errors
            }
            
            return isThirdParty ? "Unknown (Not reported by device)" : "Host Powered";
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
            
            var vidMatch = System.Text.RegularExpressions.Regex.Match(upper, @"VID[_&](?:[0-9A-F]{4})?([0-9A-F]{4})");
            if (vidMatch.Success)
            {
                ushort.TryParse(vidMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out vid);
            }

            var pidMatch = System.Text.RegularExpressions.Regex.Match(upper, @"PID[_&](?:[0-9A-F]{4})?([0-9A-F]{4})");
            if (pidMatch.Success)
            {
                ushort.TryParse(pidMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out pid);
            }
        }
    }
}
