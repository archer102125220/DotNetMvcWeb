using DotNetMvcWeb.Extensions;
using Xunit;

namespace DotNetMvcWeb.Tests.Extensions
{
    public class StringExtensionsTests
    {
        // 1. 正向測試 (Positive Tests)
        [Theory]
        [InlineData("Hello World", 5, "Hello...")]
        [InlineData("ASP.NET Core", 7, "ASP.NET...")]
        [InlineData("繁體中文測試字串", 4, "繁體中文...")]
        public void Truncate_WhenStringLengthExceedsMaxLength_ReturnsTruncatedStringWithEllipsis(string input, int maxLength, string expected)
        {
            // Act
            string result = input.Truncate(maxLength);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Hello", 10, "Hello")]
        [InlineData("MVC", 3, "MVC")]
        [InlineData("測試", 2, "測試")]
        public void Truncate_WhenStringLengthIsWithinOrEqualMaxLength_ReturnsOriginalString(string input, int maxLength, string expected)
        {
            // Act
            string result = input.Truncate(maxLength);

            // Assert
            Assert.Equal(expected, result);
        }

        // 2. 反向測試 (Negative Tests)
        [Fact]
        public void Truncate_WhenInputIsNull_ReturnsEmptyString()
        {
            // Arrange
            string? nullString = null;

            // Act
            string result = nullString.Truncate(5);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        // 3. 邊界測試 (Boundary Tests)
        [Fact]
        public void Truncate_WhenInputIsEmpty_ReturnsEmptyString()
        {
            // Arrange
            string emptyString = string.Empty;

            // Act
            string result = emptyString.Truncate(5);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Truncate_WhenMaxLengthIsZero_ReturnsEllipsis()
        {
            // Arrange
            string input = "TestString";

            // Act
            string result = input.Truncate(0);

            // Assert
            Assert.Equal("...", result);
        }

        [Fact]
        public void Truncate_WhenMaxLengthMatchesExactLength_ReturnsExactStringWithoutEllipsis()
        {
            // Arrange
            string input = "ExactLength";

            // Act
            string result = input.Truncate(input.Length);

            // Assert
            Assert.Equal("ExactLength", result);
        }
    }
}
