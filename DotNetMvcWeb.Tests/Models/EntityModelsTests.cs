using DotNetMvcWeb.Models;
using Xunit;

namespace DotNetMvcWeb.Tests.Models
{
    public class EntityModelsTests
    {
        [Fact]
        public void Product_Properties_GetAndSetCorrectly()
        {
            // Arrange
            Product product = new()
            {
                Id = 10,
                Name = "測試商品",
                Price = 999.5m,
                Description = "測試說明"
            };

            // Assert
            Assert.Equal(10, product.Id);
            Assert.Equal("測試商品", product.Name);
            Assert.Equal(999.5m, product.Price);
            Assert.Equal("測試說明", product.Description);
        }

        [Fact]
        public void MssqlDemoEntities_Properties_GetAndSetCorrectly()
        {
            // Arrange
            DateTime now = DateTime.UtcNow;
            MssqlDemoCategory category = new()
            {
                Id = 1,
                Name = "類別A",
                CreatedAt = now,
                Items = new List<MssqlDemoItem>()
            };

            MssqlDemoItem item = new()
            {
                Id = 100,
                Name = "項目A",
                Description = "說明A",
                CreatedAt = now,
                CategoryId = 1,
                Category = category
            };

            category.Items.Add(item);

            // Assert
            Assert.Equal(1, category.Id);
            Assert.Equal("類別A", category.Name);
            Assert.Equal(now, category.CreatedAt);
            Assert.Single(category.Items);

            Assert.Equal(100, item.Id);
            Assert.Equal("項目A", item.Name);
            Assert.Equal("說明A", item.Description);
            Assert.Equal(now, item.CreatedAt);
            Assert.Equal(1, item.CategoryId);
            Assert.Same(category, item.Category);
        }

        [Fact]
        public void MysqlDemoEntities_Properties_GetAndSetCorrectly()
        {
            // Arrange
            DateTime now = DateTime.UtcNow;
            MysqlDemoCategory category = new()
            {
                Id = 2,
                Name = "類別B",
                CreatedAt = now,
                Items = new List<MysqlDemoItem>()
            };

            MysqlDemoItem item = new()
            {
                Id = 200,
                Name = "項目B",
                Description = "說明B",
                CreatedAt = now,
                CategoryId = 2,
                Category = category
            };

            // Assert
            Assert.Equal(2, category.Id);
            Assert.Equal("類別B", category.Name);
            Assert.Equal(200, item.Id);
            Assert.Equal("項目B", item.Name);
            Assert.Equal(2, item.CategoryId);
            Assert.Same(category, item.Category);
        }

        [Fact]
        public void PostgresDemoEntities_Properties_GetAndSetCorrectly()
        {
            // Arrange
            DateTime now = DateTime.UtcNow;
            PostgresDemoCategory category = new()
            {
                Id = 3,
                Name = "類別C",
                CreatedAt = now,
                Items = new List<PostgresDemoItem>()
            };

            PostgresDemoItem item = new()
            {
                Id = 300,
                Name = "項目C",
                Description = "說明C",
                CreatedAt = now,
                CategoryId = 3,
                Category = category
            };

            // Assert
            Assert.Equal(3, category.Id);
            Assert.Equal("類別C", category.Name);
            Assert.Equal(300, item.Id);
            Assert.Equal("項目C", item.Name);
            Assert.Equal(3, item.CategoryId);
            Assert.Same(category, item.Category);
        }

        [Fact]
        public void OracleDemoEntities_Properties_GetAndSetCorrectly()
        {
            // Arrange
            DateTime now = DateTime.UtcNow;
            OracleDemoCategory category = new()
            {
                Id = 4,
                Name = "類別D",
                CreatedAt = now,
                Items = new List<OracleDemoItem>()
            };

            OracleDemoItem item = new()
            {
                Id = 400,
                Name = "項目D",
                Description = "說明D",
                CreatedAt = now,
                CategoryId = 4,
                Category = category
            };

            // Assert
            Assert.Equal(4, category.Id);
            Assert.Equal("類別D", category.Name);
            Assert.Equal(400, item.Id);
            Assert.Equal("項目D", item.Name);
            Assert.Equal(4, item.CategoryId);
            Assert.Same(category, item.Category);
        }
    }
}
