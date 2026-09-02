using DotNetMvcWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers
{
    public class ExtensionsDemoControllerTests
    {
        [Fact]
        public void Index_Get_SetsViewBagDemoDataAndReturnsView()
        {
            // Arrange
            ExtensionsDemoController controller = new();

            // Act
            IActionResult result = controller.Index();

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.OriginalText);
            Assert.NotNull(controller.ViewBag.ShortText);
            Assert.EndsWith("...", (string)controller.ViewBag.ShortText);
        }

        [Fact]
        public void Index_Post_ProcessesTruncationAndSetsViewBag()
        {
            // Arrange
            ExtensionsDemoController controller = new();

            // Act
            IActionResult result = controller.Index("這是一個測試用長字串", 4);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("這是一個測試用長字串", (string)controller.ViewBag.TestInput);
            Assert.Equal(4, (int)controller.ViewBag.TestLength);
            Assert.Equal("這是一個...", (string)controller.ViewBag.TestResult);
            Assert.NotNull(controller.ViewBag.OriginalText);
        }

        [Fact]
        public void Index_Post_WithNullInput_SetsEmptyResult()
        {
            // Arrange
            ExtensionsDemoController controller = new();

            // Act
            IActionResult result = controller.Index(null!, 5);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal(string.Empty, (string)controller.ViewBag.TestResult);
        }
    }
}
