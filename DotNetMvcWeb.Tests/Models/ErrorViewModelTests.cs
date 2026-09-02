using DotNetMvcWeb.Models;
using Xunit;

namespace DotNetMvcWeb.Tests.Models
{
    public class ErrorViewModelTests
    {
        // 1. 正向測試 (Positive Tests)
        [Fact]
        public void ShowRequestId_WhenRequestIdIsNotNullOrEmpty_ReturnsTrue()
        {
            // Arrange
            ErrorViewModel model = new()
            {
                RequestId = "REQ-123456",
                StatusCode = 500
            };

            // Act & Assert
            Assert.True(model.ShowRequestId);
            Assert.Equal("REQ-123456", model.RequestId);
            Assert.Equal(500, model.StatusCode);
        }

        // 2. 反向與邊界測試 (Negative & Boundary Tests)
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShowRequestId_WhenRequestIdIsNullOrEmpty_ReturnsFalse(string? requestId)
        {
            // Arrange
            ErrorViewModel model = new()
            {
                RequestId = requestId
            };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void StatusCode_WhenNotSet_IsNullByDefault()
        {
            // Arrange
            ErrorViewModel model = new();

            // Act & Assert
            Assert.Null(model.StatusCode);
            Assert.Null(model.RequestId);
            Assert.False(model.ShowRequestId);
        }
    }
}
