using DotNetMvcWeb.Controllers;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers.Api
{
    public class OracleDemoApiControllerTests
    {
        private readonly Mock<IOracleDemoItemService> _serviceMock;
        private readonly OracleDemoApiController _controller;

        public OracleDemoApiControllerTests()
        {
            _serviceMock = new Mock<IOracleDemoItemService>();
            _controller = new OracleDemoApiController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetItems_ReturnsOkWithItems()
        {
            List<OracleDemoItem> items = new() { new OracleDemoItem { Id = 1, Name = "Item1" } };
            _serviceMock.Setup(s => s.GetItemsAsync("key")).ReturnsAsync(items);

            ActionResult<IEnumerable<OracleDemoItem>> result = await _controller.GetItems("key");

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(items, ok.Value);
        }

        [Fact]
        public async Task GetItem_WhenExists_ReturnsOkWithItem()
        {
            OracleDemoItem item = new() { Id = 1, Name = "Item1" };
            _serviceMock.Setup(s => s.GetItemByIdAsync(1, true)).ReturnsAsync(item);

            ActionResult<OracleDemoItem> result = await _controller.GetItem(1);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(item, ok.Value);
        }

        [Fact]
        public async Task GetItem_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetItemByIdAsync(999, true)).ReturnsAsync((OracleDemoItem?)null);

            ActionResult<OracleDemoItem> result = await _controller.GetItem(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateItem_WhenValid_ReturnsCreatedAtAction()
        {
            OracleDemoItem item = new() { Id = 10, Name = "NewItem" };
            _serviceMock.Setup(s => s.CreateItemAsync(item)).Returns(Task.CompletedTask);

            ActionResult<OracleDemoItem> result = await _controller.CreateItem(item);

            CreatedAtActionResult created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(OracleDemoApiController.GetItem), created.ActionName);
            Assert.Equal(10, created.RouteValues?["id"]);
            Assert.Same(item, created.Value);
        }

        [Fact]
        public async Task UpdateItem_WhenIdMismatches_ReturnsBadRequest()
        {
            OracleDemoItem item = new() { Id = 5, Name = "Item5" };
            IActionResult result = await _controller.UpdateItem(10, item);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateItem_WhenValid_UpdatesAndReturnsNoContent()
        {
            OracleDemoItem item = new() { Id = 5, Name = "Item5" };
            _serviceMock.Setup(s => s.UpdateItemAsync(item)).Returns(Task.CompletedTask);

            IActionResult result = await _controller.UpdateItem(5, item);

            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.UpdateItemAsync(item), Times.Once);
        }

        [Fact]
        public async Task UpdateItem_WhenConcurrencyExceptionAndNotExists_ReturnsNotFound()
        {
            OracleDemoItem item = new() { Id = 5, Name = "Item5" };
            _serviceMock.Setup(s => s.UpdateItemAsync(item)).ThrowsAsync(new DbUpdateConcurrencyException());
            _serviceMock.Setup(s => s.ItemExists(5)).Returns(false);

            IActionResult result = await _controller.UpdateItem(5, item);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateItem_WhenConcurrencyExceptionAndExists_Rethrows()
        {
            OracleDemoItem item = new() { Id = 5, Name = "Item5" };
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
        public async Task DeleteItem_WhenExists_DeletesAndReturnsNoContent()
        {
            _serviceMock.Setup(s => s.ItemExists(1)).Returns(true);
            _serviceMock.Setup(s => s.DeleteItemAsync(1)).Returns(Task.CompletedTask);

            IActionResult result = await _controller.DeleteItem(1);

            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.DeleteItemAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetItemsViaAdoNet_WhenSuccess_ReturnsOkList()
        {
            List<OracleDemoItem> items = new() { new OracleDemoItem { Id = 1, Name = "Item1" } };
            _serviceMock.Setup(s => s.GetItemsViaAdoNetAsync("test")).ReturnsAsync(items);

            ActionResult<IEnumerable<OracleDemoItem>> result = await _controller.GetItemsViaAdoNet("test");

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(items, ok.Value);
        }

        [Fact]
        public async Task GetItemsViaAdoNet_WhenServiceThrows_ReturnsStatusCode500()
        {
            _serviceMock.Setup(s => s.GetItemsViaAdoNetAsync(It.IsAny<string>()))
                        .ThrowsAsync(new Exception("DB connection failed"));

            ActionResult<IEnumerable<OracleDemoItem>> result = await _controller.GetItemsViaAdoNet("test");

            ObjectResult errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Theory]
        [InlineData("New desc", true)]
        [InlineData("", false)]
        [InlineData("  ", false)]
        [InlineData(null, false)]
        public async Task UpdateDescriptionViaProcedure_ValidatesDescriptionAndInvokesService(string? desc, bool isValid)
        {
            IActionResult result = await _controller.UpdateDescriptionViaProcedure(5, desc!);

            if (isValid)
            {
                Assert.IsType<NoContentResult>(result);
                _serviceMock.Verify(s => s.UpdateItemDescriptionViaProcAsync(5, desc!), Times.Once);
            }
            else
            {
                Assert.IsType<BadRequestObjectResult>(result);
                _serviceMock.Verify(s => s.UpdateItemDescriptionViaProcAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            }
        }
    }
}
