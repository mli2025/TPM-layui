using System.Globalization;
using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Microsoft.Data.SqlClient;

namespace DeviceMgmt.Web.Services.Import;

/// <summary>设备台账 Excel 导入（编码已存在则更新）</summary>
public sealed class FacilityImportHandler : IImportHandler
{
    private readonly IRepository<Facility_ResourceDetail> _repo;
    private readonly IRepository<Sys_Dept> _deptRepo;
    private readonly IRepository<Basic_Resource> _resourceRepo;

    /// <summary>null=未检测；true=有表；false=无表</summary>
    private bool? _resourceTableExists;

    public FacilityImportHandler(
        IRepository<Facility_ResourceDetail> repo,
        IRepository<Sys_Dept> deptRepo,
        IRepository<Basic_Resource> resourceRepo)
    {
        _repo = repo;
        _deptRepo = deptRepo;
        _resourceRepo = resourceRepo;
    }

    public const string BizTypeConst = "Facility_ResourceDetail";
    public string BizType => BizTypeConst;
    public string DisplayName => "设备台账";

    public string[] Headers => new[]
    {
        "设备编码", "设备名称", "设备型号", "生产资源编码", "制造厂商", "制造国家", "供应商",
        "设备分类", "使用状态", "资产编码", "部门", "放置区域", "关键设备", "备注"
    };

    public (bool ok, bool skip, string? error) ImportRow(IDictionary<string, string> row)
    {
        var code = Get(row, "设备编码", "FacilityCode");
        if (string.IsNullOrWhiteSpace(code)) return (false, true, "设备编码为空，跳过");

        var entity = _repo.FindSingle("[FacilityCode]=@c", new { c = code.Trim() }) ?? new Facility_ResourceDetail();
        entity.FacilityCode = code.Trim();
        entity.FacilityName = Get(row, "设备名称", "FacilityName") ?? entity.FacilityName;
        entity.Model = Get(row, "设备型号", "Model") ?? entity.Model;
        entity.Manufacturer = Get(row, "制造厂商", "Manufacturer") ?? entity.Manufacturer;
        entity.ManufactureCountry = Get(row, "制造国家", "ManufactureCountry") ?? entity.ManufactureCountry;
        entity.Supplier = Get(row, "供应商", "Supplier") ?? entity.Supplier;
        entity.FacilityType = Get(row, "设备分类", "FacilityType", "设备类型") ?? entity.FacilityType;
        entity.AssetNumber = Get(row, "资产编码", "AssetNumber");
        entity.Location = Get(row, "放置区域", "Location") ?? entity.Location;
        entity.Remark = Get(row, "备注", "Remark");

        if (string.IsNullOrWhiteSpace(entity.FacilityType)) entity.FacilityType = "未分类";

        var statusText = Get(row, "使用状态", "Status");
        if (!string.IsNullOrWhiteSpace(statusText) && TryParseStatus(statusText, out var st)) entity.Status = st;

        var keyText = Get(row, "关键设备", "KeyFlag");
        if (!string.IsNullOrWhiteSpace(keyText)) entity.KeyFlag = ParseKeyFlag(keyText);

        var resCode = Get(row, "生产资源编码", "生产资源组", "ResourceCode");
        if (!string.IsNullOrWhiteSpace(resCode))
        {
            if (!TryResolveResourceId(resCode.Trim(), out var rid, out var resErr))
                return (false, false, resErr);
            if (rid > 0) entity.ResourceId = rid;
        }

        var deptName = Get(row, "部门", "车间名称", "DeptName");
        if (!string.IsNullOrWhiteSpace(deptName))
        {
            var dept = _deptRepo.FindSingle("[DeptName]=@n", new { n = deptName.Trim() });
            if (dept == null) return (false, false, $"部门「{deptName}」在部门表中不存在");
            entity.DeptId = dept.Id;
        }

        var err = FacilityResourceDetailSaveHelper.ValidateForImport(entity, entity.Id == 0);
        if (err != null) return (false, false, err);
        FacilityResourceDetailSaveHelper.Normalize(entity);

        if (entity.Id == 0) _repo.Insert(entity);
        else _repo.Update(entity);
        return (true, false, null);
    }

    private bool TryResolveResourceId(string code, out long resourceId, out string? error)
    {
        resourceId = 0;
        error = null;
        EnsureResourceTableChecked();
        if (_resourceTableExists == false)
            return true;

        try
        {
            var res = _resourceRepo.FindSingle("[Code]=@c", new { c = code });
            if (res == null)
            {
                error = $"生产资源编码「{code}」不存在（请填 Basic_Resource 表中的编码，或留空）";
                return false;
            }
            resourceId = res.Id;
            return true;
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            _resourceTableExists = false;
            return true;
        }
    }

    private void EnsureResourceTableChecked()
    {
        if (_resourceTableExists.HasValue) return;
        try
        {
            _resourceRepo.Count(null);
            _resourceTableExists = true;
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            _resourceTableExists = false;
        }
    }

    private static string? Get(IDictionary<string, string> row, params string[] keys)
    {
        foreach (var k in keys)
            if (row.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v)) return v.Trim();
        return null;
    }

    private static int ParseKeyFlag(string s)
    {
        s = s.Trim();
        if (s is "1" or "是" or "Y" or "y" or "yes" or "Yes") return 1;
        return 0;
    }

    private static bool TryParseStatus(string s, out int status)
    {
        status = 0;
        s = s.Trim();
        return s switch
        {
            "0" or "运行" => Set(0, out status),
            "1" or "闲置" => Set(1, out status),
            "2" or "停机" => Set(2, out status),
            "3" or "报废" => Set(3, out status),
            _ => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out status)
        };
    }

    private static bool Set(int v, out int status) { status = v; return true; }
}
