using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>安全附件台账（URS 1001-1006）</summary>
[Table("Safety_Accessory")]
public class Safety_Accessory : Entity
{
    public string AccCode { get; set; } = string.Empty;
    public long? FacilityId { get; set; }
    public string? Model { get; set; }
    public string? SetPressure { get; set; }
    public string? CheckRange { get; set; }
    public DateTime? LastCheckDate { get; set; }
    public int? CheckCycle { get; set; }
    public string? CheckOrg { get; set; }
    public int Status { get; set; } = 1;
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>检定计划</summary>
[Table("Safety_CheckPlan")]
public class Safety_CheckPlan : Entity
{
    public long AccId { get; set; }
    public DateTime? PlanDate { get; set; }
    public string? Owner { get; set; }
    public int Status { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>检定记录</summary>
[Table("Safety_CheckRecord")]
public class Safety_CheckRecord : Entity
{
    public long AccId { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? TakeDate { get; set; }
    public string? CheckResult { get; set; }
    public DateTime? NextCheckDate { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
