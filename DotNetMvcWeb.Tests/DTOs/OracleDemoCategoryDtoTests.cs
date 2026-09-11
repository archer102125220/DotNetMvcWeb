using DotNetMvcWeb.Models;
using DotNetMvcWeb.Models.DTOs.OracleDemo;
using Xunit;

namespace DotNetMvcWeb.Tests.DTOs
{
    /// <summary>
    /// Oracle 示範分類相關 DTO (Data Transfer Objects) 屬性與轉換單元測試
    /// </summary>
    public class OracleDemoCategoryDtoTests
    {
        [Fact]
        public void OracleDemoCategoryCreateDto_Properties_GetAndSetCorrectly()
        {
            // Arrange & Act
            OracleDemoCategoryCreateDto dto = new()
            {
                Name = "雲端服務"
            };

            // Assert
            Assert.Equal("雲端服務", dto.Name);
        }

        [Fact]
        public void OracleDemoCategoryUpdateDto_Properties_GetAndSetCorrectly()
        {
            // Arrange & Act
            OracleDemoCategoryUpdateDto dto = new()
            {
                Id = 7,
                Name = "基礎設施"
            };

            // Assert
            Assert.Equal(7, dto.Id);
            Assert.Equal("基礎設施", dto.Name);
        }

        [Fact]
        public void OracleDemoCategoryResponseDto_FromEntity_WithItems_MapsCorrectly()
        {
            // Arrange
            DateTime now = DateTime.UtcNow;
            OracleDemoCategory category = new()
            {
                Id = 10,
                Name = "開發工具",
                CreatedAt = now,
                Items = new List<OracleDemoItem>
                {
                    new() { Id = 101, Name = "Visual Studio", CategoryId = 10 },
                    new() { Id = 102, Name = "VS Code", CategoryId = 10 }
                }
            };

            // Act
            OracleDemoCategoryResponseDto responseDto = OracleDemoCategoryResponseDto.FromEntity(category);

            // Assert
            Assert.Equal(10, responseDto.Id);
            Assert.Equal("開發工具", responseDto.Name);
            Assert.Equal(now, responseDto.CreatedAt);
            Assert.Equal(2, responseDto.ItemCount);
        }

        [Fact]
        public void OracleDemoCategoryResponseDto_FromEntity_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => OracleDemoCategoryResponseDto.FromEntity(null!));
        }
    }
}
