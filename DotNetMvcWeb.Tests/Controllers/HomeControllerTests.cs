using System.Diagnostics;
using DotNetMvcWeb.Controllers;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers
{
    public class HomeControllerTests
    {
        // 1. 正向測試 (Positive Tests)
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            HomeController controller = new();

            // Act
            IActionResult result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Arrange
            HomeController controller = new();

            // Act
            IActionResult result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // 2. 邊界測試 (Boundary Tests) - Error with TraceIdentifier vs Activity
        [Fact]
        public void Error_WithoutCurrentActivity_ReturnsViewWithErrorViewModelUsingTraceIdentifier()
        {
            // Arrange
            HomeController controller = new();
            DefaultHttpContext httpContext = new();
            httpContext.TraceIdentifier = "TRACE-999";
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            IActionResult result = controller.Error(404);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            ErrorViewModel model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal("TRACE-999", model.RequestId);
            Assert.Equal(404, model.StatusCode);
        }

        [Fact]
        public void Error_WithCurrentActivity_ReturnsViewWithErrorViewModelUsingActivityId()
        {
            // Arrange
            Activity activity = new("TestActivity");
            activity.Start();

            try
            {
                HomeController controller = new();
                DefaultHttpContext httpContext = new();
                httpContext.TraceIdentifier = "FALLBACK-TRACE";
                controller.ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                };

                // Act
                IActionResult result = controller.Error(null);

                // Assert
                ViewResult viewResult = Assert.IsType<ViewResult>(result);
                ErrorViewModel model = Assert.IsType<ErrorViewModel>(viewResult.Model);
                Assert.Equal(activity.Id, model.RequestId);
                Assert.Null(model.StatusCode);
            }
            finally
            {
                activity.Stop();
            }
        }
    }
}
