using DotNetMvcWeb.Domain.Common;

namespace DotNetMvcWeb.Domain.Wallets.Exceptions;

/// <summary>
/// 錢包餘額不足例外
/// </summary>
public class InsufficientBalanceException : BusinessRuleException
{
    public decimal CurrentBalance { get; }
    public decimal AttemptedAmount { get; }

    public InsufficientBalanceException(decimal currentBalance, decimal attemptedAmount)
        : base($"錢包可用餘額不足。當前餘額為 {currentBalance:N2}，嘗試扣款金額為 {attemptedAmount:N2}。")
    {
        CurrentBalance = currentBalance;
        AttemptedAmount = attemptedAmount;
    }
}
