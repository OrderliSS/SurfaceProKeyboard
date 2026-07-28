using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class DiagnosticsService : IDiagnosticsService
    {
        private readonly IHidService _hidService;
        private readonly ISurfaceService _surfaceService;
        private readonly IEventLogService _eventLogService;
        private readonly IDatabaseService _databaseService;

        public DiagnosticsService(
            IHidService hidService,
            ISurfaceService surfaceService,
            IEventLogService eventLogService,
            IDatabaseService databaseService)
        {
            _hidService = hidService;
            _surfaceService = surfaceService;
            _eventLogService = eventLogService;
            _databaseService = databaseService;
        }

        public async Task<DiagnosticReport> RunDiagnosticsAsync()
        {
            var report = new DiagnosticReport
            {
                GeneratedAt = DateTime.Now,
                WindowsVersion = Environment.OSVersion.VersionString
            };

            await Task.Run(async () =>
            {
                // 1. Enumerate HID Devices
                var hids = _hidService.EnumerateHidDevices();
                report.HidDevices = hids.ToList();

                // 2. Check Surface Services
                report.SurfaceServices = _surfaceService.GetSurfaceServicesStatus().ToList();

                // 3. Check Drivers
                report.SurfaceDrivers = _surfaceService.GetSurfaceDrivers().ToList();

                // 4. SetupAPI Keyboards & Surface Devices
                report.Keyboards = _surfaceService.GetSetupApiProperties().ToList();

                // 5. Recent PnP Event Viewer Entries
                var pnpEvents = await _eventLogService.FetchSystemPnpEventsAsync(25);
                report.RecentPnpEvents = pnpEvents.ToList();

                // Health Checks
                var health = new List<string>();
                bool healthy = true;

                if (report.HidDevices.Count > 0)
                {
                    health.Add($"🟢 HID Enumeration: {report.HidDevices.Count} HID collections identified.");
                }
                else
                {
                    health.Add("⚠️ HID Enumeration: No HID devices detected.");
                    healthy = false;
                }

                bool surfaceFound = report.Keyboards.Any(k => k.PropertyValue.Contains("045E", StringComparison.OrdinalIgnoreCase) || k.PropertyValue.Contains("Surface", StringComparison.OrdinalIgnoreCase));
                if (surfaceFound)
                {
                    health.Add("🟢 Surface Hardware: Surface Type Cover / Keyboard device node verified.");
                }
                else
                {
                    health.Add("ℹ️ Surface Hardware: Standard HID Keyboard active (No Surface-specific PID filter matched).");
                }

                int runningServices = report.SurfaceServices.Count(s => s.Status.Equals("Running", StringComparison.OrdinalIgnoreCase));
                health.Add($"ℹ️ Surface Integration Services: {runningServices} of {report.SurfaceServices.Count} services running.");

                report.HealthCheckResults = health;
                report.IsOverallHealthy = healthy;
            });

            await _databaseService.SaveDiagnosticReportAsync(report);
            return report;
        }
    }
}
