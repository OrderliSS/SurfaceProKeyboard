using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class ReportExporterService : IReportExporterService
    {
        public async Task ExportToJsonAsync(DiagnosticReport report, string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(report, options);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        }

        public async Task ExportToHtmlAsync(DiagnosticReport report, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"UTF-8\">");
            sb.AppendLine("  <title>Surface Type Cover Diagnostic Report</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #0f172a; color: #f8fafc; margin: 0; padding: 24px; }");
            sb.AppendLine("    h1 { color: #38bdf8; border-bottom: 2px solid #334155; padding-bottom: 8px; }");
            sb.AppendLine("    h2 { color: #38bdf8; margin-top: 24px; }");
            sb.AppendLine("    .card { background-color: #1e293b; border-radius: 8px; padding: 16px; margin-bottom: 16px; border: 1px solid #334155; }");
            sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin-top: 8px; }");
            sb.AppendLine("    th, td { text-align: left; padding: 10px; border-bottom: 1px solid #334155; }");
            sb.AppendLine("    th { background-color: #0f172a; color: #94a3b8; }");
            sb.AppendLine("    .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-weight: bold; }");
            sb.AppendLine("    .healthy { background-color: #059669; color: white; }");
            sb.AppendLine("    .warning { background-color: #d97706; color: white; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine($"  <h1>Surface Type Cover Diagnostic Report</h1>");
            sb.AppendLine($"  <p><strong>Generated At:</strong> {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"  <p><strong>Windows Version:</strong> {report.WindowsVersion}</p>");

            sb.AppendLine($"  <div class=\"card\">");
            sb.AppendLine($"    <h2>Health Status Summary</h2>");
            sb.AppendLine($"    <p><span class=\"badge {(report.IsOverallHealthy ? "healthy" : "warning")}\">{(report.IsOverallHealthy ? "SYSTEM HEALTHY" : "ATTENTION REQUIRED")}</span></p>");
            sb.AppendLine("    <ul>");
            foreach (var h in report.HealthCheckResults)
            {
                sb.AppendLine($"      <li>{h}</li>");
            }
            sb.AppendLine("    </ul>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class=\"card\">");
            sb.AppendLine("    <h2>Surface Integration Services</h2>");
            sb.AppendLine("    <table><tr><th>Service</th><th>Display Name</th><th>Status</th></tr>");
            foreach (var s in report.SurfaceServices)
            {
                sb.AppendLine($"      <tr><td>{s.ServiceName}</td><td>{s.DisplayName}</td><td>{s.Status}</td></tr>");
            }
            sb.AppendLine("    </table>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class=\"card\">");
            sb.AppendLine("    <h2>HID Device Collections</h2>");
            sb.AppendLine("    <table><tr><th>Path / ID</th><th>Vendor ID</th><th>Product ID</th><th>Product Name</th></tr>");
            foreach (var hid in report.HidDevices)
            {
                sb.AppendLine($"      <tr><td>{hid.DevicePath}</td><td>0x{hid.VendorId:X4}</td><td>0x{hid.ProductId:X4}</td><td>{hid.Product}</td></tr>");
            }
            sb.AppendLine("    </table>");
            sb.AppendLine("  </div>");

            sb.AppendLine("</body></html>");

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }

        public async Task ExportToMarkdownAsync(DiagnosticReport report, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Surface Type Cover Diagnostic Report");
            sb.AppendLine($"**Generated At:** {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"**Windows Version:** {report.WindowsVersion}");
            sb.AppendLine();
            sb.AppendLine("## Health Check Results");
            foreach (var h in report.HealthCheckResults)
            {
                sb.AppendLine($"- {h}");
            }
            sb.AppendLine();
            sb.AppendLine("## HID Devices");
            sb.AppendLine("| Vendor ID | Product ID | Product Name | Device Path |");
            sb.AppendLine("| --- | --- | --- | --- |");
            foreach (var hid in report.HidDevices)
            {
                sb.AppendLine($"| 0x{hid.VendorId:X4} | 0x{hid.ProductId:X4} | {hid.Product} | {hid.DevicePath} |");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }

        public async Task<string> ExportZipBundleAsync(DiagnosticReport report, SurfaceDeviceDetails device, IEnumerable<DeviceConnectionEvent> history, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string tempDir = Path.Combine(outputDirectory, $"DiagnosticBundle_{timeStamp}");
            Directory.CreateDirectory(tempDir);

            string jsonPath = Path.Combine(tempDir, "diagnostic_report.json");
            string htmlPath = Path.Combine(tempDir, "diagnostic_report.html");
            string mdPath = Path.Combine(tempDir, "diagnostic_report.md");
            string devPath = Path.Combine(tempDir, "device_summary.json");

            await ExportToJsonAsync(report, jsonPath);
            await ExportToHtmlAsync(report, htmlPath);
            await ExportToMarkdownAsync(report, mdPath);

            string devJson = JsonSerializer.Serialize(device, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(devPath, devJson, Encoding.UTF8);

            string zipPath = Path.Combine(outputDirectory, $"Surface_DiagnosticBundle_{timeStamp}.zip");
            if (File.Exists(zipPath)) File.Delete(zipPath);

            ZipFile.CreateFromDirectory(tempDir, zipPath);
            Directory.Delete(tempDir, true);

            return zipPath;
        }
    }
}
