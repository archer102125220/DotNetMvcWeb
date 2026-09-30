using DotNetMvcWeb.Domain.Wallets.Exceptions;

namespace DotNetMvcWeb.Domain.Wallets.Entities;

/// <summary>
/// 數位錢包聚合根 (Aggregate Root - Rich Domain Model)
/// 封裝錢包的核心業務狀態與行為，確保實體永遠處於合法有效狀態。
/// </summary>
public class Wallet
{
    public Guid Id { get; private set; }
    public string OwnerId { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    // 供 EF Core 或反序列化使用的無參數建構子
    protected Wallet()
    {
    }

    /// <summary>
    /// 建立新錢包實體
    /// </summary>
    /// <param name="ownerId">錢包擁有者識別碼</param>
    /// <param name="initialBalance">初始金額（可選，預設為 0）</param>
    public Wallet(string ownerId, decimal initialBalance = 0m)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ArgumentException("錢包擁有人識別碼不可為空或空白。", nameof(ownerId));
        }

        if (initialBalance < 0m)
        {
            throw new InvalidAmountException(initialBalance, "初始錢包餘額");
        }

        Id = Guid.NewGuid();
        OwnerId = ownerId.Trim();
        Balance = initialBalance;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    /// <summary>
    /// 執行儲值業務行為
    /// </summary>
    /// <param name="amount">儲值金額</param>
    public void Deposit(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new InvalidAmountException(amount, "儲值");
        }

        Balance += amount;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// 執行扣款業務行為
    /// </summary>
    /// <param name="amount">扣款金額</param>
    public void Withdraw(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new InvalidAmountException(amount, "扣款");
        }

        if (amount > Balance)
        {
            throw new InsufficientBalanceException(Balance, amount);
        }

        Balance -= amount;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
