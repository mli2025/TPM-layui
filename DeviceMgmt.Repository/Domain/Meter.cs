using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>计量器具档案（URS 1101-1117）</summary>
[Table("Meter")]
public class Meter : Entity
{
    public string MeterCode { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Model { get; set; }
    public string? Category { get; set; }
    public string? Accuracy { get; set; }
    public string? Range { get; set; }
    public long? DeptId { get; set; }
    public string? Location { get; set; }
    public string? Keeper { get; set; }
    public int Status { get; set; } = 1;
    public int? CalibCycle { get; set; }
    public string? LedgerJson { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>器具出入库</summary>
[Table("Meter_InOut")]
public class Meter_InOut : Entity
{
    public long MeterId { get; set; }
    public int IoType { get; set; }
    public DateTime IoTime { get; set; } = DateTime.Now;
    public string? Operator { get; set; }
    public string? Remark { get; set; }
}

/// <summary>校准计划</summary>
[Table("Meter_CalibPlan")]
public class Meter_CalibPlan : Entity
{
    public long MeterId { get; set; }
    public DateTime? LastCalibDate { get; set; }
    public DateTime? NextCalibDate { get; set; }
    public int Status { get; set; }
    public string? Executor { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>校准记录（复核确认后生效）</summary>
[Table("Meter_CalibRecord")]
public class Meter_CalibRecord : Entity
{
    public long MeterId { get; set; }
    public DateTime? CalibDate { get; set; }
    public string? Executor { get; set; }
    public string? Regulation { get; set; }
    public string? EnvCondition { get; set; }
    public string? StdDevice { get; set; }
    public string? MeasureData { get; set; }
    public string? Uncertainty { get; set; }
    public string? Conclusion { get; set; }
    public DateTime? ValidDate { get; set; }
    public string? CertFile { get; set; }
    public string? Reviewer { get; set; }
    public DateTime? ReviewDate { get; set; }
    public bool IsEffective { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>送外检申请单（主）</summary>
[Table("Meter_SendOut")]
public class Meter_SendOut : Entity
{
    public string? ApplyNo { get; set; }
    public DateTime? SendDate { get; set; }
    public string? ServiceOrg { get; set; }
    public int ApproveStatus { get; set; }
    public string? Applicant { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>送外检明细（子）</summary>
[Table("Meter_SendOutSub")]
public class Meter_SendOutSub : Entity
{
    public long MainId { get; set; }
    public long MeterId { get; set; }
}
