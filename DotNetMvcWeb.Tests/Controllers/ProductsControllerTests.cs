using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Controllers;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Models.DTOs.Products;
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
        public async Task Index_ReturnsViewWithProductResponseDtos()
        {
            // Arrange
            List<Product> products = new()
            {
                new Product { Id = 1, Name = "P1", Price = 100, Description = "Desc1" }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            // Act
            IActionResult result = await _controller.Index();

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            IEnumerable<ProductResponseDto> model = Assert.IsAssignableFrom<IEnumerable<ProductResponseDto>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(1, model.First().Id);
            Assert.Equal("P1", model.First().Name);
        }

        // 3. Details
        [Fact]
        public async Task Details_WhenProductExists_ReturnsViewWithProductResponseDto()
        {
            // Arrange
            Product product = new() { Id = 1, Name = "P1", Price = 100, Description = "Desc1" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            IActionResult result = await _controller.Details(1);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            ProductResponseDto model = Assert.IsType<ProductResponseDto>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("P1", model.Name);
            Assert.Equal(100m, model.Price);
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
        public void Create_Get_ReturnsViewWithNewProductCreateDto()
        {
            // Act
            IActionResult result = _controller.Create();

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProductCreateDto>(viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_WhenDtoIsNull_ReturnsBadRequest()
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
            ProductCreateDto dto = new() { Name = "NewProduct", Price = 500, Description = "NewDesc" };
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            // Act
            IActionResult result = await _controller.Create(dto);

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductsController.Index), redirect.ActionName);
            _repositoryMock.Verify(r => r.AddAsync(It.Is<Product>(p => p.Name == "NewProduct" && p.Price == 500)), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsViewWithDto()
        {
            // Arrange
            ProductCreateDto dto = new() { Name = "", Price = 500 };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            IActionResult result = await _controller.Create(dto);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(dto, viewResult.Model);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        }

        // 5. Edit
        [Fact]
        public async Task Edit_Get_WhenProductExists_ReturnsViewWithProductUpdateDto()
        {
            // Arrange
            Product product = new() { Id = 2, Name = "P2", Price = 200, Description = "Desc2" };
            _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(product);

            // Act
            IActionResult result = await _controller.Edit(2);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            ProductUpdateDto model = Assert.IsType<ProductUpdateDto>(viewResult.Model);
            Assert.Equal(2, model.Id);
            Assert.Equal("P2", model.Name);
            Assert.Equal(200m, model.Price);
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
        public async Task Edit_Post_WhenDtoIsNull_ReturnsBadRequest()
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
            ProductUpdateDto dto = new() { Id = 2, Name = "P2", Price = 200 };

            // Act
            IActionResult result = await _controller.Edit(1, dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenValid_UpdatesAndRedirects()
        {
            // Arrange
            ProductUpdateDto dto = new() { Id = 2, Name = "Updated", Price = 250, Description = "UpdatedDesc" };
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            // Act
            IActionResult result = await _controller.Edit(2, dto);

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductsController.Index), redirect.ActionName);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == 2 && p.Name == "Updated" && p.Price == 250)), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsViewWithDto()
        {
            // Arrange
            ProductUpdateDto dto = new() { Id = 2, Name = "", Price = 250 };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            IActionResult result = await _controller.Edit(2, dto);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(dto, viewResult.Model);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        // 6. Delete
        [Fact]
        public async Task Delete_Get_WhenProductExists_ReturnsViewWithProductResponseDto()
        {
            // Arrange
            Product product = new() { Id = 3, Name = "P3", Price = 300, Description = "Desc3" };
            _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(product);

            // Act
            IActionResult result = await _controller.Delete(3);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            ProductResponseDto model = Assert.IsType<ProductResponseDto>(viewResult.Model);
            Assert.Equal(3, model.Id);
            Assert.Equal("P3", model.Name);
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
