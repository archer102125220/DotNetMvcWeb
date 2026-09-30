using DotNetMvcWeb.Data;
using DotNetMvcWeb.Domain.Wallets.Entities;
using DotNetMvcWeb.Domain.Wallets.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Repositories.Implements;

/// <summary>
/// 數位錢包倉儲實作 (Wallet Repository Implementation)
/// 實作位於 Domain 層定義的 IWalletRepository 介面 (依賴反轉原則 DIP)，
/// 負責透過 EF Core 將 Wallet 聚合根持久化至資料庫。
/// </summary>
public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;

    public WalletRepository(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Wallet?> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return null;
        }

        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.OwnerId == ownerId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(wallet);

        await _context.Wallets.AddAsync(wallet, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(wallet);

        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
