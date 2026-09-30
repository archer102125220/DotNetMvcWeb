using DotNetMvcWeb.Domain.Common;
using DotNetMvcWeb.Domain.Wallets.Entities;
using DotNetMvcWeb.Domain.Wallets.Repositories;
using DotNetMvcWeb.Models.ViewModels;
using DotNetMvcWeb.Services.Interfaces;

namespace DotNetMvcWeb.Services.Implements;

/// <summary>
/// 錢包應用服務實作 (Application Service Implementation)
/// 職責定位：只負責流程協調與事務邊界，不承擔核心業務規則的判斷（業務規則完全由 Wallet 聚合根自身捍衛）。
/// </summary>
public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepository)
    {
        ArgumentNullException.ThrowIfNull(walletRepository);
        _walletRepository = walletRepository;
    }

    /// <inheritdoc />
    public async Task<WalletViewModel> GetOrCreateWalletAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        Wallet? wallet = await _walletRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
        if (wallet is null)
        {
            wallet = new Wallet(ownerId, initialBalance: 1000m); // 學習示範用初始餘額 1000
            await _walletRepository.AddAsync(wallet, cancellationToken);
        }

        return MapToViewModel(wallet);
    }

    /// <inheritdoc />
    public async Task<WalletViewModel> DepositAsync(string ownerId, decimal amount, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        Wallet? wallet = await _walletRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
        if (wallet is null)
        {
            return new WalletViewModel
            {
                OwnerId = ownerId,
                IsSuccess = false,
                StatusMessage = $"找不到使用者 {ownerId} 的錢包帳戶。"
            };
        }

        try
        {
            // 呼叫領域方法：由 Wallet 實體自身驗證金額合法性
            wallet.Deposit(amount);
            await _walletRepository.UpdateAsync(wallet, cancellationToken);

            var viewModel = MapToViewModel(wallet);
            viewModel.IsSuccess = true;
            viewModel.StatusMessage = $"成功儲值 NT$ {amount:N0} 元！當前餘額為 NT$ {wallet.Balance:N0} 元。";
            return viewModel;
        }
        catch (BusinessRuleException ex)
        {
            var viewModel = MapToViewModel(wallet);
            viewModel.IsSuccess = false;
            viewModel.StatusMessage = ex.Message;
            return viewModel;
        }
    }

    /// <inheritdoc />
    public async Task<WalletViewModel> WithdrawAsync(string ownerId, decimal amount, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        Wallet? wallet = await _walletRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
        if (wallet is null)
        {
            return new WalletViewModel
            {
                OwnerId = ownerId,
                IsSuccess = false,
                StatusMessage = $"找不到使用者 {ownerId} 的錢包帳戶。"
            };
        }

        try
        {
            // 呼叫領域方法：由 Wallet 實體自身嚴格檢核餘額與金額
            wallet.Withdraw(amount);
            await _walletRepository.UpdateAsync(wallet, cancellationToken);

            var viewModel = MapToViewModel(wallet);
            viewModel.IsSuccess = true;
            viewModel.StatusMessage = $"成功扣款 NT$ {amount:N0} 元！當前餘額為 NT$ {wallet.Balance:N0} 元。";
            return viewModel;
        }
        catch (BusinessRuleException ex)
        {
            var viewModel = MapToViewModel(wallet);
            viewModel.IsSuccess = false;
            viewModel.StatusMessage = ex.Message;
            return viewModel;
        }
    }

    private static WalletViewModel MapToViewModel(Wallet wallet)
    {
        return new WalletViewModel
        {
            Id = wallet.Id,
            OwnerId = wallet.OwnerId,
            Balance = wallet.Balance,
            UpdatedAtUtc = wallet.UpdatedAtUtc,
            IsSuccess = true
        };
    }
}
