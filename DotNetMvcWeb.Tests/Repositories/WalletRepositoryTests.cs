using DotNetMvcWeb.Data;
using DotNetMvcWeb.Domain.Wallets.Entities;
using DotNetMvcWeb.Repositories.Implements;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNetMvcWeb.Tests.Repositories;

/// <summary>
/// 錢包倉儲單元測試 (WalletRepository Tests)
/// 驗證 EF Core 與 Wallet 領域聚合根的資料持久化與查詢行為。
/// </summary>
public class WalletRepositoryTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"{dbName}_{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void Constructor_WhenContextNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WalletRepository(null!));
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_ShouldPersistAndRetrieveWallet()
    {
        // Arrange
        using AppDbContext context = CreateDbContext(nameof(AddAsync_And_GetByIdAsync_ShouldPersistAndRetrieveWallet));
        var repository = new WalletRepository(context);
        var wallet = new Wallet("owner_123", 500m);

        // Act
        await repository.AddAsync(wallet);
        var retrieved = await repository.GetByIdAsync(wallet.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(wallet.Id, retrieved.Id);
        Assert.Equal("owner_123", retrieved.OwnerId);
        Assert.Equal(500m, retrieved.Balance);
    }

    [Fact]
    public async Task GetByOwnerIdAsync_WhenOwnerExists_ShouldReturnWallet()
    {
        // Arrange
        using AppDbContext context = CreateDbContext(nameof(GetByOwnerIdAsync_WhenOwnerExists_ShouldReturnWallet));
        var repository = new WalletRepository(context);
        var wallet = new Wallet("vip_user_01", 1000m);
        await repository.AddAsync(wallet);

        // Act
        var retrieved = await repository.GetByOwnerIdAsync("vip_user_01");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(wallet.Id, retrieved.Id);
        Assert.Equal("vip_user_01", retrieved.OwnerId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByOwnerIdAsync_WhenOwnerIsNullOrWhiteSpace_ShouldReturnNull(string invalidOwner)
    {
        // Arrange
        using AppDbContext context = CreateDbContext(nameof(GetByOwnerIdAsync_WhenOwnerIsNullOrWhiteSpace_ShouldReturnNull));
        var repository = new WalletRepository(context);

        // Act
        var retrieved = await repository.GetByOwnerIdAsync(invalidOwner);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistBalanceChange()
    {
        // Arrange
        using AppDbContext context = CreateDbContext(nameof(UpdateAsync_ShouldPersistBalanceChange));
        var repository = new WalletRepository(context);
        var wallet = new Wallet("owner_update", 300m);
        await repository.AddAsync(wallet);

        // Act: 透過 Domain 業務方法扣款，再由 Repository 持久化
        wallet.Withdraw(100m);
        await repository.UpdateAsync(wallet);

        // Assert
        var updated = await repository.GetByIdAsync(wallet.Id);
        Assert.NotNull(updated);
        Assert.Equal(200m, updated.Balance);
    }

    [Fact]
    public async Task AddAsync_WhenWalletNull_ThrowsArgumentNullException()
    {
        using AppDbContext context = CreateDbContext(nameof(AddAsync_WhenWalletNull_ThrowsArgumentNullException));
        var repository = new WalletRepository(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WhenWalletNull_ThrowsArgumentNullException()
    {
        using AppDbContext context = CreateDbContext(nameof(UpdateAsync_WhenWalletNull_ThrowsArgumentNullException));
        var repository = new WalletRepository(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null!));
    }
}
