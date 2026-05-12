using DeviceMgmt.App.Interface;
using DeviceMgmt.Repository.Domain;
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

        var user = _authUtil.GetCurrentUser(token);
        if (user != null)
        {
            HttpContext.Items["CurrentUser"] = user;
            HttpContext.Items["Token"] = token;

            ViewData["CurrentUser"] = user.User;
            ViewData["Modules"] = EnsureMaintenanceMenus(user.Modules);
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
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static List<Sys_Module> EnsureMaintenanceMenus(List<Sys_Module>? modules)
    {
        var list = (modules ?? new List<Sys_Module>()).ToList();
        if (list.Count == 0) return list;

        long parentId = 0;
        var parent = list.FirstOrDefault(x => string.Equals(x.Name, "设备保养", StringComparison.OrdinalIgnoreCase));
        if (parent != null) parentId = parent.Id;
        if (parentId == 0)
        {
            var equipParent = list.FirstOrDefault(x => string.Equals(x.Name, "设备管理", StringComparison.OrdinalIgnoreCase) && (x.ParentId == 0 || x.ParentId == -1));
            parentId = 901002;
            list.Add(new Sys_Module
            {
                Id = parentId,
                Name = "设备保养",
                Code = "maintenance",
                Url = null,
                ParentId = equipParent?.Id ?? 0,
                Sort = 9,
                Status = 1,
                Icon = "wrench"
            });
        }

        AddIfMissing(list, new Sys_Module { Id = 901201, Name = "设备保养项目列表", Code = "mt-item", Url = "/Facility_Item/Index", ParentId = parentId, Sort = 1, Status = 1, Icon = "list" });
        AddIfMissing(list, new Sys_Module { Id = 901202, Name = "设备保养模板列表", Code = "mt-template", Url = "/Facility_TheTemplateMain/Index", ParentId = parentId, Sort = 2, Status = 1, Icon = "layout-list" });
        AddIfMissing(list, new Sys_Module { Id = 901203, Name = "外委保养列表", Code = "mt-outsourcing", Url = "/Facility_OutsourcingMaintenance/Index", ParentId = parentId, Sort = 3, Status = 1, Icon = "truck" });
        AddIfMissing(list, new Sys_Module { Id = 901204, Name = "保养工单列表", Code = "mt-bill", Url = "/Facility_BillMain/Index", ParentId = parentId, Sort = 4, Status = 1, Icon = "clipboard-list" });

        return list;
    }

    private static void AddIfMissing(List<Sys_Module> list, Sys_Module module)
    {
        if (list.Any(x =>
            (!string.IsNullOrWhiteSpace(module.Url) && string.Equals(x.Url, module.Url, StringComparison.OrdinalIgnoreCase))
            || x.Id == module.Id))
        {
            return;
        }
        list.Add(module);
    }
}
