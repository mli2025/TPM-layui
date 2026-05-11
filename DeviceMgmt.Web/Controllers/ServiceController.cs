using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers;

public class ServiceController : BaseController
{
    private readonly IUnitWork _uw;

    public ServiceController(IAuth auth, IUnitWork uw) : base(auth)
    {
        _uw = uw;
    }

    [HttpPost]
    public IActionResult GetSelect2Data([FromForm] QuerySelect2Req req)
    {
        var data = new List<object>();
        if (!string.IsNullOrWhiteSpace(req.table) && !string.IsNullOrWhiteSpace(req.valueColumn) && !string.IsNullOrWhiteSpace(req.displayColumn))
        {
            var where = string.Empty;
            if (!string.IsNullOrWhiteSpace(req.q))
            {
                where = $" WHERE [{req.displayColumn}] LIKE @q";
            }
            var sql = $"SELECT TOP 50 [{req.valueColumn}] AS id, [{req.displayColumn}] AS text FROM [dbo].[{req.table}]" + where;
            using var conn = _uw.OpenConnection();
            try
            {
                data = conn.Query<object>(sql, new { q = "%" + (req.q ?? string.Empty) + "%" }).ToList();
            }
            catch
            {
                data = new List<object>();
            }
        }
        return Json(new { data, total_count = data.Count });
    }

    [HttpPost]
    public IActionResult GetSelect2InitItem([FromForm] QuerySelect2Req req)
    {
        var data = new List<object>();
        if (!string.IsNullOrWhiteSpace(req.table) && !string.IsNullOrWhiteSpace(req.valueColumn) && !string.IsNullOrWhiteSpace(req.displayColumn))
        {
            var sql = $"SELECT TOP 1 [{req.valueColumn}] AS id, [{req.displayColumn}] AS text FROM [dbo].[{req.table}] WHERE [{req.valueColumn}]=@v";
            using var conn = _uw.OpenConnection();
            try
            {
                data = conn.Query<object>(sql, new { v = req.selectedId }).ToList();
            }
            catch
            {
                data = new List<object>();
            }
        }
        return Json(new { data });
    }
}
