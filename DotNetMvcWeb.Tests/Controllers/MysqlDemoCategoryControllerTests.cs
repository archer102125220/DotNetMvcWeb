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
    public class MysqlDemoCategoryControllerTests
    {
        private readonly Mock<IMysqlDemoCategoryService> _categoryServiceMock;
        private readonly MysqlDemoCategoryController _controller;
        private readonly DefaultHttpContext _httpContext;

        public MysqlDemoCategoryControllerTests()
        {
            _categoryServiceMock = new Mock<IMysqlDemoCategoryService>();
            _controller = new MysqlDemoCategoryController(_categoryServiceMock.Object);

            _httpContext = new DefaultHttpContext();
            Mock<IUrlHelper> urlHelperMock = new();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/MysqlDemoCategory");

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            };
            _controller.Url = urlHelperMock.Object;
        }

        [Fact]
        public async Task Index_WithoutHtmx_ReturnsViewWithCategories()
        {
            List<MysqlDemoCategory> categories = new() { new MysqlDemoCategory { Id = 1, Name = "Cat1" } };
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

            IActionResult result = await _controller.Index();

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Same(categories, view.Model);
        }

        [Fact]
        public async Task Index_WithHtmx_ReturnsPartialView()
        {
            _httpContext.Request.Headers["HX-Request"] = "true";
            List<MysqlDemoCategory> categories = new() { new MysqlDemoCategory { Id = 1, Name = "Cat1" } };
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

            IActionResult result = await _controller.Index();

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CategoryList", partial.ViewName);
        }

        [Fact]
        public async Task Create_Get_WithHtmx_ReturnsPartialView()
        {
            _httpContext.Request.Headers["HX-Request"] = "true";
            IActionResult result = await _controller.Create();
            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
        }

        [Fact]
        public async Task Create_Get_WithoutHtmx_ReturnsIndexView()
        {
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MysqlDemoCategory>());
            IActionResult result = await _controller.Create();
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            Assert.True((bool)_controller.ViewBag.IsCreate);
        }

        [Fact]
        public async Task Create_Post_WhenValid_CreatesCategoryAndPushesUrl()
        {
            MysqlDemoCategory item = new() { Name = "NewCat" };
            _categoryServiceMock.Setup(s => s.CreateCategoryAsync(item)).Returns(Task.CompletedTask);
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MysqlDemoCategory>());

            IActionResult result = await _controller.Create(item);

            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _categoryServiceMock.Verify(s => s.CreateCategoryAsync(item), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WhenInvalid_SetsRetargetHeadersAndReturnsPartial()
        {
            MysqlDemoCategory item = new() { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            IActionResult result = await _controller.Create(item);

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Equal("#mysql-demo-category-form-container", _httpContext.Response.Headers["HX-Retarget"].ToString());
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
            _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(999)).ReturnsAsync((MysqlDemoCategory?)null);
            IActionResult result = await _controller.Edit(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WithHtmx_ReturnsPartialView()
        {
            _httpContext.Request.Headers["HX-Request"] = "true";
            MysqlDemoCategory cat = new() { Id = 1, Name = "Cat1" };
            _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(cat);

            IActionResult result = await _controller.Edit(1);

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
        }

        [Fact]
        public async Task Edit_Get_WithoutHtmx_ReturnsIndexView()
        {
            MysqlDemoCategory cat = new() { Id = 1, Name = "Cat1" };
            _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(cat);
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MysqlDemoCategory>());

            IActionResult result = await _controller.Edit(1);

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            Assert.True((bool)_controller.ViewBag.IsEdit);
        }

        [Fact]
        public async Task Edit_Post_WhenIdMismatches_ReturnsNotFound()
        {
            MysqlDemoCategory cat = new() { Id = 2, Name = "Cat2" };
            IActionResult result = await _controller.Edit(1, cat);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenValid_UpdatesAndPushesUrl()
        {
            MysqlDemoCategory cat = new() { Id = 2, Name = "Cat2" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(cat)).Returns(Task.CompletedTask);
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MysqlDemoCategory>());

            IActionResult result = await _controller.Edit(2, cat);

            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
        }

        [Fact]
        public async Task Edit_Post_WhenConcurrencyExceptionAndNotExists_ReturnsNotFound()
        {
            MysqlDemoCategory cat = new() { Id = 2, Name = "Cat2" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(cat)).ThrowsAsync(new DbUpdateConcurrencyException());
            _categoryServiceMock.Setup(s => s.CategoryExists(2)).Returns(false);

            IActionResult result = await _controller.Edit(2, cat);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenConcurrencyExceptionAndExists_Rethrows()
        {
            MysqlDemoCategory cat = new() { Id = 2, Name = "Cat2" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(cat)).ThrowsAsync(new DbUpdateConcurrencyException());
            _categoryServiceMock.Setup(s => s.CategoryExists(2)).Returns(true);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.Edit(2, cat));
        }

        [Fact]
        public async Task Edit_Post_WhenInvalid_SetsRetargetAndReturnsPartial()
        {
            MysqlDemoCategory cat = new() { Id = 2, Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            IActionResult result = await _controller.Edit(2, cat);

            PartialViewResult partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateOrEdit", partial.ViewName);
            Assert.Equal("#mysql-demo-category-form-container", _httpContext.Response.Headers["HX-Retarget"].ToString());
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesCategoryAndPushesUrl()
        {
            _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(5)).Returns(Task.CompletedTask);
            _categoryServiceMock.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<MysqlDemoCategory>());

            IActionResult result = await _controller.DeleteConfirmed(5);

            Assert.True(_httpContext.Response.Headers.ContainsKey("HX-Push-Url"));
            _categoryServiceMock.Verify(s => s.DeleteCategoryAsync(5), Times.Once);
        }
    }
}
