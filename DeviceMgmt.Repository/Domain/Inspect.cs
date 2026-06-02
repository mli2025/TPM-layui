using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>点巡检标准（主，URS 701-706）。标准是「点检项的组合模板」，与具体设备解耦：一套标准可被多台设备/整类设备复用。</summary>
[Table("Inspect_Standard")]
public class Inspect_Standard : Entity
{
    public string? StdNo { get; set; }
    /// <summary>标准名称</summary>
    public string? StdName { get; set; }
    /// <summary>适用设备类型（可选，用于计划批量带入设备）</summary>
    public string? FacilityType { get; set; }
    public long? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public string? CycleType { get; set; }
    public int Status { get; set; }
    public string? Maker { get; set; }
    /// <summary>编制人员工Id</summary>
    public long? MakerId { get; set; }
    public string? Checker { get; set; }
    public DateTime? CheckDate { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>点巡检标准明细（点检项）</summary>
[Table("Inspect_StandardSub")]
public class Inspect_StandardSub : Entity
{
    public long MainId { get; set; }
    public string? ItemName { get; set; }
    public string? Method { get; set; }
    public string? Standard { get; set; }
    public int Sort { get; set; }
}

/// <summary>点检计划（标准 × 多设备 × 周期 的生成批次，保存后按设备逐台生成点检执行单）</summary>
[Table("Inspect_Plan")]
public class Inspect_Plan : Entity
{
    public string? PlanNo { get; set; }
    public long StandardId { get; set; }
    public string? Executor { get; set; }
    /// <summary>执行人员工Id</summary>
    public long? ExecutorId { get; set; }
    /// <summary>起始计划日期</summary>
    public DateTime? PlanDate { get; set; }
    /// <summary>生成期数（按周期重复生成的张数）</summary>
    public int Periods { get; set; } = 1;
    public int ConfirmStatus { get; set; }
    public int Status { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>点检计划-设备关联（一个计划可覆盖多台设备）</summary>
[Table("Inspect_PlanDevice")]
public class Inspect_PlanDevice : Entity
{
    public long PlanId { get; set; }
    public long FacilityId { get; set; }
    public string? FacilityName { get; set; }
}

/// <summary>点检记录（执行单）。由计划按设备逐台生成；ExecTime 为空=待执行，非空=已完成。</summary>
[Table("Inspect_Record")]
public class Inspect_Record : Entity
{
    public string? RecordNo { get; set; }
    public long? PlanId { get; set; }
    public long? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public string? Executor { get; set; }
    /// <summary>计划日期（待执行排程用）</summary>
    public DateTime? PlanDate { get; set; }
    public DateTime? ExecTime { get; set; }
    public int Result { get; set; }       // 0正常/1异常
    public string? Remark { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>点检记录明细（逐项结果，本补丁新增表 Inspect_RecordSub）</summary>
[Table("Inspect_RecordSub")]
public class Inspect_RecordSub : Entity
{
    public long RecordId { get; set; }
    public string? ItemName { get; set; }
    public string? ResultValue { get; set; }
    public bool IsNormal { get; set; } = true;
    public string? Remark { get; set; }
}

/// <summary>点检异常处置分流（5 类：立即维修/计划检修/停产窗口/观察监控/无需处理）</summary>
[Table("Inspect_Disposal")]
public class Inspect_Disposal : Entity
{
    public long RecordId { get; set; }
    public string? DisposalType { get; set; }
    public long? LinkBillId { get; set; }
    public long? LinkPlanId { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
