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
    public class OracleDemoControllerTests
    {
        private readonly Mock<IOracleDemoItemService> _itemServiceMock;
        private readonly Mock<IOracleDemoCategoryService> _categoryServiceMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly OracleDemoController _controller;
        private readonly DefaultHttpContext _httpContext;

        public OracleDemoControllerTests()
        {
            _itemServiceMock = new Mock<IOracleDemoItemService>();
            _categoryServiceMock = new Mock<IOracleDemoCategoryService>();
            _configMock = new Mock<IConfiguration>();

            _controller = new OracleDemoController(
                _itemServiceMock.Object,
                _categoryServiceMock.Object,
                _configMock.Object);

            _httpContext = new DefaultHttpContext();
            Mock<IUrlHelper> urlHelperMock = new();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/OracleDemo");

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            };
            _controller.Url = urlHelperMock.Object;
        }

        [Fact]
        public async Task Index_WithoutHtmx_ReturnsFullView()
        {
            List<OracleDemoItem> items = new() { new OracleDemoItem { Id = 1, Name = "Ora1" } };
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(items);

            IActionResult result = await _controller.Index(null);

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Same(items, view.Model);
        }

        [Fact]
        public async Task Index_WithHtmx_ReturnsPartialView()
        {
            _httpContext.Request.Headers["HX-Request"] = "true";
            List<OracleDemoItem> items = new() { new OracleDemoItem { Id = 1, Name = "Ora1" } };
            _itemServiceMock.Setup(s => s.GetItemsAsync("kw")).ReturnsAsync(items);

            IActionResult result = await _controller.Index("kw");

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DemoList", partial.ViewName);
        }

        [Fact]
        public async Task Create_Get_WithHtmx_ReturnsPartialView()
        {
            _httpContext.Request.Headers["HX-Request"] = "true";
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<OracleDemoCategory>());

            IActionResult result = await _controller.Create();

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
        }

        [Fact]
        public async Task Create_Get_WithoutHtmx_ReturnsIndexView()
        {
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<OracleDemoCategory>());
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<OracleDemoItem>());

            IActionResult result = await _controller.Create();

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            Assert.True((bool)_controller.ViewBag.IsCreate);
        }

        [Fact]
        public async Task Create_Post_WhenValid_CreatesItemAndPushesUrl()
        {
            OracleDemoItem item = new() { Name = "NewItem" };
            _itemServiceMock.Setup(s => s.CreateItemAsync(item)).Returns(Task.CompletedTask);
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<OracleDemoItem>());

            IActionResult result = await _controller.Create(item);

            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _itemServiceMock.Verify(s => s.CreateItemAsync(item), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenInvalid_SetsRetargetAndReturnsPartial()
        {
            OracleDemoItem item = new() { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<OracleDemoCategory>());

            IActionResult result = await _controller.Create(item);

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Equal("#oracle-demo-form-container", _httpContext.Response.Headers["HX-Retarget"].ToString());
        }

        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            IActionResult result = await _controller.Edit((int?)null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsNotFound()
        {
            _itemServiceMock.Setup(s => s.GetItemByIdAsync(99, false)).ReturnsAsync((OracleDemoItem?)null);
            IActionResult result = await _controller.Edit(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WithHtmx_ReturnsPartialView()
        {
            _httpContext.Request.Headers["HX-Request"] = "true";
            OracleDemoItem item = new() { Id = 1, Name = "Item1" };
            _itemServiceMock.Setup(s => s.GetItemByIdAsync(1, false)).ReturnsAsync(item);
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<OracleDemoCategory>());

            IActionResult result = await _controller.Edit(1);

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
        }

        [Fact]
        public async Task Edit_Get_WithoutHtmx_ReturnsIndexView()
        {
            OracleDemoItem item = new() { Id = 1, Name = "Item1" };
            _itemServiceMock.Setup(s => s.GetItemByIdAsync(1, false)).ReturnsAsync(item);
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<OracleDemoCategory>());
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<OracleDemoItem>());

            IActionResult result = await _controller.Edit(1);

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            Assert.True((bool)_controller.ViewBag.IsEdit);
        }

        [Fact]
        public async Task Edit_Post_WhenIdMismatches_ReturnsNotFound()
        {
            OracleDemoItem item = new() { Id = 1, Name = "Item1" };
            IActionResult result = await _controller.Edit(2, item);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenValid_UpdatesAndPushesUrl()
        {
            OracleDemoItem item = new() { Id = 1, Name = "Item1" };
            _itemServiceMock.Setup(s => s.UpdateItemAsync(item)).Returns(Task.CompletedTask);
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<OracleDemoItem>());

            IActionResult result = await _controller.Edit(1, item);

            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
        }

        [Fact]
        public async Task Edit_Post_WhenConcurrencyExceptionAndNotExists_ReturnsNotFound()
        {
            OracleDemoItem item = new() { Id = 1, Name = "Item1" };
            _itemServiceMock.Setup(s => s.UpdateItemAsync(item)).ThrowsAsync(new DbUpdateConcurrencyException());
            _itemServiceMock.Setup(s => s.ItemExists(1)).Returns(false);

            IActionResult result = await _controller.Edit(1, item);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenConcurrencyExceptionAndExists_Rethrows()
        {
            OracleDemoItem item = new() { Id = 1, Name = "Item1" };
            _itemServiceMock.Setup(s => s.UpdateItemAsync(item)).ThrowsAsync(new DbUpdateConcurrencyException());
            _itemServiceMock.Setup(s => s.ItemExists(1)).Returns(true);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.Edit(1, item));
        }

        [Fact]
        public async Task Edit_Post_WhenInvalid_SetsRetargetAndReturnsPartial()
        {
            OracleDemoItem item = new() { Id = 1, Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");
            _categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(new List<OracleDemoCategory>());

            IActionResult result = await _controller.Edit(1, item);

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Equal("#oracle-demo-form-container", _httpContext.Response.Headers["HX-Retarget"].ToString());
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesItemAndPushesUrl()
        {
            _itemServiceMock.Setup(s => s.DeleteItemAsync(1)).Returns(Task.CompletedTask);
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<OracleDemoItem>());

            IActionResult result = await _controller.DeleteConfirmed(1);

            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _itemServiceMock.Verify(s => s.DeleteItemAsync(1), Times.Once);
        }

        [Theory]
        [InlineData("New desc", true)]
        [InlineData("", false)]
        [InlineData("  ", false)]
        [InlineData(null, false)]
        public async Task UpdateDescriptionViaProcedure_InvokesOnlyWhenNonEmpty(string? desc, bool shouldCall)
        {
            _itemServiceMock.Setup(s => s.GetItemsAsync(null)).ReturnsAsync(new List<OracleDemoItem>());

            IActionResult result = await _controller.UpdateDescriptionViaProcedure(1, desc!);

            if (shouldCall)
            {
                _itemServiceMock.Verify(s => s.UpdateItemDescriptionViaProcAsync(1, desc!), Times.Once);
            }
            else
            {
                _itemServiceMock.Verify(s => s.UpdateItemDescriptionViaProcAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            }
        }

        [Fact]
        public async Task AdoNetDemo_ReturnsViewWithItems()
        {
            List<OracleDemoItem> items = new() { new OracleDemoItem { Id = 1, Name = "AdoItem" } };
            _itemServiceMock.Setup(s => s.GetItemsViaAdoNetAsync("kw")).ReturnsAsync(items);

            IActionResult result = await _controller.AdoNetDemo("kw");

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Same(items, view.Model);
            Assert.Equal("kw", _controller.ViewBag.Keyword);
        }
    }
}
