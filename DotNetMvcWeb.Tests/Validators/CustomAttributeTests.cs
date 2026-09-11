using System.ComponentModel.DataAnnotations;
using DotNetMvcWeb.Models.Validators;
using DotNetMvcWeb.Models.Validators.CustomAttributes;
using Xunit;

namespace DotNetMvcWeb.Tests.Validators
{
    /// <summary>
    /// 自訂驗證標籤 (Custom Validation Attributes) 與輔助工具單元測試
    /// </summary>
    public class CustomAttributeTests
    {
        private class TestPriceModel
        {
            [PriceRange(10.0, 1000.0)]
            public decimal Price { get; set; }
        }

        private class TestStringModel
        {
            [NonEmptyString]
            public string? Name { get; set; }
        }

        [Theory]
        [InlineData(10.0)]
        [InlineData(500.5)]
        [InlineData(1000.0)]
        public void PriceRangeAttribute_ValidPrice_ShouldPassValidation(double price)
        {
            // Arrange
            TestPriceModel model = new() { Price = (decimal)price };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(model);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(9.99)]
        [InlineData(-100.0)]
        [InlineData(1000.01)]
        public void PriceRangeAttribute_InvalidPrice_ShouldFailValidation(double price)
        {
            // Arrange
            TestPriceModel model = new() { Price = (decimal)price };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(model);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(TestPriceModel.Price)));
            string? errorMessage = result.GetErrorMessageForProperty(nameof(TestPriceModel.Price));
            Assert.NotNull(errorMessage);
            Assert.Contains("價格必須介於", errorMessage);
        }

        [Fact]
        public void PriceRangeAttribute_NullValue_ShouldSucceed()
        {
            // Arrange
            PriceRangeAttribute attribute = new(10.0, 100.0);
            ValidationContext context = new(new object()) { MemberName = "TestProp" };

            // Act
            ValidationResult? result = attribute.GetValidationResult(null, context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData("ValidName")]
        [InlineData(" A ")]
        [InlineData("123")]
        public void NonEmptyStringAttribute_ValidString_ShouldPassValidation(string text)
        {
            // Arrange
            TestStringModel model = new() { Name = text };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(model);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public void NonEmptyStringAttribute_WhitespaceOrEmpty_ShouldFailValidation(string text)
        {
            // Arrange
            TestStringModel model = new() { Name = text };

            // Act
            ModelValidationResult result = ModelValidatorHelper.Validate(model);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrorFor(nameof(TestStringModel.Name)));
            string? errorMessage = result.GetErrorMessageForProperty(nameof(TestStringModel.Name));
            Assert.NotNull(errorMessage);
            Assert.Contains("不得為空或僅包含空白字元", errorMessage);
        }

        [Fact]
        public void NonEmptyStringAttribute_NullValue_ShouldSucceed()
        {
            // Arrange
            NonEmptyStringAttribute attribute = new();
            ValidationContext context = new(new object()) { MemberName = "TestProp" };

            // Act
            ValidationResult? result = attribute.GetValidationResult(null, context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void ModelValidatorHelper_ValidateProperty_ShouldReturnErrorsForSpecificProperty()
        {
            // Arrange
            TestPriceModel model = new() { Price = 0m };

            // Act
            IList<ValidationResult> errors = ModelValidatorHelper.ValidateProperty(model, nameof(TestPriceModel.Price), 0m);

            // Assert
            Assert.NotEmpty(errors);
        }
    }
}
