# Surface Type Cover Manager

A professional Windows desktop application for monitoring, diagnosing, and managing Microsoft Surface Type Covers and Signature Keyboards. Built with **C#**, **.NET 8 (WPF)**, **CommunityToolkit.Mvvm**, **Microsoft.Extensions.Hosting**, **LiveCharts2**, and **SQLite**.

Designed to provide hardware telemetry similar to Logitech G Hub, Lenovo Vantage, and HWiNFO, specifically dedicated to Microsoft Surface keyboard attachments.

---

## 🌟 Key Features

### 1. Primary Dashboard
- **Real-Time Status Badge**: Displays `Connected` 🟢 or `Disconnected` 🔴 status.
- **Hardware Telemetry**: Model name (Surface Pro 4 Type Cover, Surface Signature Keyboard, Surface Pro Flex, etc.), Vendor ID (`045E`), Product ID (PID), Firmware Version, Serial Number, and Hardware ID.
- **Connection Duration**: Active connection timer, reconnect counters, and last disconnect timestamp.
- **Keyboard Lock States**: Live monitoring for **Caps Lock**, **Num Lock**, **Fn Lock**, and **Backlight** states.
- **Precision Touchpad & Battery**: Touchpad enablement and battery state fallback.

### 2. Live Keyboard Visualizer
- **Interactive Visual Layout**: Key press visualization highlighting active keys in real-time.
- **Performance Telemetry**:
  - **Polling Rate Estimate** (Hz)
  - **Inter-Key Latency Estimate** (ms)
  - **Modifiers State** (Ctrl, Alt, Shift, Win)
  - **Ghosting Signal Detection** (3+ non-modifier key anomaly tracking)
  - **Stuck Key Detection** (holds longer than 5s)
  - **NKRO Rollover Test** (Peak concurrent key presses)

### 3. Typing Speed & Latency Test
- Interactive speed test prompt measuring **WPM**, **Accuracy %**, **Errors**, **Duplicate Presses**, **Dropped Events**, and **Average/Max Latency**.
- Real-time latency chart over time powered by **LiveCharts2**.
- Automatic persistence to SQLite database.

### 4. Connection Event Monitor
- Event timeline tracking device arrivals, removals, reconnects, and disconnect durations.
- Summary telemetry cards for historical attach/detach statistics.

### 5. Precision Touchpad Diagnostics
- Detects Windows **Precision Touchpad (PTP)** capability.
- Multi-finger gesture support status.
- Interactive pointer movement test pad & activity recorder.

### 6. System Health Diagnostics & Exporter
- Full diagnostic suite scanning **HID collections**, **SetupAPI keyboard nodes**, **Surface integration services**, **driver versions**, and **Windows Event Viewer PnP logs**.
- **One-Click Exporters**:
  - **JSON Diagnostic Report**
  - **HTML Interactive Dark Theme Report**
  - **Markdown Summary**
  - **ZIP Diagnostic Package Bundle**

### 7. SetupAPI DEVPKEY Inspector
- Searchable property grid exposing every SetupAPI `DEVPKEY` attribute provided by Windows (`Friendly Name`, `Manufacturer`, `Class GUID`, `Instance ID`, `Container ID`, `Location Paths`, `Bus Type`, `Removal Policy`, `Driver Version`, etc.).

### 8. Event Log Stream
- Real-time event log viewer streaming `Microsoft-Windows-Kernel-PnP` and `DeviceSetupManager` system events with search filtering.

### 9. SQLite Historical Database
- Persistent storage for connection history, typing sessions, and diagnostic health snapshots.

---

## 🏗️ Architecture & Technology Stack

- **Language**: C# 12 / .NET 8
- **UI Framework**: WPF (`net8.0-windows10.0.19041.0`)
- **MVVM Framework**: `CommunityToolkit.Mvvm`
- **Dependency Injection**: `Microsoft.Extensions.Hosting` (Generic Host)
- **Logging**: `Serilog`
- **Charts**: `LiveChartsCore.SkiaSharpView.WPF`
- **Database**: SQLite (`Microsoft.Data.Sqlite`)
- **Toast Notifications**: Windows App SDK / WinRT (`Microsoft.Toolkit.Uwp.Notifications`)

### Project Structure

```
SurfaceTypeCoverManager.sln
 ├── SurfaceTypeCoverManager.Core
 │    ├── Enums.cs              (ConnectionStatus, LockState, DiagnosticLevel, ThemeMode)
 │    ├── Models.cs             (SurfaceDeviceDetails, KeyStrokeInfo, DiagnosticReport, etc.)
 │    └── Interfaces.cs         (IDeviceWatcherService, IHidService, ISurfaceService, etc.)
 │
 ├── SurfaceTypeCoverManager.Services
 │    ├── Interop/              (SetupAPI, HID, RawInput, User32 Win32 P/Invoke declarations)
 │    └── Services/             (DeviceWatcher, SurfaceService, HidService, KeyboardService,
 │                               DatabaseService, DiagnosticsService, ReportExporterService, etc.)
 │
 ├── SurfaceTypeCoverManager.UI
 │    ├── Styles/               (ThemeResources.xaml)
 │    ├── ViewModels/           (MainViewModel, DashboardVM, KeyboardVM, TypingTestVM, etc.)
 │    ├── Views/                (DashboardView, KeyboardView, DiagnosticsView, etc.)
 │    ├── App.xaml / App.xaml.cs(Dependency Injection container startup)
 │    └── MainWindow.xaml       (G Hub / Vantage inspired dark theme interface)
 │
 └── SurfaceTypeCoverManager.Tests
      └── (Unit tests for device detection, metrics calculations, DB operations, exporters)
```

---

## 🚀 How to Build & Run

### Prerequisites
- Windows 10 (19041+) or Windows 11
- .NET 8.0 SDK

### Commands

1. **Build Solution**:
   ```bash
   dotnet build SurfaceTypeCoverManager.sln
   ```

2. **Run Unit Tests**:
   ```bash
   dotnet test SurfaceTypeCoverManager.Tests/SurfaceTypeCoverManager.Tests.csproj
   ```

3. **Launch Application**:
   ```bash
   dotnet run --project SurfaceTypeCoverManager.UI/SurfaceTypeCoverManager.UI.csproj
   ```

---

## 🔒 Important Constraints & Safety

- **No Firmware Modification**: Does not attempt to alter device firmware or reverse-engineer locked protocols.
- **Official APIs Only**: Strictly uses standard Windows APIs (`SetupAPI`, `Win32 HID`, `Win32 Raw Input`, `WMI`, `EventLogWatcher`, `Registry`).
- **Graceful Fallbacks**: Hardware properties that Windows does not expose are cleanly labeled as `"Unavailable"` or `"Not Supported"`.
- **Low Footprint**: Event-driven WndProc hook design maintains **< 1% CPU** and **< 90MB RAM** usage.
