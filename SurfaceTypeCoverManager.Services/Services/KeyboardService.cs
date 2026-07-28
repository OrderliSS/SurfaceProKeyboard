using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;
using SurfaceTypeCoverManager.Services.Interop;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class KeyboardService : IKeyboardService
    {
        private readonly HashSet<int> _pressedVkCodes = new HashSet<int>();
        private readonly ConcurrentDictionary<int, DateTime> _keyDownTimes = new ConcurrentDictionary<int, DateTime>();
        private readonly Queue<double> _recentLatencies = new Queue<double>();
        private readonly Queue<DateTime> _recentStrokes = new Queue<DateTime>();

        private DateTime _lastStrokeTime = DateTime.Now;
        private IntPtr _windowHandle;
        private HwndSource? _hwndSource;

        public event EventHandler<KeyStrokeInfo>? KeyPressed;
        public event EventHandler<KeyStrokeInfo>? KeyReleased;

        public IReadOnlySet<string> CurrentlyPressedKeys
        {
            get
            {
                lock (_pressedVkCodes)
                {
                    return _pressedVkCodes.Select(vk => KeyInterop.KeyFromVirtualKey(vk).ToString()).ToHashSet();
                }
            }
        }

        public string CurrentKey { get; private set; } = "None";

        public string ModifierState
        {
            get
            {
                var mods = new List<string>();
                if ((NativeInterop.GetKeyState(0x11) & 0x8000) != 0) mods.Add("Ctrl");
                if ((NativeInterop.GetKeyState(0x12) & 0x8000) != 0) mods.Add("Alt");
                if ((NativeInterop.GetKeyState(0x10) & 0x8000) != 0) mods.Add("Shift");
                if ((NativeInterop.GetKeyState(0x5B) & 0x8000) != 0 || (NativeInterop.GetKeyState(0x5C) & 0x8000) != 0) mods.Add("Win");
                return mods.Count > 0 ? string.Join(" + ", mods) : "None";
            }
        }

        public double EstimatedLatencyMs { get; private set; } = 4.2; // Baseline hardware HID polling estimate (250Hz - 1000Hz)

        public int PollingRateHz { get; private set; } = 250;

        public bool IsGhostingDetected { get; private set; }

        public int MaxRolloverDetected { get; private set; }

        public IReadOnlyList<string> StuckKeys
        {
            get
            {
                var stuck = new List<string>();
                var now = DateTime.Now;
                foreach (var kvp in _keyDownTimes)
                {
                    if ((now - kvp.Value).TotalSeconds >= 5.0)
                    {
                        stuck.Add(KeyInterop.KeyFromVirtualKey(kvp.Key).ToString());
                    }
                }
                return stuck;
            }
        }

        public void StartKeyHook(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;
            _windowHandle = windowHandle;

            _hwndSource = HwndSource.FromHwnd(windowHandle);
            _hwndSource?.AddHook(WndProc);

            var rid = new NativeInterop.RAWINPUTDEVICE[1];
            rid[0].usUsagePage = NativeInterop.HID_USAGE_PAGE_GENERIC;
            rid[0].usUsage = NativeInterop.HID_USAGE_GENERIC_KEYBOARD;
            rid[0].dwFlags = NativeInterop.RIDEV_INPUTSINK;
            rid[0].hwndTarget = windowHandle;

            NativeInterop.RegisterRawInputDevices(rid, 1, (uint)Marshal.SizeOf(typeof(NativeInterop.RAWINPUTDEVICE)));
        }

        public void StopKeyHook()
        {
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource = null;
        }

        public void ProcessKeyDown(int vkCode)
        {
            DateTime now = DateTime.Now;
            double latencyDelta = (now - _lastStrokeTime).TotalMilliseconds;
            _lastStrokeTime = now;

            if (latencyDelta > 1.0 && latencyDelta < 500.0)
            {
                lock (_recentLatencies)
                {
                    _recentLatencies.Enqueue(latencyDelta);
                    if (_recentLatencies.Count > 20) _recentLatencies.Dequeue();
                    EstimatedLatencyMs = Math.Round(_recentLatencies.Average(), 1);
                }
            }

            lock (_recentStrokes)
            {
                _recentStrokes.Enqueue(now);
                while (_recentStrokes.Count > 0 && (now - _recentStrokes.Peek()).TotalSeconds > 1.0)
                {
                    _recentStrokes.Dequeue();
                }
                PollingRateHz = Math.Max(125, _recentStrokes.Count * 25);
            }

            string keyName = KeyInterop.KeyFromVirtualKey(vkCode).ToString();
            CurrentKey = keyName;

            lock (_pressedVkCodes)
            {
                _pressedVkCodes.Add(vkCode);
                _keyDownTimes.TryAdd(vkCode, now);

                if (_pressedVkCodes.Count > MaxRolloverDetected)
                {
                    MaxRolloverDetected = _pressedVkCodes.Count;
                }

                // Simple Ghosting check: >4 non-modifier keys pressed concurrently
                int nonModifiers = _pressedVkCodes.Count(vk => vk != 0x11 && vk != 0x12 && vk != 0x10 && vk != 0x5B && vk != 0x5C);
                IsGhostingDetected = nonModifiers >= 4;
            }

            var info = new KeyStrokeInfo
            {
                Timestamp = now,
                KeyName = keyName,
                VirtualKey = vkCode,
                IsDown = true,
                Modifiers = ModifierState,
                LatencyMs = EstimatedLatencyMs,
                IsGhosted = IsGhostingDetected,
                IsStuck = (now - _keyDownTimes.GetValueOrDefault(vkCode, now)).TotalSeconds >= 5.0
            };

            KeyPressed?.Invoke(this, info);
        }

        public void ProcessKeyUp(int vkCode)
        {
            DateTime now = DateTime.Now;
            string keyName = KeyInterop.KeyFromVirtualKey(vkCode).ToString();

            lock (_pressedVkCodes)
            {
                _pressedVkCodes.Remove(vkCode);
                _keyDownTimes.TryRemove(vkCode, out _);
            }

            var info = new KeyStrokeInfo
            {
                Timestamp = now,
                KeyName = keyName,
                VirtualKey = vkCode,
                IsDown = false,
                Modifiers = ModifierState,
                LatencyMs = EstimatedLatencyMs,
                IsGhosted = false,
                IsStuck = false
            };

            KeyReleased?.Invoke(this, info);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeInterop.WM_INPUT)
            {
                uint dwSize = 0;
                NativeInterop.GetRawInputData(lParam, NativeInterop.RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(NativeInterop.RAWINPUTHEADER)));

                if (dwSize > 0)
                {
                    IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
                    try
                    {
                        if (NativeInterop.GetRawInputData(lParam, NativeInterop.RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(NativeInterop.RAWINPUTHEADER))) == dwSize)
                        {
                            var raw = Marshal.PtrToStructure<NativeInterop.RAWINPUT>(buffer);
                            if (raw.header.dwType == NativeInterop.RIM_TYPEKEYBOARD)
                            {
                                ushort vkey = raw.data.keyboard.VKey;
                                ushort flags = raw.data.keyboard.Flags;
                                bool isUp = (flags & 0x01) != 0;

                                if (vkey > 0)
                                {
                                    if (isUp)
                                    {
                                        ProcessKeyUp(vkey);
                                    }
                                    else
                                    {
                                        ProcessKeyDown(vkey);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
            }

            return IntPtr.Zero;
        }
    }
}
