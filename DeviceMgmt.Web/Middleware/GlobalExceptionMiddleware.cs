using System.Text;
using Newtonsoft.Json;

namespace DeviceMgmt.Web.Middleware;

/// <summary>
/// 兜底异常处理：API 路径(/api/* 或 Accept: application/json) 返回 {code:-1,msg:...}; 其他走默认 500
/// 配合 Serilog 输出完整堆栈
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception at {Path}", ctx.Request.Path);

            if (ctx.Response.HasStarted) throw;

            var path = ctx.Request.Path.Value ?? string.Empty;
            var wantsJson = path.Contains("/api/", StringComparison.OrdinalIgnoreCase)
                || ctx.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase)
                || ctx.Request.Headers.XRequestedWith.ToString().Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (wantsJson)
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "application/json; charset=utf-8";
                var body = JsonConvert.SerializeObject(new
                {
                    code = -1,
                    msg = ex.Message,
                    detail = ex.GetType().Name
                });
                await ctx.Response.WriteAsync(body, Encoding.UTF8);
                return;
            }

            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "text/plain; charset=utf-8";
            await ctx.Response.WriteAsync($"500 Internal Server Error: {ex.Message}", Encoding.UTF8);
        }
    }
}
