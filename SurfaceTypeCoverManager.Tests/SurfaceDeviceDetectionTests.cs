using System;
using System.Threading.Tasks;
using Moq;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;
using SurfaceTypeCoverManager.Services.Services;
using Xunit;

namespace SurfaceTypeCoverManager.Tests
{
    public class SurfaceDeviceDetectionTests
    {
        [Fact]
        public async Task DetectSurfaceDeviceAsync_ReturnsValidDetailsStructure()
        {
            // Arrange
            var surfaceService = new SurfaceService();

            // Act
            var details = await surfaceService.DetectSurfaceDeviceAsync();

            // Assert
            Assert.NotNull(details);
            Assert.NotNull(details.ModelName);
            Assert.NotNull(details.VendorId);
            Assert.NotNull(details.ProductId);
            Assert.NotNull(details.FirmwareVersion);
        }

        [Fact]
        public void SurfaceServicesStatus_ReturnsListOfServices()
        {
            // Arrange
            var surfaceService = new SurfaceService();

            // Act
            var services = surfaceService.GetSurfaceServicesStatus();

            // Assert
            Assert.NotNull(services);
            Assert.NotEmpty(services);
        }
    }
}
