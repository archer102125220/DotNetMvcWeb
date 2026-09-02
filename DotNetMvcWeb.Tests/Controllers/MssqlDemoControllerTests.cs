using DotNetMvcWeb.Controllers;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers
{
    public class MssqlDemoControllerTests
    {
        private readonly Mock<IMssqlDemoItemService> _itemServiceMock;
        private readonly Mock<IMssqlDemoCategoryService> _categoryServiceMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly MssqlDemoController _controller;
        private readonly DefaultHttpContext _httpContext;

        public MssqlDemoControllerTests()
        {
            _itemServiceMock = new Mock<IMssqlDemoItemService>();
            _categoryServiceMock = new Mock<IMssqlDemoCategoryService>();
            _configMock = new Mock<IConfiguration>();

            _controller = new MssqlDemoController(
                _itemServiceMock.Object,
                _categoryServiceMock.Object,
                _configMock.Object);

            _httpContext = new DefaultHttpContext();
            Mock<IUrlHelper> urlHelperMock = new();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/MssqlDemo");

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            };
            _controller.Url = urlHelperMock.Object;
        }

        // 1. Index 測試 (標準 vs HTMX)
        [Fact]
        public async Task Index_WithoutHtmx_ReturnsFullViewWithItems()
        {
            // Arrange
            List<MssqlDemoItem> items = new() { new MssqlDemoItem { Id = 1, Name = "Item1" } };
            _itemServiceMock.Setup(s => s.GetItemsAsync("test")).ReturnsAsync(items);

            // Act
            IActionResult result = await _controller.Index("test");

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(items, viewResult.Model);
            Assert.Equal("test", _controller.ViewBag.Keyword);
        }

        [Fact]
        public async Task Index_WithHtmx_ReturnsPartialView()
        {
            // Arrange
            _httpContext.Request.Headers["HX-Request"] = "true";
            List<MssqlDemoItem> items = new() { new MssqlDemoItem { Id = 1, Name = "Item1" } };
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(items);

            // Act
            IActionResult result = await _controller.Index(null);

            // Assert
            PartialViewResult partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DemoList", partialResult.ViewName);
            Assert.Same(items, partialResult.Model);
        }

        // 2. Create GET 測試 (標準 vs HTMX)
        [Fact]
        public async Task Create_Get_WithHtmx_ReturnsPartialView()
        {
            // Arrange
            _httpContext.Request.Headers["HX-Request"] = "true";
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.Create();

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.IsType<MssqlDemoItem>(partial.Model);
        }

        [Fact]
        public async Task Create_Get_WithoutHtmx_ReturnsIndexViewWithActiveItem()
        {
            // Arrange
            List<MssqlDemoItem> items = new() { new MssqlDemoItem { Id = 1, Name = "Item1" } };
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(items);

            // Act
            IActionResult result = await _controller.Create();

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            Assert.True((bool)_controller.ViewBag.IsCreate);
            Assert.NotNull(_controller.ViewBag.ActiveItem);
        }

        // 3. Create POST 測試 (有效 vs 無效)
        [Fact]
        public async Task Create_Post_WhenValid_CreatesItemAndPushesUrl()
        {
            // Arrange
            MssqlDemoItem item = new() { Name = "NewItem" };
            _itemServiceMock.Setup(s => s.CreateItemAsync(item)).Returns(Task.CompletedTask);
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<MssqlDemoItem>());

            // Act
            IActionResult result = await _controller.Create(item);

            // Assert
            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _itemServiceMock.Verify(s => s.CreateItemAsync(item), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenInvalid_SetsRetargetHeadersAndReturnsPartial()
        {
            // Arrange
            MssqlDemoItem item = new() { Name = "" };
            _controller.ModelState.AddModelError("Name", "Name is required");
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.Create(item);

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Equal("#mssql-demo-form-container", _httpContext.Response.Headers["HX-Retarget"].ToString());
            Assert.Equal("innerHTML", _httpContext.Response.Headers["HX-Reswap"].ToString());
        }

        // 4. Edit GET 測試
        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            // Act
            IActionResult result = await _controller.Edit((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenItemNotFound_ReturnsNotFound()
        {
            // Arrange
            _itemServiceMock.Setup(s => s.GetItemByIdAsync(999, false)).ReturnsAsync((MssqlDemoItem?)null);

            // Act
            IActionResult result = await _controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WithHtmx_ReturnsPartialView()
        {
            // Arrange
            _httpContext.Request.Headers["HX-Request"] = "true";
            MssqlDemoItem item = new() { Id = 5, Name = "Item5" };
            _itemServiceMock.Setup(s => s.GetItemByIdAsync(5, false)).ReturnsAsync(item);
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.Edit(5);

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Same(item, partial.Model);
        }

        [Fact]
        public async Task Edit_Get_WithoutHtmx_ReturnsIndexViewWithActiveItem()
        {
            // Arrange
            MssqlDemoItem item = new() { Id = 5, Name = "Item5" };
            _itemServiceMock.Setup(s => s.GetItemByIdAsync(5, false)).ReturnsAsync(item);
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<MssqlDemoItem>());

            // Act
            IActionResult result = await _controller.Edit(5);

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            Assert.True((bool)_controller.ViewBag.IsEdit);
            Assert.Same(item, _controller.ViewBag.ActiveItem);
        }

        // 5. Edit POST 測試
        [Fact]
        public async Task Edit_Post_WhenIdMismatches_ReturnsNotFound()
        {
            // Arrange
            MssqlDemoItem item = new() { Id = 10, Name = "Item10" };

            // Act
            IActionResult result = await _controller.Edit(9, item);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenValid_UpdatesAndPushesUrl()
        {
            // Arrange
            MssqlDemoItem item = new() { Id = 10, Name = "Item10" };
            _itemServiceMock.Setup(s => s.UpdateItemAsync(item)).Returns(Task.CompletedTask);
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<MssqlDemoItem>());

            // Act
            IActionResult result = await _controller.Edit(10, item);

            // Assert
            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _itemServiceMock.Verify(s => s.UpdateItemAsync(item), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_WhenConcurrencyExceptionAndItemNotExists_ReturnsNotFound()
        {
            // Arrange
            MssqlDemoItem item = new() { Id = 10, Name = "Item10" };
            _itemServiceMock.Setup(s => s.UpdateItemAsync(item)).ThrowsAsync(new DbUpdateConcurrencyException());
            _itemServiceMock.Setup(s => s.ItemExists(10)).Returns(false);

            // Act
            IActionResult result = await _controller.Edit(10, item);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenConcurrencyExceptionAndItemExists_RethrowsException()
        {
            // Arrange
            MssqlDemoItem item = new() { Id = 10, Name = "Item10" };
            _itemServiceMock.Setup(s => s.UpdateItemAsync(item)).ThrowsAsync(new DbUpdateConcurrencyException());
            _itemServiceMock.Setup(s => s.ItemExists(10)).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.Edit(10, item));
        }

        [Fact]
        public async Task Edit_Post_WhenInvalid_SetsRetargetAndReturnsPartial()
        {
            // Arrange
            MssqlDemoItem item = new() { Id = 10, Name = "" };
            _controller.ModelState.AddModelError("Name", "Name is required");
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.Edit(10, item);

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Equal("#mssql-demo-form-container", _httpContext.Response.Headers["HX-Retarget"].ToString());
        }

        // 6. DeleteConfirmed
        [Fact]
        public async Task DeleteConfirmed_DeletesItemAndPushesUrl()
        {
            // Arrange
            _itemServiceMock.Setup(s => s.DeleteItemAsync(12)).Returns(Task.CompletedTask);
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<MssqlDemoItem>());

            // Act
            IActionResult result = await _controller.DeleteConfirmed(12);

            // Assert
            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _itemServiceMock.Verify(s => s.DeleteItemAsync(12), Times.Once);
        }

        // 7. UpdateDescriptionViaProcedure
        [Theory]
        [InlineData("New description text", true)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData(null, false)]
        public async Task UpdateDescriptionViaProcedure_InvokesServiceOnlyWhenDescriptionIsNotBlank(string? desc, bool shouldCall)
        {
            // Arrange
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<MssqlDemoItem>());

            // Act
            IActionResult result = await _controller.UpdateDescriptionViaProcedure(5, desc!);

            // Assert
            if (shouldCall)
            {
                _itemServiceMock.Verify(s => s.UpdateItemDescriptionViaProcAsync(5, desc!), Times.Once);
            }
            else
            {
                _itemServiceMock.Verify(s => s.UpdateItemDescriptionViaProcAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            }
        }

        // 8. AdoNetDemo
        [Fact]
        public async Task AdoNetDemo_ReturnsViewWithItems()
        {
            // Arrange
            List<MssqlDemoItem> items = new() { new MssqlDemoItem { Id = 1, Name = "AdoItem" } };
            _itemServiceMock.Setup(s => s.GetItemsViaAdoNetAsync("kw")).ReturnsAsync(items);

            // Act
            IActionResult result = await _controller.AdoNetDemo("kw");

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Same(items, view.Model);
            Assert.Equal("kw", _controller.ViewBag.Keyword);
        }
    }
}
