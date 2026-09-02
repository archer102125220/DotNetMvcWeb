using DotNetMvcWeb.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Middlewares
{
    public class RequestTimingMiddlewareTests
    {
        // 1. 正向測試 (Positive Tests)
        [Fact]
        public async Task InvokeAsync_WhenValidHttpContext_ExecutesNextDelegateAndLogsExecutionTime()
        {
            // Arrange
            bool nextCalled = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            Mock<ILogger<RequestTimingMiddleware>> loggerMock = new();
            RequestTimingMiddleware middleware = new(next, loggerMock.Object);

            DefaultHttpContext context = new();
            context.Request.Method = "GET";
            context.Request.Path = "/api/test";

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(nextCalled);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request [GET] /api/test executed in")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // 2. 反向測試 (Negative Tests)
        [Fact]
        public void Constructor_WhenNextDelegateIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Mock<ILogger<RequestTimingMiddleware>> loggerMock = new();

            // Act & Assert
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new RequestTimingMiddleware(null!, loggerMock.Object));
            Assert.Equal("next", ex.ParamName);
        }

        [Fact]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;

            // Act & Assert
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new RequestTimingMiddleware(next, null!));
            Assert.Equal("logger", ex.ParamName);
        }

        [Fact]
        public async Task InvokeAsync_WhenHttpContextIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
            Mock<ILogger<RequestTimingMiddleware>> loggerMock = new();
            RequestTimingMiddleware middleware = new(next, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => middleware.InvokeAsync(null!));
        }

        // 3. 擴充方法測試 (Extension Method Tests)
        [Fact]
        public void UseRequestTiming_WhenBuilderIsValid_RegistersMiddleware()
        {
            // Arrange
            Mock<IApplicationBuilder> builderMock = new();
            builderMock.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                       .Returns(builderMock.Object);

            // Act
            IApplicationBuilder result = builderMock.Object.UseRequestTiming();

            // Assert
            Assert.NotNull(result);
            builderMock.Verify(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
        }

        [Fact]
        public void UseRequestTiming_WhenBuilderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IApplicationBuilder? nullBuilder = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullBuilder!.UseRequestTiming());
        }
    }
}
