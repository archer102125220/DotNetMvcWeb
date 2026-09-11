using DotNetMvcWeb.Models.DTOs.OracleDemo;
using DotNetMvcWeb.Models.Validators;
using Xunit;

namespace DotNetMvcWeb.Tests.Validators
{
    /// <summary>
    /// Oracle 示範項目 DTO 驗證器單元測試（包含 Data Annotations 屬性與 IValidatableObject 複合驗證）
    /// </summary>
    public class OracleDemoItemValidatorTests
    {
        [Fact]
        public void OracleDemoItemCreateDto_ValidNormalItem_ShouldPassValidation()
        {
            // Arrange
            OracleDemoItemCreateDto dto = new()
            {
                Name = "一般例行性任務",
                Description = "這是一般的日常例行維護項目",
                CategoryId = 1
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void OracleDemoItemCreateDto_WithoutDescriptionForNormalItem_ShouldPassValidation()
        {
            // Arrange
            OracleDemoItemCreateDto dto = new()
            {
                Name = "一般任務（無描述）",
                Description = null,
                CategoryId = null
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
        public void OracleDemoItemCreateDto_InvalidName_ShouldFailValidation(string invalidName)
        {
            // Arrange
            OracleDemoItemCreateDto dto = new()
            {
                Name = invalidName
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(OracleDemoItemCreateDto.Name)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void OracleDemoItemCreateDto_InvalidCategoryId_ShouldFailValidation(int invalidCategoryId)
        {
            // Arrange
            OracleDemoItemCreateDto dto = new()
            {
                Name = "有效名稱",
                CategoryId = invalidCategoryId
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(OracleDemoItemCreateDto.CategoryId)));
        }

        #region IValidatableObject 複合商業規則測試

        [Theory]
        [InlineData("重要系統安全性更新")]
        [InlineData("加急工單處理")]
        [InlineData("Urgent: Server Down")]
        public void OracleDemoItemCreateDto_ImportantKeywordWithoutDescription_ShouldFailIValidatableObject(string urgentName)
        {
            // Arrange: 標題包含「重要/加急/Urgent」，但 Description 為空
            OracleDemoItemCreateDto dto = new()
            {
                Name = urgentName,
                Description = null
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert: 應觸發 IValidatableObject 複合規則驗證失敗
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(OracleDemoItemCreateDto.Description)));
            string? errorMessage = result.GetErrorMessageForProperty(nameof(OracleDemoItemCreateDto.Description));
            Assert.NotNull(errorMessage);
            Assert.Contains("請詳細說明原因", errorMessage);
        }

        [Theory]
        [InlineData("重要系統安全性更新", "已修補 CVE-2024 漏洞，需重啟測試。")]
        [InlineData("加急工單處理", "客戶資料庫連線逾時，緊急排查中。")]
        [InlineData("Urgent Hotfix", "Hotfix for payment gateway issue.")]
        public void OracleDemoItemCreateDto_ImportantKeywordWithDescription_ShouldPassIValidatableObject(string urgentName, string description)
        {
            // Arrange: 標題包含「重要/加急/Urgent」，且有明確填寫 Description
            OracleDemoItemCreateDto dto = new()
            {
                Name = urgentName,
                Description = description
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        #endregion

        #region Update DTO Tests

        [Fact]
        public void OracleDemoItemUpdateDto_ValidData_ShouldPassValidation()
        {
            // Arrange
            OracleDemoItemUpdateDto dto = new()
            {
                Id = 10,
                Name = "更新後的項目名稱",
                Description = "更新後的詳細描述",
                CategoryId = 2
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
        public void OracleDemoItemUpdateDto_InvalidId_ShouldFailValidation(int invalidId)
        {
            // Arrange
            OracleDemoItemUpdateDto dto = new()
            {
                Id = invalidId,
                Name = "有效名稱"
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(OracleDemoItemUpdateDto.Id)));
        }

        [Fact]
        public void OracleDemoItemUpdateDto_ImportantKeywordWithoutDescription_ShouldFailIValidatableObject()
        {
            // Arrange
            OracleDemoItemUpdateDto dto = new()
            {
                Id = 5,
                Name = "重要更新項目",
                Description = ""
            };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(OracleDemoItemUpdateDto.Description)));
        }

        #endregion
    }
}
