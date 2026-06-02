using System.Text.RegularExpressions;
using DeviceMgmt.Repository.Domain;
using Microsoft.Data.SqlClient;

namespace DeviceMgmt.App.Apps.Facility;

/// <summary>设备台账保存前校验与必填字段补全</summary>
public static class FacilityResourceDetailSaveHelper
{
    public static string? Validate(Facility_ResourceDetail e, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(e.FacilityCode)) return "设备编码不能为空";
        if (string.IsNullOrWhiteSpace(e.FacilityName)) return "设备名称不能为空";
        if (string.IsNullOrWhiteSpace(e.FacilityType)) return "设备类型不能为空";
        if (isNew && e.DeptId <= 0) return "请选择工作中心（部门）";
        if (isNew && e.ResourceId <= 0) return "请选择生产资源";
        return null;
    }

    public static void Normalize(Facility_ResourceDetail e)
    {
        e.FacilityCode = NullToEmpty(e.FacilityCode);
        e.FacilityName = NullToEmpty(e.FacilityName);
        e.FacilityType = NullToEmpty(e.FacilityType);
        e.Manufacturer = NullToEmpty(e.Manufacturer);
        e.Supplier = NullToEmpty(e.Supplier);
        e.ManufactureCountry = NullToEmpty(e.ManufactureCountry);
        e.Model = NullToEmpty(e.Model);
        e.SerialNumber = NullToEmpty(e.SerialNumber);
        e.Location = NullToEmpty(e.Location);
        e.FacilitySign = NullToEmpty(e.FacilitySign);
        e.Standard = NullToEmpty(e.Standard);
        e.Keeper = NullToEmpty(e.Keeper);

        if (e.EntityId <= 0) e.EntityId = 1;
        if (e.ManufacturerDate == default) e.ManufacturerDate = DateTime.Now;
        if (e.KeyFlag != 1) e.KeyFlag = 0;
    }

    public static string ToFriendlyMessage(Exception ex)
    {
        if (ex is SqlException sql) return MapSql(sql) ?? sql.Message;
        var inner = ex.InnerException;
        while (inner != null)
        {
            if (inner is SqlException sqlInner)
            {
                var msg = MapSql(sqlInner);
                if (msg != null) return msg;
            }
            inner = inner.InnerException;
        }
        var text = ex.Message;
        if (text.Contains("NULL", StringComparison.OrdinalIgnoreCase) && text.Contains("INSERT", StringComparison.OrdinalIgnoreCase))
            return MapNullInsert(text) ?? "保存失败：存在必填项未填写，请检查表单";
        return text;
    }

    private static string? MapSql(SqlException sql)
    {
        if (sql.Number is 2627 or 2601) return "保存失败：设备编码已存在，请更换编码";
        return MapNullInsert(sql.Message);
    }

    private static string? MapNullInsert(string message)
    {
        var m = Regex.Match(message, @"列\s*'([^']+)'", RegexOptions.IgnoreCase);
        if (!m.Success) m = Regex.Match(message, @"column\s+'([^']+)'", RegexOptions.IgnoreCase);
        if (!m.Success) return null;
        var col = m.Groups[1].Value;
        var label = ColumnLabels.TryGetValue(col, out var cn) ? cn : col;
        return $"保存失败：「{label}」不能为空";
    }

    private static readonly Dictionary<string, string> ColumnLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Manufacturer"] = "制造商",
        ["Supplier"] = "供应商",
        ["ManufactureCountry"] = "出厂国家",
        ["Model"] = "型号",
        ["SerialNumber"] = "出厂编号",
        ["Location"] = "安装位置",
        ["FacilityCode"] = "设备编码",
        ["FacilityName"] = "设备名称",
        ["FacilityType"] = "设备类型",
        ["DeptId"] = "工作中心",
        ["ResourceId"] = "生产资源"
    };

    private static string NullToEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();
}
