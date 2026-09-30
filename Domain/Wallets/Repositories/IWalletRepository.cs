using DotNetMvcWeb.Domain.Wallets.Entities;

namespace DotNetMvcWeb.Domain.Wallets.Repositories;

/// <summary>
/// 錢包領域倉儲介面 (Domain Repository Interface)
/// 介面定義在 Domain 層，由外層 Infrastructure/Repositories 負責實作 (依賴反轉 DIP)。
/// </summary>
public interface IWalletRepository
{
    /// <summary>
    /// 根據錢包 ID 取得錢包實體
    /// </summary>
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據擁有者識別碼取得錢包實體
    /// </summary>
    Task<Wallet?> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增錢包
    /// </summary>
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新錢包狀態
    /// </summary>
    Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default);
}
