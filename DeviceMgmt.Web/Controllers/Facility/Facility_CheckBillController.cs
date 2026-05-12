using DeviceMgmt.App.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

/// <summary>
/// 设备点检单列表（view-only），数据复用 Facility_BillMain，按 BillType=INSPECTION 过滤
/// </summary>
public class Facility_CheckBillController : BaseController
{
    public Facility_CheckBillController(IAuth auth) : base(auth) { }
    public IActionResult Index() => View();
}
