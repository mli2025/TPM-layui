using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>能源计量点（URS 1301-1306；采集由 n8n 写入）</summary>
[Table("Energy_Point")]
public class Energy_Point : Entity
{
    public string PointCode { get; set; } = string.Empty;
    public string? MediaType { get; set; }
    public string? MeterModel { get; set; }
    public string? Protocol { get; set; }
    public int? SampleRate { get; set; }
    public long? DeptId { get; set; }
    public int Status { get; set; } = 1;
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>能源时序数据（n8n 写入）</summary>
[Table("Energy_RealtimeData")]
public class Energy_RealtimeData : Entity
{
    public long PointId { get; set; }
    public DateTime Ts { get; set; }
    public decimal? InstValue { get; set; }
    public decimal? AccuValue { get; set; }
}

/// <summary>能耗汇总</summary>
[Table("Energy_Summary")]
public class Energy_Summary : Entity
{
    public long PointId { get; set; }
    public string? Dimension { get; set; }
    public long? DeptId { get; set; }
    public DateTime? StatDate { get; set; }
    public decimal? SummaryValue { get; set; }
}

/// <summary>能源报警规则</summary>
[Table("Energy_AlarmRule")]
public class Energy_AlarmRule : Entity
{
    public long PointId { get; set; }
    public decimal? Threshold { get; set; }
    public string? AlarmLevel { get; set; }
    public string? NotifyWay { get; set; }
    public string? NotifyUser { get; set; }
    public int Status { get; set; } = 1;
}

/// <summary>能源报警记录</summary>
[Table("Energy_AlarmRecord")]
public class Energy_AlarmRecord : Entity
{
    public long PointId { get; set; }
    public DateTime AlarmTime { get; set; } = DateTime.Now;
    public string? AlarmLevel { get; set; }
    public decimal? AlarmValue { get; set; }
    public int HandleStatus { get; set; }
}

/// <summary>设备运行时长</summary>
[Table("Energy_RunTime")]
public class Energy_RunTime : Entity
{
    public long FacilityId { get; set; }
    public string? WorkSection { get; set; }
    public string? Product { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public decimal? RunHours { get; set; }
}
