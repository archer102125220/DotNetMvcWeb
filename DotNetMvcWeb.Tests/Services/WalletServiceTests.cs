using DotNetMvcWeb.Domain.Wallets.Entities;
using DotNetMvcWeb.Domain.Wallets.Repositories;
using DotNetMvcWeb.Services.Implements;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Services;

/// <summary>
/// 錢包應用服務單元測試 (WalletService Tests)
/// 驗證 Service 層協調工作流（取得實體 -> 呼叫領域行為 -> 儲存實體 -> 捕捉業務異常轉換為 ViewModel）的邏輯。
/// </summary>
public class WalletServiceTests
{
    private readonly Mock<IWalletRepository> _repositoryMock;
    private readonly WalletService _service;

    public WalletServiceTests()
    {
        _repositoryMock = new Mock<IWalletRepository>();
        _service = new WalletService(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_WhenRepositoryNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WalletService(null!));
    }

    [Fact]
    public async Task GetOrCreateWalletAsync_WhenWalletDoesNotExist_ShouldCreateAndReturnNewWallet()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync("new_user", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var result = await _service.GetOrCreateWalletAsync("new_user");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new_user", result.OwnerId);
        Assert.Equal(1000m, result.Balance);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DepositAsync_WhenSuccessful_ShouldReturnUpdatedBalanceAndSuccessMessage()
    {
        // Arrange
        var wallet = new Wallet("user_deposit", 500m);
        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync("user_deposit", It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _service.DepositAsync("user_deposit", 200m);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(700m, result.Balance);
        Assert.Contains("成功儲值", result.StatusMessage);
        _repositoryMock.Verify(r => r.UpdateAsync(wallet, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_WhenBalanceInsufficient_ShouldCatchExceptionAndReturnErrorMessage()
    {
        // Arrange
        var wallet = new Wallet("user_withdraw", 100m);
        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync("user_withdraw", It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _service.WithdrawAsync("user_withdraw", 500m);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(100m, result.Balance); // 餘額未變
        Assert.Contains("可用餘額不足", result.StatusMessage);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_WhenAmountNegative_ShouldCatchExceptionAndReturnErrorMessage()
    {
        // Arrange
        var wallet = new Wallet("user_withdraw", 100m);
        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync("user_withdraw", It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _service.WithdrawAsync("user_withdraw", -50m);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("必須大於 0", result.StatusMessage);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
