using DeviceMgmt.App.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

/// <summary>
/// 设备点检项目列表（view-only），数据复用 Facility_Item，按 Type=1 过滤
/// </summary>
public class Facility_CheckItemController : BaseController
{
    public Facility_CheckItemController(IAuth auth) : base(auth) { }
    public IActionResult Index() => View();
}
