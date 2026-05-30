using DeviceMgmt.App.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DeviceMgmt.Web.Controllers.Base;

public class BaseController : Controller
{
    public const string TokenKey = "Token";
    protected readonly IAuth _authUtil;

    public BaseController(IAuth authUtil)
    {
        _authUtil = authUtil;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var path = request.Path.Value ?? "/";
        var token = string.Empty;
        if (request.Cookies.TryGetValue(TokenKey, out var cookieVal) && !string.IsNullOrEmpty(cookieVal))
            token = cookieVal;
        if (string.IsNullOrEmpty(token))
            token = request.Headers[TokenKey].ToString();

        if (string.IsNullOrEmpty(token) || !_authUtil.CheckLogin(token))
        {
            if (IsAjax(request))
            {
                context.Result = new JsonResult(new { code = 401, msg = "Unauthorized" });
            }
            else
            {
                context.Result = LoginResult(path);
            }
            return;
        }

        // 滑动续期：每次受保护请求刷新会话有效期，实现空闲超时自动注销（URS 407）
        _authUtil.RenewToken(token);

        var user = _authUtil.GetCurrentUser(token);
        if (user != null)
        {
            HttpContext.Items["CurrentUser"] = user;
            HttpContext.Items["Token"] = token;

            ViewData["CurrentUser"] = user.User;
            ViewData["Modules"] = user.Modules;
            ViewData["ModuleElements"] = user.ModuleElements;

            var moduleId = request.Query["moduleId"];
            if (!string.IsNullOrEmpty(moduleId.ToString()))
            {
                if (long.TryParse(moduleId.ToString(), out var reqModule))
                {
                    ViewData["Buttons"] = user.ModuleElements.Where(u => u.ModuleId == reqModule).ToList();
                }
            }
        }

        base.OnActionExecuting(context);
    }

    protected string CurrentToken => HttpContext.Items["Token"] as string ?? string.Empty;

    protected DeviceMgmt.App.AuthStrategies.AuthStrategyContext? CurrentUser
        => HttpContext.Items["CurrentUser"] as DeviceMgmt.App.AuthStrategies.AuthStrategyContext;

    protected virtual IActionResult LoginResult(string path)
    {
        var safe = Url.Content("~/" + path.TrimStart('/'));
        return new RedirectResult("~/Account/Login?returnUrl=" + Uri.EscapeDataString(safe));
    }

    private static bool IsAjax(HttpRequest req)
    {
        if (req.Headers.TryGetValue("X-Requested-With", out var v) && v.ToString().Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            return true;
        var accept = req.Headers.Accept.ToString();
        if (accept.Contains("application/json", StringComparison.OrdinalIgnoreCase)) return true;
        // mobile fetch APIs at /m/api/* always JSON
        var path = req.Path.Value ?? string.Empty;
        if (path.StartsWith("/m/api/", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
