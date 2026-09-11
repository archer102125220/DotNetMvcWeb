using DotNetMvcWeb.Models.DTOs.OracleDemo;
using DotNetMvcWeb.Models.Validators;
using Xunit;

namespace DotNetMvcWeb.Tests.Validators
{
    /// <summary>
    /// Oracle 示範分類相關 DTO 驗證器單元測試
    /// </summary>
    public class OracleDemoCategoryValidatorTests
    {
        [Fact]
        public void OracleDemoCategoryCreateDto_ValidName_ShouldPassValidation()
        {
            // Arrange
            OracleDemoCategoryCreateDto dto = new()
            {
                Name = "資料庫架構類別"
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
        public void OracleDemoCategoryCreateDto_EmptyOrWhitespaceName_ShouldFailValidation(string invalidName)
        {
            // Arrange
            OracleDemoCategoryCreateDto dto = new()
            {
                Name = invalidName
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(OracleDemoCategoryCreateDto.Name)));
        }

        [Fact]
        public void OracleDemoCategoryCreateDto_NameExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            OracleDemoCategoryCreateDto dto = new()
            {
                Name = new string('C', 51) // 超過 50 字元限制
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(OracleDemoCategoryCreateDto.Name)));
        }

        [Fact]
        public void OracleDemoCategoryUpdateDto_ValidData_ShouldPassValidation()
        {
            // Arrange
            OracleDemoCategoryUpdateDto dto = new()
            {
                Id = 1,
                Name = "更新後的類別名稱"
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void OracleDemoCategoryUpdateDto_InvalidId_ShouldFailValidation(int invalidId)
        {
            // Arrange
            OracleDemoCategoryUpdateDto dto = new()
            {
                Id = invalidId,
                Name = "合法名稱"
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(OracleDemoCategoryUpdateDto.Id)));
        }
    }
}
