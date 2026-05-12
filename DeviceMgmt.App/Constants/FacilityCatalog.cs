namespace DeviceMgmt.App.Constants;

/// <summary>
/// 保养项目、模板主表共用的业务分类（数据库存 smallint：1/2/3）。
/// </summary>
public static class FacilityCategoryType
{
    public const short Inspection = 1;
    public const short Maintenance = 2;
    public const short Mold = 3;

    public static bool IsDefined(short value) => value is Inspection or Maintenance or Mold;

    public static string GetChineseLabel(short value) => value switch
    {
        Inspection => "点检项目",
        Maintenance => "保养项目",
        Mold => "模具保养",
        _ => value.ToString()
    };
}

/// <summary>
/// 模板保养周期，数据库存英文码（NVARCHAR）。
/// </summary>
public static class TemplateMaintenanceCycle
{
    public const string Year = "YEAR";
    public const string Quarter = "QUARTER";
    public const string Month = "MONTH";
    public const string Week = "WEEK";

    public static string? NormalizeToCode(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var s = input.Trim();
        var u = s.ToUpperInvariant();
        if (u is "YEAR" or "QUARTER" or "MONTH" or "WEEK") return u;
        return s switch
        {
            "年" => Year,
            "季" => Quarter,
            "月" => Month,
            "周" => Week,
            _ => u
        };
    }

    public static string GetChineseLabel(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;
        return code.Trim().ToUpperInvariant() switch
        {
            Year => "年",
            Quarter => "季",
            Month => "月",
            Week => "周",
            _ => code
        };
    }

    public static bool IsValidCode(string? code)
    {
        var n = NormalizeToCode(code);
        return n is Year or Quarter or Month or Week;
    }
}
