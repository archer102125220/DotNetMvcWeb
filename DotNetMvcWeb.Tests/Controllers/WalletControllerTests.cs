using DotNetMvcWeb.Controllers;
using DotNetMvcWeb.Models.ViewModels;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DotNetMvcWeb.Tests.Controllers;

/// <summary>
/// 錢包控制器單元測試 (WalletController Tests)
/// 驗證 HTTP 請求處理、HTMX 標頭識別與局部視圖回傳機制。
/// </summary>
public class WalletControllerTests
{
    private readonly Mock<IWalletService> _serviceMock;
    private readonly WalletController _controller;

    public WalletControllerTests()
    {
        _serviceMock = new Mock<IWalletService>();
        _controller = new WalletController(_serviceMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithWalletViewModel()
    {
        // Arrange
        var fakeModel = new WalletViewModel { OwnerId = "demo_user", Balance = 1000m };
        _serviceMock
            .Setup(s => s.GetOrCreateWalletAsync("demo_user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeModel);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<WalletViewModel>(viewResult.Model);
        Assert.Equal(1000m, model.Balance);
    }

    [Fact]
    public async Task Deposit_WhenHtmxRequest_ReturnsPartialView()
    {
        // Arrange
        var fakeModel = new WalletViewModel { OwnerId = "demo_user", Balance = 1500m };
        _serviceMock
            .Setup(s => s.DepositAsync("demo_user", 500m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeModel);

        // 模擬 HTMX Request Header
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["HX-Request"] = "true";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _controller.Deposit(500m, CancellationToken.None);

        // Assert
        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_WalletCard", partialResult.ViewName);
        Assert.Same(fakeModel, partialResult.Model);
    }

    [Fact]
    public async Task Withdraw_WhenStandardRequest_ReturnsFullView()
    {
        // Arrange
        var fakeModel = new WalletViewModel { OwnerId = "demo_user", Balance = 700m };
        _serviceMock
            .Setup(s => s.WithdrawAsync("demo_user", 300m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeModel);

        // 模擬一般 Standard Request (無 HX-Request Header)
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _controller.Withdraw(300m, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Same(fakeModel, viewResult.Model);
    }
}
