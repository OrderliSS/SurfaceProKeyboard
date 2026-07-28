using System;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Core.Models;
using SurfaceTypeCoverManager.Services.Services;
using Xunit;

namespace SurfaceTypeCoverManager.Tests
{
    public class DatabaseServiceTests
    {
        [Fact]
        public async Task DatabaseService_InitializeAndSaveOperations_Succeed()
        {
            // Arrange
            var db = new DatabaseService();
            await db.InitializeAsync();

            var connectionEvt = new DeviceConnectionEvent
            {
                Timestamp = DateTime.Now,
                EventType = "Arrival",
                DeviceName = "Surface Pro 4 Type Cover",
                HardwareId = "VID_045E&PID_07E5",
                Details = "Test Connection"
            };

            // Act
            await db.SaveConnectionEventAsync(connectionEvt);
            var history = await db.GetConnectionHistoryAsync();

            // Assert
            Assert.NotNull(history);
            Assert.NotEmpty(history);
        }
    }
}
