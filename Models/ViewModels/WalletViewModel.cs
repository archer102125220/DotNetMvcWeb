namespace DotNetMvcWeb.Models.ViewModels;

/// <summary>
/// 錢包展示視圖模型 (Wallet View Model)
/// 專門給 Razor View 渲染使用，與 Domain Entity 解耦。
/// </summary>
public class WalletViewModel
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// 操作結果訊息（成功或錯誤提示）
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool IsSuccess { get; set; } = true;
}

/// <summary>
/// 錢包儲值/扣款請求模型
/// </summary>
public class WalletTransactionRequest
{
    public string OwnerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
