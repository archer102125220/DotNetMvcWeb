using DotNetMvcWeb.Domain.Common;

namespace DotNetMvcWeb.Domain.Wallets.Exceptions;

/// <summary>
/// 交易金額不合法例外（金額必須大於 0）
/// </summary>
public class InvalidAmountException : BusinessRuleException
{
    public decimal Amount { get; }

    public InvalidAmountException(decimal amount, string operation)
        : base($"{operation} 金額必須大於 0，傳入金額為 {amount:N2}。")
    {
        Amount = amount;
    }
}
