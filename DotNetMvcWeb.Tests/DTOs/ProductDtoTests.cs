using DotNetMvcWeb.Models;
using DotNetMvcWeb.Models.DTOs.Products;
using Xunit;

namespace DotNetMvcWeb.Tests.DTOs
{
    /// <summary>
    /// 產品相關 DTO (Data Transfer Objects) 屬性與轉換單元測試
    /// </summary>
    public class ProductDtoTests
    {
        [Fact]
        public void ProductCreateDto_Properties_GetAndSetCorrectly()
        {
            // Arrange & Act
            ProductCreateDto dto = new()
            {
                Name = "機械鍵盤",
                Price = 2990m,
                Description = "青軸 RGB 機械鍵盤"
            };

            // Assert
            Assert.Equal("機械鍵盤", dto.Name);
            Assert.Equal(2990m, dto.Price);
            Assert.Equal("青軸 RGB 機械鍵盤", dto.Description);
        }

        [Fact]
        public void ProductUpdateDto_Properties_GetAndSetCorrectly()
        {
            // Arrange & Act
            ProductUpdateDto dto = new()
            {
                Id = 10,
                Name = "無線滑鼠",
                Price = 1290m,
                Description = "人體工學靜音滑鼠"
            };

            // Assert
            Assert.Equal(10, dto.Id);
            Assert.Equal("無線滑鼠", dto.Name);
            Assert.Equal(1290m, dto.Price);
            Assert.Equal("人體工學靜音滑鼠", dto.Description);
        }

        [Fact]
        public void ProductResponseDto_FromEntity_MapsAllPropertiesCorrectly()
        {
            // Arrange
            Product entity = new()
            {
                Id = 42,
                Name = "27 吋 4K 螢幕",
                Price = 12999m,
                Description = "IPS 面板，支援 HDR 400"
            };

            // Act
            ProductResponseDto responseDto = ProductResponseDto.FromEntity(entity);

            // Assert
            Assert.Equal(42, responseDto.Id);
            Assert.Equal("27 吋 4K 螢幕", responseDto.Name);
            Assert.Equal(12999m, responseDto.Price);
            Assert.Equal("IPS 面板，支援 HDR 400", responseDto.Description);
            Assert.NotEmpty(responseDto.FormattedPrice);
        }

        [Fact]
        public void ProductResponseDto_FromEntity_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => ProductResponseDto.FromEntity(null!));
        }
    }
}
