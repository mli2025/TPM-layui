using DeviceMgmt.App.Apps.Maint;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Maint;

/// <summary>维保资质有效期监控</summary>
public class Maint_QualificationController : BaseController
{
    private readonly Maint_QualificationApp _app;
    private readonly IRepository<Facility_ResourceDetail> _deviceRepo;

    public Maint_QualificationController(IAuth auth, Maint_QualificationApp app, IRepository<Facility_ResourceDetail> deviceRepo) : base(auth)
    {
        _app = app;
        _deviceRepo = deviceRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req)
    {
        var page = _app.Getmainlist(req);
        var rows = (page.data as IEnumerable<Maint_Qualification>)?.ToList() ?? new List<Maint_Qualification>();
        var devIds = rows.Where(r => r.FacilityId.HasValue).Select(r => r.FacilityId!.Value).Distinct().ToArray();
        var devMap = devIds.Length == 0 ? new Dictionary<long, Facility_ResourceDetail>()
            : _deviceRepo.Find("[Id] IN @ids", new { ids = devIds }).GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First());
        var data = rows.Select(r =>
        {
            devMap.TryGetValue(r.FacilityId ?? 0, out var dev);
            return new
            {
                r.Id, r.QualType, r.FacilityId, r.EffectDate, r.ExpireDate, r.WarnDays, r.Status,
                FacilityCode = dev?.FacilityCode, FacilityName = dev?.FacilityName
            };
        }).ToList();
        return Json(new TableData { code = 0, count = page.count, data = data });
    }

    [HttpGet]
    public IActionResult GetInfo([FromQuery] long id) => Json(new ResponseData { code = 0, data = _app.Get(id) });

    private static readonly string[] QualImportHeaders =
        { "资质类型", "设备编码", "生效日期", "到期日期", "提前预警天数", "状态(有效/停用)" };

    [HttpGet]
    public IActionResult ImportTemplate()
    {
        var bytes = NPOIHelper.BuildTemplate(QualImportHeaders, "资质导入模板");
        return File(bytes, "application/vnd.ms-excel", "维保资质导入模板.xls");
    }

    [HttpPost]
    public IActionResult ImportExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0) return Json(new ResponseData { code = 400, msg = "请选择 Excel 文件" });
        int success = 0, fail = 0;
        var errors = new List<string>();
        using (var stream = file.OpenReadStream())
        {
            var (_, rows) = NPOIHelper.ReadRows(stream, file.FileName);
            var line = 1;
            foreach (var r in rows)
            {
                line++;
                var qualType = Get(r, "资质类型").Trim();
                var devCode = Get(r, "设备编码").Trim();
                if (string.IsNullOrWhiteSpace(qualType)) { fail++; errors.Add($"第{line}行：资质类型为空"); continue; }
                long? facilityId = null;
                if (!string.IsNullOrWhiteSpace(devCode))
                {
                    var dev = _deviceRepo.Find("[FacilityCode]=@c", new { c = devCode }, "[Id] DESC").FirstOrDefault();
                    if (dev == null) { fail++; errors.Add($"第{line}行：设备编码 {devCode} 不存在"); continue; }
                    facilityId = dev.Id;
                }
                var statusText = Get(r, "状态(有效/停用)");
                var m = new Maint_Qualification
                {
                    QualType = qualType,
                    FacilityId = facilityId,
                    EffectDate = ParseDate(Get(r, "生效日期")),
                    ExpireDate = ParseDate(Get(r, "到期日期")),
                    WarnDays = int.TryParse(Get(r, "提前预警天数").Trim(), out var wd) ? wd : 30,
                    Status = (statusText.Contains("停") || statusText.Trim() == "0") ? 0 : 1
                };
                try { _app.SaveQual(m); success++; }
                catch (Exception ex) { fail++; errors.Add($"第{line}行：{ex.Message}"); }
            }
        }
        return Json(new ResponseData { code = 0, msg = "ok", data = new { success, fail, errors = errors.Take(50) } });
    }

    private static string Get(Dictionary<string, string> r, string key) => r.TryGetValue(key, out var v) ? (v ?? string.Empty) : string.Empty;
    private static DateTime? ParseDate(string s) => DateTime.TryParse((s ?? string.Empty).Trim(), out var d) ? d : (DateTime?)null;

    [HttpPost]
    public IActionResult Save([FromBody] Maint_Qualification model)
    {
        if (model == null) return Json(new ResponseData { code = 400, msg = "no data" });
        var id = _app.SaveQual(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
