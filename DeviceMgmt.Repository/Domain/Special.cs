using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>特种设备台账（URS 901-905）</summary>
[Table("Special_Equipment")]
public class Special_Equipment : Entity
{
    public string EquipCode { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? RegisterNo { get; set; }
    public string? UseCertNo { get; set; }
    public decimal? DesignLife { get; set; }
    public string? SafetyLevel { get; set; }
    public DateTime? NextInspectDate { get; set; }
    public string? InspectOrg { get; set; }
    public long? FacilityId { get; set; }
    public string? ExtJson { get; set; }
    public int Status { get; set; } = 1;
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>法定检验计划</summary>
[Table("Special_InspectPlan")]
public class Special_InspectPlan : Entity
{
    public string? PlanNo { get; set; }
    public long EquipId { get; set; }
    public int? CycleMonths { get; set; }
    public DateTime? LastInspectDate { get; set; }
    public DateTime? NextInspectDate { get; set; }
    public int Status { get; set; }
    public string? Owner { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>检验记录</summary>
[Table("Special_InspectRecord")]
public class Special_InspectRecord : Entity
{
    public long EquipId { get; set; }
    public string? InspectOrg { get; set; }
    public DateTime? InspectDate { get; set; }
    public string? ReportFile { get; set; }
    public string? Rectification { get; set; }
    public string? ReInspect { get; set; }
    public string? Conclusion { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
