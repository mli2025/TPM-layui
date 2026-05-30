using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>批量导入日志（URS 通用能力）</summary>
[Table("Sys_ImportLog")]
public class Sys_ImportLog : Entity
{
    public string BizType { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public int SkipCount { get; set; }
    public string? ErrorDetail { get; set; }
    public long? OperatorId { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>自定义报表定义（URS 通用能力）</summary>
[Table("Sys_ReportDef")]
public class Sys_ReportDef : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? QueryDef { get; set; }
    public string? ChartDef { get; set; }
    public long? OwnerId { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
