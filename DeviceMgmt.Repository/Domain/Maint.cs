using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>维保标准库（独立于工单的标准/规程，URS 801-809）</summary>
[Table("Maint_Standard")]
public class Maint_Standard : Entity
{
    public string? StdNo { get; set; }
    public long? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public string? FacilityType { get; set; }
    public string? CycleType { get; set; }      // 日常/月度/年度 等
    public string? EntrustType { get; set; }     // 自维/外委
    public int Status { get; set; } = 1;
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>维保标准明细（保养项 + 消耗备件）</summary>
[Table("Maint_StandardSub")]
public class Maint_StandardSub : Entity
{
    public long MainId { get; set; }
    public string? ItemName { get; set; }
    public long? SpareId { get; set; }
    public decimal? SpareQty { get; set; }
    public int Sort { get; set; }
}

/// <summary>维保延期申请（计划/工单延期，需审批）</summary>
[Table("Maint_DelayApply")]
public class Maint_DelayApply : Entity
{
    public string? BizType { get; set; }         // 计划/工单
    public long BizId { get; set; }
    public DateTime? OldDate { get; set; }
    public DateTime? NewDate { get; set; }
    public string? Reason { get; set; }
    public string? ApplyUser { get; set; }
    public int ApproveStatus { get; set; }       // 0待审/1通过/2驳回
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>维保资质有效期监控（安全合格证/使用许可证/保修证）</summary>
[Table("Maint_Qualification")]
public class Maint_Qualification : Entity
{
    public string? QualType { get; set; }
    public long? FacilityId { get; set; }
    public DateTime? EffectDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public int WarnDays { get; set; } = 30;
    public int Status { get; set; } = 1;
}
