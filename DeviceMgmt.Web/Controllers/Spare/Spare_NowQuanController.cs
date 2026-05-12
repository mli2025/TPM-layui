using System.Text;
using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Spare;

public class Spare_NowQuanController : BaseController
{
    private readonly Spare_NowQuanApp _app;
    private readonly IRepository<Spare_NowQuan> _stockRepo;

    public Spare_NowQuanController(IAuth auth, Spare_NowQuanApp app, IRepository<Spare_NowQuan> stockRepo) : base(auth)
    {
        _app = app;
        _stockRepo = stockRepo;
    }

    public IActionResult Index() => View();
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req)
    {
        // join stock with spare master, filter by spare keyword
        var page = Math.Max(1, req.page);
        var limit = Math.Max(1, req.limit);
        var skip = (page - 1) * limit;

        string? kw = null;
        long? whId = null;
        if (req.searchParam != null)
        {
            foreach (var sp in req.searchParam)
            {
                if (sp == null || string.IsNullOrWhiteSpace(sp.value)) continue;
                if (sp.field == "Kw") kw = sp.value;
                else if (sp.field == "WarehouseId" && long.TryParse(sp.value, out var v)) whId = v;
            }
        }

        var whereSb = new StringBuilder("1=1");
        var p = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(kw))
        {
            whereSb.Append(" AND (s.[Code] LIKE @k OR s.[Name] LIKE @k OR s.[Specs] LIKE @k)");
            p["k"] = "%" + kw + "%";
        }
        if (whId.HasValue)
        {
            whereSb.Append(" AND q.[WarehouseId]=@w");
            p["w"] = whId.Value;
        }

        var cntSql = $"SELECT COUNT(1) FROM [Spare_NowQuan] q LEFT JOIN [Basic_Spare] s ON s.[Id]=q.[SpareId] WHERE {whereSb}";
        var listSql = $@"
SELECT q.[Id], q.[SpareId], q.[WarehouseId], q.[Qty], s.[Code] AS SpareCode, s.[Name] AS SpareName, s.[Specs], s.[Danwei], s.[SafeStock]
FROM [Spare_NowQuan] q LEFT JOIN [Basic_Spare] s ON s.[Id]=q.[SpareId]
WHERE {whereSb}
ORDER BY q.[Id] DESC
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
        p["skip"] = skip;
        p["take"] = limit;

        var total = _stockRepo.Query<int>(cntSql, p).FirstOrDefault();
        var data = _stockRepo.Query<dynamic>(listSql, p).ToList();
        return Json(new TableData { code = 0, count = total, data = data });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Spare_NowQuan>) ?? Enumerable.Empty<Spare_NowQuan>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Spare_NowQuan");
        return File(bytes, "application/vnd.ms-excel", "Spare_NowQuan.xls");
    }
}
