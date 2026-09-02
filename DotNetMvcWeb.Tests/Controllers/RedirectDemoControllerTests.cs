using DotNetMvcWeb.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers
{
    public class RedirectDemoControllerTests
    {
        [Fact]
        public void DemoList_ReturnsView()
        {
            // Arrange
            RedirectDemoController controller = new();

            // Act
            IActionResult result = controller.DemoList();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ExternalRedirectDemo_ReturnsRedirectResultToGoogle()
        {
            // Arrange
            RedirectDemoController controller = new();

            // Act
            IActionResult result = controller.ExternalRedirectDemo();

            // Assert
            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://www.google.com", redirect.Url);
            Assert.False(redirect.Permanent);
        }

        [Fact]
        public void PermanentRedirectDemo_ReturnsPermanentRedirectResult()
        {
            // Arrange
            RedirectDemoController controller = new();

            // Act
            IActionResult result = controller.PermanentRedirectDemo();

            // Assert
            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://www.google.com", redirect.Url);
            Assert.True(redirect.Permanent);
        }

        [Fact]
        public void RedirectToActionDemo_ReturnsRedirectWithRouteValues()
        {
            // Arrange
            RedirectDemoController controller = new();

            // Act
            IActionResult result = controller.RedirectToActionDemo();

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(RedirectDemoController.TargetAction), redirect.ActionName);
            Assert.NotNull(redirect.RouteValues);
            Assert.Equal(123, redirect.RouteValues["id"]);
            Assert.Equal("Hello from RedirectToAction", redirect.RouteValues["message"]);
        }

        [Fact]
        public void RedirectToOtherController_ReturnsRedirectToHomeIndex()
        {
            // Arrange
            RedirectDemoController controller = new();

            // Act
            IActionResult result = controller.RedirectToOtherController();

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public void LocalRedirectDemo_WhenUrlIsLocal_ReturnsLocalRedirectResult()
        {
            // Arrange
            RedirectDemoController controller = new();
            Mock<IUrlHelper> urlHelperMock = new();
            urlHelperMock.Setup(u => u.IsLocalUrl("/Home/Index")).Returns(true);
            controller.Url = urlHelperMock.Object;

            // Act
            IActionResult result = controller.LocalRedirectDemo("/Home/Index");

            // Assert
            LocalRedirectResult localRedirect = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal("/Home/Index", localRedirect.Url);
        }

        [Fact]
        public void LocalRedirectDemo_WhenUrlIsExternal_ThrowsInvalidOperationException()
        {
            // Arrange
            RedirectDemoController controller = new();
            Mock<IUrlHelper> urlHelperMock = new();
            urlHelperMock.Setup(u => u.IsLocalUrl("https://evil.com")).Returns(false);
            controller.Url = urlHelperMock.Object;

            // Act & Assert
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => controller.LocalRedirectDemo("https://evil.com"));
            Assert.Contains("發現非本地的外部網址", ex.Message);
        }

        [Fact]
        public void HtmxRedirectDemo_SetsHxRedirectHeaderAndReturnsOk()
        {
            // Arrange
            RedirectDemoController controller = new();
            DefaultHttpContext httpContext = new();
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            IActionResult result = controller.HtmxRedirectDemo();

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.True(httpContext.Response.Headers.ContainsKey("HX-Redirect"));
            Assert.Equal("/Home/Index", httpContext.Response.Headers["HX-Redirect"].ToString());
        }

        [Fact]
        public void TargetAction_SetsViewBagAndReturnsView()
        {
            // Arrange
            RedirectDemoController controller = new();

            // Act
            IActionResult result = controller.TargetAction(456, "測試訊息");

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(456, controller.ViewBag.Id);
            Assert.Equal("測試訊息", controller.ViewBag.Message);
        }
    }
}
