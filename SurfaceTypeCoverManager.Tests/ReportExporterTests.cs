using System;
using System.IO;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Core.Models;
using SurfaceTypeCoverManager.Services.Services;
using Xunit;

namespace SurfaceTypeCoverManager.Tests
{
    public class ReportExporterTests
    {
        [Fact]
        public async Task ExportToJsonAsync_CreatesValidFile()
        {
            // Arrange
            var exporter = new ReportExporterService();
            var report = new DiagnosticReport
            {
                GeneratedAt = DateTime.Now,
                WindowsVersion = "Windows 11 23H2"
            };

            string tempFile = Path.Combine(Path.GetTempPath(), $"test_report_{Guid.NewGuid()}.json");

            try
            {
                // Act
                await exporter.ExportToJsonAsync(report, tempFile);

                // Assert
                Assert.True(File.Exists(tempFile));
                string content = await File.ReadAllTextAsync(tempFile);
                Assert.Contains("Windows 11 23H2", content);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}
