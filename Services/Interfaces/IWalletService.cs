using DotNetMvcWeb.Models.ViewModels;

namespace DotNetMvcWeb.Services.Interfaces;

/// <summary>
/// 錢包應用服務介面 (Application Service Interface)
/// 負責協調使用者用例 (Use Cases)、交易流程調度與 DTO/ViewModel 轉換。
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// 取得或初始化使用者的錢包
    /// </summary>
    Task<WalletViewModel> GetOrCreateWalletAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 執行儲值用例
    /// </summary>
    Task<WalletViewModel> DepositAsync(string ownerId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// 執行扣款用例
    /// </summary>
    Task<WalletViewModel> WithdrawAsync(string ownerId, decimal amount, CancellationToken cancellationToken = default);
}
