using DeviceMgmt.App.Interface;
using DeviceMgmt.Web.Controllers.Base;
using DeviceMgmt.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DeviceMgmt.Web.Controllers;

public class HomeController : BaseController
{
    public HomeController(IAuth auth) : base(auth) { }

    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
