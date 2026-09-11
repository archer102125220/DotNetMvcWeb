using DotNetMvcWeb.Models.DTOs.Products;
using DotNetMvcWeb.Models.Validators;
using Xunit;

namespace DotNetMvcWeb.Tests.Validators
{
    /// <summary>
    /// 產品相關 DTO 的資料驗證規則 (Validators) 單元測試
    /// </summary>
    public class ProductValidatorTests
    {
        [Fact]
        public void ProductCreateDto_ValidData_ShouldPassValidation()
        {
            // Arrange
            ProductCreateDto dto = new()
            {
                Name = "高效能筆記型電腦",
                Price = 39999.0m,
                Description = "最新第 14 代處理器，32GB 記憶體。"
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ProductCreateDto_InvalidName_ShouldFailValidation(string invalidName)
        {
            // Arrange
            ProductCreateDto dto = new()
            {
                Name = invalidName,
                Price = 100m,
                Description = "合法描述"
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(ProductCreateDto.Name)));
        }

        [Fact]
        public void ProductCreateDto_NameExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            ProductCreateDto dto = new()
            {
                Name = new string('A', 101), // 超過 100 字元上限
                Price = 100m
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(ProductCreateDto.Name)));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-10.0)]
        [InlineData(1_000_000.01)]
        public void ProductCreateDto_InvalidPrice_ShouldFailValidation(double invalidPrice)
        {
            // Arrange
            ProductCreateDto dto = new()
            {
                Name = "測試商品",
                Price = (decimal)invalidPrice
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(ProductCreateDto.Price)));
            string? errorMessage = result.GetErrorMessageForProperty(nameof(ProductCreateDto.Price));
            Assert.NotNull(errorMessage);
            Assert.Contains("價格", errorMessage);
        }

        [Fact]
        public void ProductCreateDto_DescriptionExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            ProductCreateDto dto = new()
            {
                Name = "測試商品",
                Price = 100m,
                Description = new string('B', 501) // 超過 500 字元上限
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(ProductCreateDto.Description)));
        }

        [Fact]
        public void ProductUpdateDto_ValidData_ShouldPassValidation()
        {
            // Arrange
            ProductUpdateDto dto = new()
            {
                Id = 1,
                Name = "更新後的商品名稱",
                Price = 1999m,
                Description = "更新後的描述"
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ProductUpdateDto_InvalidId_ShouldFailValidation(int invalidId)
        {
            // Arrange
            ProductUpdateDto dto = new()
            {
                Id = invalidId,
                Name = "商品名稱",
                Price = 100m
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(ProductUpdateDto.Id)));
        }
    }
}
