using System;
using System.Threading;
using SurfaceTypeCoverManager.Services.Services;
using Xunit;

namespace SurfaceTypeCoverManager.Tests
{
    public class KeyboardMetricsTests
    {
        [Fact]
        public void ProcessKeyDown_UpdatesCurrentKeyAndPressedState()
        {
            // Arrange
            var keyboardService = new KeyboardService();
            bool eventFired = false;
            keyboardService.KeyPressed += (s, e) => eventFired = true;

            // Act
            keyboardService.ProcessKeyDown(0x41); // VirtualKey 'A'

            // Assert
            Assert.True(eventFired);
            Assert.Equal("A", keyboardService.CurrentKey);
            Assert.Contains("A", keyboardService.CurrentlyPressedKeys);
        }

        [Fact]
        public void ProcessKeyUp_RemovesKeyFromPressedState()
        {
            // Arrange
            var keyboardService = new KeyboardService();
            keyboardService.ProcessKeyDown(0x42); // VirtualKey 'B'
            Assert.Contains("B", keyboardService.CurrentlyPressedKeys);

            // Act
            keyboardService.ProcessKeyUp(0x42);

            // Assert
            Assert.DoesNotContain("B", keyboardService.CurrentlyPressedKeys);
        }
    }
}
