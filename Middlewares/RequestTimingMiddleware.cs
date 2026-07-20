using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotNetMvcWeb.Middlewares;

/// <summary>
/// 範例 Middleware：用來紀錄每個 HTTP Request 執行時間
/// </summary>
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Stopwatch stopwatch = Stopwatch.StartNew();

        // 呼叫管線中的下一個 Middleware
        await _next(context);

        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        // 紀錄執行時間
        _logger.LogInformation(
            "Request [{Method}] {Path} executed in {ElapsedMilliseconds} ms",
            context.Request.Method,
            context.Request.Path,
            elapsedMilliseconds);
    }
}

/// <summary>
/// 提供擴充方法，方便在 Program.cs 中透過 app.UseRequestTiming() 註冊此 Middleware
/// </summary>
public static class RequestTimingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.UseMiddleware<RequestTimingMiddleware>();
    }
}
