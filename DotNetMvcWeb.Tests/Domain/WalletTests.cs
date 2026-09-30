using DotNetMvcWeb.Domain.Wallets.Entities;
using DotNetMvcWeb.Domain.Wallets.Exceptions;
using Xunit;

namespace DotNetMvcWeb.Tests.Domain;

/// <summary>
/// 錢包純領域單元測試 (Pure Domain Unit Tests)
/// 體現 Rich Domain Model 的優勢：不需要任何 Mocking 或資料庫，即可針對業務規則進行 100% 獨立驗證。
/// </summary>
public class WalletTests
{
    [Fact]
    public void Constructor_WithValidOwnerId_ShouldInitializeSuccessfully()
    {
        // Arrange
        string ownerId = "user_001";
        decimal initialBalance = 100m;

        // Act
        var wallet = new Wallet(ownerId, initialBalance);

        // Assert
        Assert.NotEqual(Guid.Empty, wallet.Id);
        Assert.Equal("user_001", wallet.OwnerId);
        Assert.Equal(100m, wallet.Balance);
        Assert.True(wallet.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Equal(wallet.CreatedAtUtc, wallet.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOwnerId_ShouldThrowArgumentException(string invalidOwnerId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Wallet(invalidOwnerId));
    }

    [Fact]
    public void Constructor_WithNegativeBalance_ShouldThrowInvalidAmountException()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidAmountException>(() => new Wallet("user_001", -50m));
        Assert.Equal(-50m, ex.Amount);
    }

    [Fact]
    public void Deposit_WithPositiveAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var wallet = new Wallet("user_001", 100m);
        decimal depositAmount = 250m;

        // Act
        wallet.Deposit(depositAmount);

        // Assert
        Assert.Equal(350m, wallet.Balance);
        Assert.True(wallet.UpdatedAtUtc >= wallet.CreatedAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Deposit_WithZeroOrNegativeAmount_ShouldThrowInvalidAmountException(decimal invalidAmount)
    {
        // Arrange
        var wallet = new Wallet("user_001", 100m);

        // Act & Assert
        var ex = Assert.Throws<InvalidAmountException>(() => wallet.Deposit(invalidAmount));
        Assert.Equal(invalidAmount, ex.Amount);
    }

    [Fact]
    public void Withdraw_WithSufficientBalance_ShouldDecreaseBalance()
    {
        // Arrange
        var wallet = new Wallet("user_001", 300m);
        decimal withdrawAmount = 120m;

        // Act
        wallet.Withdraw(withdrawAmount);

        // Assert
        Assert.Equal(180m, wallet.Balance);
        Assert.True(wallet.UpdatedAtUtc >= wallet.CreatedAtUtc);
    }

    [Fact]
    public void Withdraw_WithAmountExceedingBalance_ShouldThrowInsufficientBalanceException_AndNotChangeBalance()
    {
        // Arrange
        var wallet = new Wallet("user_001", 100m);
        decimal excessiveAmount = 150m;

        // Act & Assert
        var ex = Assert.Throws<InsufficientBalanceException>(() => wallet.Withdraw(excessiveAmount));
        Assert.Equal(100m, ex.CurrentBalance);
        Assert.Equal(150m, ex.AttemptedAmount);
        Assert.Equal(100m, wallet.Balance); // 確保不變性：失敗時餘額不被篡改
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Withdraw_WithZeroOrNegativeAmount_ShouldThrowInvalidAmountException(decimal invalidAmount)
    {
        // Arrange
        var wallet = new Wallet("user_001", 100m);

        // Act & Assert
        var ex = Assert.Throws<InvalidAmountException>(() => wallet.Withdraw(invalidAmount));
        Assert.Equal(invalidAmount, ex.Amount);
    }
}
