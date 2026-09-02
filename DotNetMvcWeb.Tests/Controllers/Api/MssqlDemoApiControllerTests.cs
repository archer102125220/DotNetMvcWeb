using DotNetMvcWeb.Controllers.Api;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers.Api
{
    public class MssqlDemoApiControllerTests
    {
        private readonly Mock<IMssqlDemoItemService> _serviceMock;
        private readonly MssqlDemoApiController _controller;

        public MssqlDemoApiControllerTests()
        {
            _serviceMock = new Mock<IMssqlDemoItemService>();
            _controller = new MssqlDemoApiController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetItems_ReturnsOkWithItems()
        {
            List<MssqlDemoItem> items = new() { new MssqlDemoItem { Id = 1, Name = "Item1" } };
            _serviceMock.Setup(s => s.GetItemsAsync("key")).ReturnsAsync(items);

            ActionResult<IEnumerable<MssqlDemoItem>> result = await _controller.GetItems("key");

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(items, ok.Value);
        }

        [Fact]
        public async Task GetItem_WhenExists_ReturnsOkWithItem()
        {
            MssqlDemoItem item = new() { Id = 1, Name = "Item1" };
            _serviceMock.Setup(s => s.GetItemByIdAsync(1, true)).ReturnsAsync(item);

            ActionResult<MssqlDemoItem> result = await _controller.GetItem(1);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(item, ok.Value);
        }

        [Fact]
        public async Task GetItem_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetItemByIdAsync(999, true)).ReturnsAsync((MssqlDemoItem?)null);

            ActionResult<MssqlDemoItem> result = await _controller.GetItem(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateItem_WhenValid_ReturnsCreatedAtAction()
        {
            MssqlDemoItem item = new() { Id = 10, Name = "NewItem" };
            _serviceMock.Setup(s => s.CreateItemAsync(item)).Returns(Task.CompletedTask);

            ActionResult<MssqlDemoItem> result = await _controller.CreateItem(item);

            CreatedAtActionResult created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(MssqlDemoApiController.GetItem), created.ActionName);
            Assert.Equal(10, created.RouteValues?["id"]);
            Assert.Same(item, created.Value);
        }

        [Fact]
        public async Task UpdateItem_WhenIdMismatches_ReturnsBadRequest()
        {
            MssqlDemoItem item = new() { Id = 5, Name = "Item5" };
            IActionResult result = await _controller.UpdateItem(10, item);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateItem_WhenValid_UpdatesAndReturnsNoContent()
        {
            MssqlDemoItem item = new() { Id = 5, Name = "Item5" };
            _serviceMock.Setup(s => s.UpdateItemAsync(item)).Returns(Task.CompletedTask);

            IActionResult result = await _controller.UpdateItem(5, item);

            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.UpdateItemAsync(item), Times.Once);
        }

        [Fact]
        public async Task UpdateItem_WhenConcurrencyExceptionAndNotExists_ReturnsNotFound()
        {
            MssqlDemoItem item = new() { Id = 5, Name = "Item5" };
            _serviceMock.Setup(s => s.UpdateItemAsync(item)).ThrowsAsync(new DbUpdateConcurrencyException());
            _serviceMock.Setup(s => s.ItemExists(5)).Returns(false);

            IActionResult result = await _controller.UpdateItem(5, item);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateItem_WhenConcurrencyExceptionAndExists_Rethrows()
        {
            MssqlDemoItem item = new() { Id = 5, Name = "Item5" };
            _serviceMock.Setup(s => s.UpdateItemAsync(item)).ThrowsAsync(new DbUpdateConcurrencyException());
            _serviceMock.Setup(s => s.ItemExists(5)).Returns(true);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.UpdateItem(5, item));
        }

        [Fact]
        public async Task DeleteItem_WhenNotExists_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.ItemExists(99)).Returns(false);

            IActionResult result = await _controller.DeleteItem(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteItem_WhenExists_DeletesAndReturnsOk()
        {
            _serviceMock.Setup(s => s.ItemExists(1)).Returns(true);
            _serviceMock.Setup(s => s.DeleteItemAsync(1)).Returns(Task.CompletedTask);

            IActionResult result = await _controller.DeleteItem(1);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.Verify(s => s.DeleteItemAsync(1), Times.Once);
        }

        [Fact]
        public async Task AdoNetDemo_WhenSuccess_ReturnsFormattedList()
        {
            List<MssqlDemoItem> items = new()
            {
                new MssqlDemoItem
                {
                    Id = 1,
                    Name = "Item1",
                    Description = "Desc1",
                    CreatedAt = DateTime.UtcNow,
                    CategoryId = 2,
                    Category = new MssqlDemoCategory { Name = "Cat2" }
                }
            };
            _serviceMock.Setup(s => s.GetItemsViaAdoNetAsync("test")).ReturnsAsync(items);

            IActionResult result = await _controller.AdoNetDemo("test");

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task AdoNetDemo_WhenServiceThrows_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.GetItemsViaAdoNetAsync(It.IsAny<string>()))
                        .ThrowsAsync(new Exception("DB connection failed"));

            IActionResult result = await _controller.AdoNetDemo("test");

            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }
    }
}
