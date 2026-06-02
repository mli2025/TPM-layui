using DeviceMgmt.App.AuthStrategies;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.Web.Services;

/// <summary>
/// 基于当前 HTTP 请求的审计上下文实现：
/// - 操作者来自 BaseController 写入的 HttpContext.Items["CurrentUser"]
/// - 模块取路由 controller 名
/// - 操作理由可由控制器写入 HttpContext.Items["AuditReason"]
/// 无请求上下文（如启动期）时各属性返回 null。
/// </summary>
public sealed class HttpAuditContext : IAuditContext
{
    private readonly IHttpContextAccessor _http;

    public HttpAuditContext(IHttpContextAccessor http) => _http = http;

    private AuthStrategyContext? Current
        => _http.HttpContext?.Items["CurrentUser"] as AuthStrategyContext;

    public long? UserId => Current?.User?.Id;

    public string? UserAccount => Current?.User?.Account ?? Current?.User?.Name;

    public string? IpAddress => _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? Module => _http.HttpContext?.GetRouteValue("controller") as string;

    public string? Reason => _http.HttpContext?.Items["AuditReason"] as string;
}
