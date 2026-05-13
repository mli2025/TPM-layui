using DeviceMgmt.App.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

/// <summary>
/// 设备点检模板列表：与 <see cref="Facility_TheTemplateMainController"/> 保养模板页一致，固定 Type=点检，接口全部转发至该控制器。
/// </summary>
public class Facility_CheckTemplateController : BaseController
{
    public Facility_CheckTemplateController(IAuth auth) : base(auth) { }
    public IActionResult Index() => View();
}
