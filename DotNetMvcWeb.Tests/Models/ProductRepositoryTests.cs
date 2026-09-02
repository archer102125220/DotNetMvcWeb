using DotNetMvcWeb.Models;
using Xunit;

namespace DotNetMvcWeb.Tests.Models
{
    public class ProductRepositoryTests
    {
        // 1. 正向測試 (Positive Tests)
        [Fact]
        public async Task GetAllAsync_ReturnsInitialSeedProducts()
        {
            // Arrange
            ProductRepository repository = new();

            // Act
            IEnumerable<Product> products = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(products);
            Assert.Equal(2, products.Count());
            Assert.Contains(products, p => p.Name == "筆記型電腦");
            Assert.Contains(products, p => p.Name == "無線滑鼠");
        }

        [Fact]
        public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
        {
            // Arrange
            ProductRepository repository = new();

            // Act
            Product? product = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(product);
            Assert.Equal(1, product.Id);
            Assert.Equal("筆記型電腦", product.Name);
            Assert.Equal(35000, product.Price);
        }

        [Fact]
        public async Task AddAsync_WhenProductIsValid_AssignsIncrementedIdAndAdds()
        {
            // Arrange
            ProductRepository repository = new();
            Product newProduct = new()
            {
                Name = "機械鍵盤",
                Price = 2800,
                Description = "RGB 背光機械鍵盤"
            };

            // Act
            await repository.AddAsync(newProduct);
            Product? retrieved = await repository.GetByIdAsync(newProduct.Id);

            // Assert
            Assert.Equal(3, newProduct.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("機械鍵盤", retrieved.Name);
            IEnumerable<Product> all = await repository.GetAllAsync();
            Assert.Equal(3, all.Count());
        }

        [Fact]
        public async Task UpdateAsync_WhenProductExists_UpdatesPropertiesSuccessfully()
        {
            // Arrange
            ProductRepository repository = new();
            Product updatedProduct = new()
            {
                Id = 1,
                Name = "頂級筆記型電腦",
                Price = 45000,
                Description = "升級版規格"
            };

            // Act
            await repository.UpdateAsync(updatedProduct);
            Product? retrieved = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("頂級筆記型電腦", retrieved.Name);
            Assert.Equal(45000, retrieved.Price);
            Assert.Equal("升級版規格", retrieved.Description);
        }

        [Fact]
        public async Task DeleteAsync_WhenProductExists_RemovesProduct()
        {
            // Arrange
            ProductRepository repository = new();

            // Act
            await repository.DeleteAsync(1);
            Product? retrieved = await repository.GetByIdAsync(1);
            IEnumerable<Product> all = await repository.GetAllAsync();

            // Assert
            Assert.Null(retrieved);
            Assert.Single(all);
        }

        // 2. 反向測試 (Negative Tests)
        [Fact]
        public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
        {
            // Arrange
            ProductRepository repository = new();

            // Act
            Product? product = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(product);
        }

        [Fact]
        public async Task AddAsync_WhenProductIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            ProductRepository repository = new();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_WhenProductIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            ProductRepository repository = new();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null!));
        }

        // 3. 邊界測試 (Boundary Tests)
        [Fact]
        public async Task UpdateAsync_WhenProductDoesNotExist_DoesNothing()
        {
            // Arrange
            ProductRepository repository = new();
            Product nonExistentProduct = new()
            {
                Id = 9999,
                Name = "不存在商品",
                Price = 100
            };

            // Act
            await repository.UpdateAsync(nonExistentProduct);
            IEnumerable<Product> all = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, all.Count());
            Assert.Null(await repository.GetByIdAsync(9999));
        }

        [Fact]
        public async Task DeleteAsync_WhenProductDoesNotExist_DoesNotThrowOrAffectOthers()
        {
            // Arrange
            ProductRepository repository = new();

            // Act
            await repository.DeleteAsync(8888);
            IEnumerable<Product> all = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, all.Count());
        }

        [Fact]
        public async Task AddAsync_WhenAllProductsDeleted_AssignsIdOne()
        {
            // Arrange
            ProductRepository repository = new();
            await repository.DeleteAsync(1);
            await repository.DeleteAsync(2);

            Product newFirst = new() { Name = "全新首筆商品", Price = 500 };

            // Act
            await repository.AddAsync(newFirst);

            // Assert
            Assert.Equal(1, newFirst.Id);
            Product? retrieved = await repository.GetByIdAsync(1);
            Assert.NotNull(retrieved);
        }
    }
}
