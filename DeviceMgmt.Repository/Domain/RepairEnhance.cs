using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>维修工单模板（常见故障标准处置）</summary>
[Table("Facility_RepairTemplate")]
public class Facility_RepairTemplate : Entity
{
    public string? TemplateName { get; set; }
    public string? FaultCategory { get; set; }
    public string? StdSteps { get; set; }
    public decimal? StdHours { get; set; }
    public string? StdParts { get; set; }
    public int Status { get; set; } = 1;
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>维修费用多设备分摊</summary>
[Table("Facility_RepairCost")]
public class Facility_RepairCost : Entity
{
    public long RepairBillId { get; set; }
    public long FacilityId { get; set; }
    public string? CostType { get; set; }     // 人工/备件/外委/其他
    public decimal? Amount { get; set; }
    public decimal? Ratio { get; set; }       // 分摊比例 %
    public string? Remark { get; set; }
}

/// <summary>报警规则（阈值/条件）</summary>
[Table("Facility_AlarmRule")]
public class Facility_AlarmRule : Entity
{
    public string? RuleName { get; set; }
    public long? FacilityId { get; set; }
    public string? FacilityType { get; set; }
    public string? MetricName { get; set; }
    public string? Operator { get; set; }     // >,>=,<,<=,==
    public decimal? Threshold { get; set; }
    public string? Level { get; set; }        // 提示/警告/严重
    public int Enabled { get; set; } = 1;
    public string? NotifyTo { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>报警记录（采集/工作流 写入，或手工登记）</summary>
[Table("Facility_AlarmRecord")]
public class Facility_AlarmRecord : Entity
{
    public long? RuleId { get; set; }
    public long? FacilityId { get; set; }
    public string? MetricName { get; set; }
    public decimal? MetricValue { get; set; }
    public string? Level { get; set; }
    public DateTime AlarmTime { get; set; } = DateTime.Now;
    public int Handled { get; set; }
    public string? Handler { get; set; }
    public string? HandleRemark { get; set; }
}
