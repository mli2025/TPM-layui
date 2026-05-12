using DeviceMgmt.App.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuth _auth;

    public AccountController(IAuth auth)
    {
        _auth = auth;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/");
        return View();
    }

    [HttpPost]
    public IActionResult DoLogin([FromForm] string account, [FromForm] string password, [FromForm] string? returnUrl)
    {
        var result = _auth.Login("Web", account, password);
        if (!result.success || string.IsNullOrEmpty(result.Token))
        {
            return Json(new { code = result.code, msg = result.msg });
        }
        var home = Url.Content("~/");
        var safeReturn = string.IsNullOrWhiteSpace(returnUrl) ? home : returnUrl.Trim();
        Response.Cookies.Append("Token", result.Token, new CookieOptions
        {
            Path = "/",
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.Now.AddHours(8)
        });
        return Json(new { code = 200, msg = "ok", data = new { token = result.Token, returnUrl = safeReturn } });
    }

    [HttpGet]
    public IActionResult Logout()
    {
        var token = Request.Cookies["Token"];
        if (!string.IsNullOrEmpty(token)) _auth.Logout(token);
        Response.Cookies.Delete("Token", new CookieOptions { Path = "/" });
        return Redirect(Url.Content("~/Account/Login"));
    }

    [HttpGet]
    public IActionResult Profile()
    {
        var token = Request.Cookies["Token"] ?? Request.Headers["Token"].ToString();
        var ctx = _auth.GetCurrentUser(token);
        if (ctx == null)
        {
            return Json(new { code = 401, msg = "未登录" });
        }
        return Json(new
        {
            code = 200,
            msg = "ok",
            data = new
            {
                id = ctx.User.Id,
                account = ctx.User.Account,
                name = ctx.User.Name,
                status = ctx.User.Status,
                moduleCount = ctx.Modules.Count
            }
        });
    }

    [HttpPost]
    public IActionResult ChangePassword([FromForm] string oldPassword, [FromForm] string newPassword)
    {
        var token = Request.Cookies["Token"] ?? Request.Headers["Token"].ToString();
        var result = _auth.ChangePassword(token, oldPassword, newPassword);
        return Json(new { code = result.code, msg = result.msg, success = result.success });
    }
}
