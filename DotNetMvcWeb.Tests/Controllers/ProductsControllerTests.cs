using DotNetMvcWeb.Controllers;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers
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

        // 1. 建構子測試
        [Fact]
        public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductsController(null!));
        }

        // 2. Index
        [Fact]
        public async Task Index_ReturnsViewWithProducts()
        {
            // Arrange
            List<Product> products = new() { new Product { Id = 1, Name = "P1", Price = 100 } };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            // Act
            IActionResult result = await _controller.Index();

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            IEnumerable<Product> model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Single(model);
        }

        // 3. Details
        [Fact]
        public async Task Details_WhenProductExists_ReturnsViewWithProduct()
        {
            // Arrange
            Product product = new() { Id = 1, Name = "P1", Price = 100 };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            IActionResult result = await _controller.Details(1);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Product model = Assert.IsType<Product>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Details_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act
            IActionResult result = await _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // 4. Create
        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Act
            IActionResult result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_WhenProductIsNull_ReturnsBadRequest()
        {
            // Act
            IActionResult result = await _controller.Create(null!);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsValid_AddsProductAndRedirects()
        {
            // Arrange
            Product product = new() { Id = 1, Name = "NewProduct", Price = 500 };
            _repositoryMock.Setup(r => r.AddAsync(product)).Returns(Task.CompletedTask);

            // Act
            IActionResult result = await _controller.Create(product);

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductsController.Index), redirect.ActionName);
            _repositoryMock.Verify(r => r.AddAsync(product), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsViewWithProduct()
        {
            // Arrange
            Product product = new() { Id = 1, Name = "", Price = 500 };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            IActionResult result = await _controller.Create(product);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(product, viewResult.Model);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        }

        // 5. Edit
        [Fact]
        public async Task Edit_Get_WhenProductExists_ReturnsView()
        {
            // Arrange
            Product product = new() { Id = 2, Name = "P2", Price = 200 };
            _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(product);

            // Act
            IActionResult result = await _controller.Edit(2);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(product, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Get_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act
            IActionResult result = await _controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenProductIsNull_ReturnsBadRequest()
        {
            // Act
            IActionResult result = await _controller.Edit(1, null!);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenIdMismatches_ReturnsNotFound()
        {
            // Arrange
            Product product = new() { Id = 2, Name = "P2", Price = 200 };

            // Act
            IActionResult result = await _controller.Edit(1, product);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenValid_UpdatesAndRedirects()
        {
            // Arrange
            Product product = new() { Id = 2, Name = "Updated", Price = 250 };
            _repositoryMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);

            // Act
            IActionResult result = await _controller.Edit(2, product);

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductsController.Index), redirect.ActionName);
            _repositoryMock.Verify(r => r.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsView()
        {
            // Arrange
            Product product = new() { Id = 2, Name = "", Price = 250 };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            IActionResult result = await _controller.Edit(2, product);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(product, viewResult.Model);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        // 6. Delete
        [Fact]
        public async Task Delete_Get_WhenProductExists_ReturnsView()
        {
            // Arrange
            Product product = new() { Id = 3, Name = "P3", Price = 300 };
            _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(product);

            // Act
            IActionResult result = await _controller.Delete(3);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(product, viewResult.Model);
        }

        [Fact]
        public async Task Delete_Get_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act
            IActionResult result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DeletesAndRedirects()
        {
            // Arrange
            _repositoryMock.Setup(r => r.DeleteAsync(3)).Returns(Task.CompletedTask);

            // Act
            IActionResult result = await _controller.DeleteConfirmed(3);

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductsController.Index), redirect.ActionName);
            _repositoryMock.Verify(r => r.DeleteAsync(3), Times.Once);
        }
    }
}
