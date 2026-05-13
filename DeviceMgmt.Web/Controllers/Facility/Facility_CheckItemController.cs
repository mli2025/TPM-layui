using DeviceMgmt.App.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

/// <summary>
/// 设备点检项目列表：UI 与 <see cref="Facility_ItemController"/> 保养项目页一致，固定 Type=点检，数据写入 Facility_Item。
/// </summary>
public class Facility_CheckItemController : BaseController
{
    public Facility_CheckItemController(IAuth auth) : base(auth) { }
    public IActionResult Index() => View();
}
