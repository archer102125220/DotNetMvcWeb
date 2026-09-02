using DotNetMvcWeb.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace DotNetMvcWeb.Tests.TagHelpers
{
    public class PageHeaderTagHelperTests
    {
        // 1. 正向測試 (Positive Tests) - 包含子內容按鈕
        [Fact]
        public async Task ProcessAsync_WhenChildContentProvided_GeneratesTitleAndActionsWrapper()
        {
            // Arrange
            PageHeaderTagHelper tagHelper = new()
            {
                Title = "商品管理"
            };

            TagHelperContext context = new(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test-id");

            TagHelperOutput output = new(
                "page-header",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) =>
                {
                    DefaultTagHelperContent tagHelperContent = new();
                    tagHelperContent.SetHtmlContent("<button class=\"btn btn-primary\">新增</button>");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.Equal("div", output.TagName);
            Assert.Equal("page-header d-flex justify-content-between align-items-center mb-4 flex-wrap", output.Attributes["class"].Value);
            string content = output.Content.GetContent();
            Assert.Contains("<h1 class=\"m-0\">商品管理</h1>", content);
            Assert.Contains("<div class=\"d-flex gap-2 align-items-center\"><button class=\"btn btn-primary\">新增</button></div>", content);
        }

        // 2. 邊界測試 (Boundary Tests) - 子內容為空
        [Fact]
        public async Task ProcessAsync_WhenChildContentIsEmpty_RendersOnlyTitleWithoutActionsWrapper()
        {
            // Arrange
            PageHeaderTagHelper tagHelper = new()
            {
                Title = "系統設定"
            };

            TagHelperContext context = new(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test-id");

            TagHelperOutput output = new(
                "page-header",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) =>
                {
                    DefaultTagHelperContent tagHelperContent = new();
                    tagHelperContent.SetHtmlContent("");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            string content = output.Content.GetContent();
            Assert.Equal("<h1 class=\"m-0\">系統設定</h1>", content);
        }

        // 3. 邊界測試 (Boundary Tests) - 子內容純空白與 Title 為空
        [Fact]
        public async Task ProcessAsync_WhenChildContentIsWhitespace_RendersOnlyTitle()
        {
            // Arrange
            PageHeaderTagHelper tagHelper = new()
            {
                Title = ""
            };

            TagHelperContext context = new(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test-id");

            TagHelperOutput output = new(
                "page-header",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) =>
                {
                    DefaultTagHelperContent tagHelperContent = new();
                    tagHelperContent.SetHtmlContent("   \t\n  ");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            string content = output.Content.GetContent();
            Assert.Equal("<h1 class=\"m-0\"></h1>", content);
        }
    }
}
