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
        Response.Cookies.Append("Token", result.Token, new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.Now.AddHours(8)
        });
        return Json(new { code = 200, msg = "ok", data = new { token = result.Token, returnUrl = returnUrl ?? Url.Content("~/") } });
    }

    [HttpGet]
    public IActionResult Logout()
    {
        var token = Request.Cookies["Token"];
        if (!string.IsNullOrEmpty(token)) _auth.Logout(token);
        Response.Cookies.Delete("Token");
        return Redirect(Url.Content("~/Account/Login"));
    }
}
