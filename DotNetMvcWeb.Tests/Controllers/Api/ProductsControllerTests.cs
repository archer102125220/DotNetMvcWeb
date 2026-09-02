using DotNetMvcWeb.Controllers.Api;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers.Api
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _controller = new ProductsController(_repositoryMock.Object);
        }

        [Fact]
        public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductsController(null!));
        }

        [Fact]
        public async Task GetProducts_ReturnsOkWithProducts()
        {
            List<Product> products = new() { new Product { Id = 1, Name = "P1", Price = 100 } };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            ActionResult<IEnumerable<Product>> result = await _controller.GetProducts();

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            IEnumerable<Product> model = Assert.IsAssignableFrom<IEnumerable<Product>>(ok.Value);
            Assert.Single(model);
        }

        [Fact]
        public async Task GetProduct_WhenExists_ReturnsOkWithProduct()
        {
            Product product = new() { Id = 1, Name = "P1", Price = 100 };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            ActionResult<Product> result = await _controller.GetProduct(1);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            Product model = Assert.IsType<Product>(ok.Value);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task GetProduct_WhenNotFound_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            ActionResult<Product> result = await _controller.GetProduct(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_WhenProductIsNull_ReturnsBadRequest()
        {
            ActionResult<Product> result = await _controller.CreateProduct(null!);
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_WhenValid_ReturnsCreatedAtAction()
        {
            Product product = new() { Id = 5, Name = "P5", Price = 500 };
            _repositoryMock.Setup(r => r.AddAsync(product)).Returns(Task.CompletedTask);

            ActionResult<Product> result = await _controller.CreateProduct(product);

            CreatedAtActionResult created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ProductsController.GetProduct), created.ActionName);
            Assert.Equal(5, created.RouteValues?["id"]);
            Assert.Same(product, created.Value);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductIsNull_ReturnsBadRequest()
        {
            IActionResult result = await _controller.UpdateProduct(1, null!);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_WhenNotFound_ReturnsNotFound()
        {
            Product product = new() { Id = 999, Name = "P999" };
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            IActionResult result = await _controller.UpdateProduct(999, product);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_WhenValid_UpdatesAndReturnsNoContent()
        {
            Product product = new() { Id = 1, Name = "Updated" };
            Product existing = new() { Id = 1, Name = "Old" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
            _repositoryMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);

            IActionResult result = await _controller.UpdateProduct(1, product);

            Assert.IsType<NoContentResult>(result);
            _repositoryMock.Verify(r => r.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WhenNotFound_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            IActionResult result = await _controller.DeleteProduct(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_WhenValid_DeletesAndReturnsNoContent()
        {
            Product existing = new() { Id = 1, Name = "Old" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
            _repositoryMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            IActionResult result = await _controller.DeleteProduct(1);

            Assert.IsType<NoContentResult>(result);
            _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}
