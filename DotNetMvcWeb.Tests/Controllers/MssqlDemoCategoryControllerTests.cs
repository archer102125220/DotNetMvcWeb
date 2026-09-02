using DotNetMvcWeb.Controllers;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers
{
    public class MssqlDemoCategoryControllerTests
    {
        private readonly Mock<IMssqlDemoCategoryService> _categoryServiceMock;
        private readonly MssqlDemoCategoryController _controller;
        private readonly DefaultHttpContext _httpContext;

        public MssqlDemoCategoryControllerTests()
        {
            _categoryServiceMock = new Mock<IMssqlDemoCategoryService>();
            _controller = new MssqlDemoCategoryController(_categoryServiceMock.Object);

            _httpContext = new DefaultHttpContext();
            Mock<IUrlHelper> urlHelperMock = new();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/MssqlDemoCategory");

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            };
            _controller.Url = urlHelperMock.Object;
        }

        // 1. Index
        [Fact]
        public async Task Index_WithoutHtmx_ReturnsViewWithCategories()
        {
            // Arrange
            List<MssqlDemoCategory> categories = new() { new MssqlDemoCategory { Id = 1, Name = "Cat1" } };
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

            // Act
            IActionResult result = await _controller.Index();

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Same(categories, view.Model);
        }

        [Fact]
        public async Task Index_WithHtmx_ReturnsPartialView()
        {
            // Arrange
            _httpContext.Request.Headers["HX-Request"] = "true";
            List<MssqlDemoCategory> categories = new() { new MssqlDemoCategory { Id = 1, Name = "Cat1" } };
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

            // Act
            IActionResult result = await _controller.Index();

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CategoryList", partial.ViewName);
            Assert.Same(categories, partial.Model);
        }

        // 2. Create GET
        [Fact]
        public async Task Create_Get_WithHtmx_ReturnsPartialView()
        {
            // Arrange
            _httpContext.Request.Headers["HX-Request"] = "true";

            // Act
            IActionResult result = await _controller.Create();

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
        }

        [Fact]
        public async Task Create_Get_WithoutHtmx_ReturnsIndexView()
        {
            // Arrange
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.Create();

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            Assert.True((bool)_controller.ViewBag.IsCreate);
        }

        // 3. Create POST
        [Fact]
        public async Task Create_Post_WhenValid_CreatesCategoryAndPushesUrl()
        {
            // Arrange
            MssqlDemoCategory item = new() { Name = "NewCat" };
            _categoryServiceMock.Setup(s => s.CreateCategoryAsync(item)).Returns(Task.CompletedTask);
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.Create(item);

            // Assert
            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _categoryServiceMock.Verify(s => s.CreateCategoryAsync(item), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenInvalid_SetsRetargetHeadersAndReturnsPartial()
        {
            // Arrange
            MssqlDemoCategory item = new() { Name = "" };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            IActionResult result = await _controller.Create(item);

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Equal("#mssql-demo-category-form-container", _httpContext.Response.Headers["HX-Retarget"].ToString());
        }

        // 4. Edit GET
        [Fact]
        public async Task Edit_Get_WhenIdIsNull_ReturnsNotFound()
        {
            // Act
            IActionResult result = await _controller.Edit((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(999)).ReturnsAsync((MssqlDemoCategory?)null);

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
            MssqlDemoCategory cat = new() { Id = 1, Name = "Cat1" };
            _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(cat);

            // Act
            IActionResult result = await _controller.Edit(1);

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Same(cat, partial.Model);
        }

        [Fact]
        public async Task Edit_Get_WithoutHtmx_ReturnsIndexView()
        {
            // Arrange
            MssqlDemoCategory cat = new() { Id = 1, Name = "Cat1" };
            _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(cat);
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.Edit(1);

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            Assert.True((bool)_controller.ViewBag.IsEdit);
        }

        // 5. Edit POST
        [Fact]
        public async Task Edit_Post_WhenIdMismatches_ReturnsNotFound()
        {
            // Arrange
            MssqlDemoCategory cat = new() { Id = 2, Name = "Cat2" };

            // Act
            IActionResult result = await _controller.Edit(1, cat);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenValid_UpdatesAndPushesUrl()
        {
            // Arrange
            MssqlDemoCategory cat = new() { Id = 2, Name = "Cat2" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(cat)).Returns(Task.CompletedTask);
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.Edit(2, cat);

            // Assert
            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _categoryServiceMock.Verify(s => s.UpdateCategoryAsync(cat), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_WhenConcurrencyExceptionAndNotExists_ReturnsNotFound()
        {
            // Arrange
            MssqlDemoCategory cat = new() { Id = 2, Name = "Cat2" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(cat)).ThrowsAsync(new DbUpdateConcurrencyException());
            _categoryServiceMock.Setup(s => s.CategoryExists(2)).Returns(false);

            // Act
            IActionResult result = await _controller.Edit(2, cat);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenConcurrencyExceptionAndExists_Rethrows()
        {
            // Arrange
            MssqlDemoCategory cat = new() { Id = 2, Name = "Cat2" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(cat)).ThrowsAsync(new DbUpdateConcurrencyException());
            _categoryServiceMock.Setup(s => s.CategoryExists(2)).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.Edit(2, cat));
        }

        [Fact]
        public async Task Edit_Post_WhenInvalid_SetsRetargetAndReturnsPartial()
        {
            // Arrange
            MssqlDemoCategory cat = new() { Id = 2, Name = "" };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            IActionResult result = await _controller.Edit(2, cat);

            // Assert
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Equal("#mssql-demo-category-form-container", _httpContext.Response.Headers["HX-Retarget"].ToString());
        }

        // 6. DeleteConfirmed
        [Fact]
        public async Task DeleteConfirmed_DeletesCategoryAndPushesUrl()
        {
            // Arrange
            _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(5)).Returns(Task.CompletedTask);
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MssqlDemoCategory>());

            // Act
            IActionResult result = await _controller.DeleteConfirmed(5);

            // Assert
            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _categoryServiceMock.Verify(s => s.DeleteCategoryAsync(5), Times.Once);
        }
    }
}
