using DotNetMvcWeb.Models;
using DotNetMvcWeb.Models.DTOs.OracleDemo;
using Xunit;

namespace DotNetMvcWeb.Tests.DTOs
{
    /// <summary>
    /// Oracle 示範項目相關 DTO (Data Transfer Objects) 屬性與轉換單元測試
    /// </summary>
    public class OracleDemoItemDtoTests
    {
        [Fact]
        public void OracleDemoItemCreateDto_Properties_GetAndSetCorrectly()
        {
            // Arrange & Act
            OracleDemoItemCreateDto dto = new()
            {
                Name = "資料庫備份任務",
                Description = "每日凌晨 2 點自動進行全量備份",
                CategoryId = 3
            };

            // Assert
            Assert.Equal("資料庫備份任務", dto.Name);
            Assert.Equal("每日凌晨 2 點自動進行全量備份", dto.Description);
            Assert.Equal(3, dto.CategoryId);
        }

        [Fact]
        public void OracleDemoItemUpdateDto_Properties_GetAndSetCorrectly()
        {
            // Arrange & Act
            OracleDemoItemUpdateDto dto = new()
            {
                Id = 99,
                Name = "更新後的任務名稱",
                Description = "更新後的詳細內容",
                CategoryId = 5
            };

            // Assert
            Assert.Equal(99, dto.Id);
            Assert.Equal("更新後的任務名稱", dto.Name);
            Assert.Equal("更新後的詳細內容", dto.Description);
            Assert.Equal(5, dto.CategoryId);
        }

        [Fact]
        public void OracleDemoItemResponseDto_FromEntity_WithCategory_FlattensCategoryNameCorrectly()
        {
            // Arrange
            DateTime now = DateTime.UtcNow;
            OracleDemoCategory category = new()
            {
                Id = 1,
                Name = "後端系統"
            };

            OracleDemoItem item = new()
            {
                Id = 200,
                Name = "API 效能最佳化",
                Description = "改善查詢索引與快取",
                CreatedAt = now,
                CategoryId = 1,
                Category = category
            };

            // Act
            OracleDemoItemResponseDto responseDto = OracleDemoItemResponseDto.FromEntity(item);

            // Assert
            Assert.Equal(200, responseDto.Id);
            Assert.Equal("API 效能最佳化", responseDto.Name);
            Assert.Equal("改善查詢索引與快取", responseDto.Description);
            Assert.Equal(now, responseDto.CreatedAt);
            Assert.Equal(1, responseDto.CategoryId);
            Assert.Equal("後端系統", responseDto.CategoryName); // 驗證關聯扁平化
        }

        [Fact]
        public void OracleDemoItemResponseDto_FromEntity_WithoutCategory_CategoryNameIsNull()
        {
            // Arrange
            DateTime now = DateTime.UtcNow;
            OracleDemoItem item = new()
            {
                Id = 201,
                Name = "獨立無分類項目",
                Description = "無分類",
                CreatedAt = now,
                CategoryId = null,
                Category = null
            };

            // Act
            OracleDemoItemResponseDto responseDto = OracleDemoItemResponseDto.FromEntity(item);

            // Assert
            Assert.Equal(201, responseDto.Id);
            Assert.Null(responseDto.CategoryId);
            Assert.Null(responseDto.CategoryName);
        }

        [Fact]
        public void OracleDemoItemResponseDto_FromEntity_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => OracleDemoItemResponseDto.FromEntity(null!));
        }
    }
}
