using DotNetMvcWeb.Models.ViewModels;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotNetMvcWeb.Controllers;

/// <summary>
/// 數位錢包控制器 (Wallet Controller)
/// 負責呈現錢包畫面與處理 HTMX 局部交易請求。
/// </summary>
public class WalletController : Controller
{
    private readonly IWalletService _walletService;
    private const string DefaultOwnerId = "demo_user";

    public WalletController(IWalletService walletService)
    {
        ArgumentNullException.ThrowIfNull(walletService);
        _walletService = walletService;
    }

    /// <summary>
    /// 錢包主頁面
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _walletService.GetOrCreateWalletAsync(DefaultOwnerId, cancellationToken);
        return View(model);
    }

    /// <summary>
    /// 儲值操作 (支援 HTMX 局部刷新)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit([FromForm] decimal amount, CancellationToken cancellationToken)
    {
        var model = await _walletService.DepositAsync(DefaultOwnerId, amount, cancellationToken);

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return PartialView("_WalletCard", model);
        }

        return View("Index", model);
    }

    /// <summary>
    /// 扣款操作 (支援 HTMX 局部刷新)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw([FromForm] decimal amount, CancellationToken cancellationToken)
    {
        var model = await _walletService.WithdrawAsync(DefaultOwnerId, amount, cancellationToken);

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return PartialView("_WalletCard", model);
        }

        return View("Index", model);
    }
}
