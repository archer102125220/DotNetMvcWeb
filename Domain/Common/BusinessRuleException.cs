namespace DotNetMvcWeb.Domain.Common;

/// <summary>
/// 領域業務規則例外基底類別
/// </summary>
public abstract class BusinessRuleException : Exception
{
    protected BusinessRuleException(string message) : base(message)
    {
    }

    protected BusinessRuleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
