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

/// <summary>点检计划（标准 × 多设备 × 周期 × 班次 的循环规则）。不绑定执行人，按角色分配；由滚动后台任务按规则逐日生成点检执行单。</summary>
[Table("Inspect_Plan")]
public class Inspect_Plan : Entity
{
    public string? PlanNo { get; set; }
    /// <summary>已废弃：旧的点检标准库Id。保留列以兼容历史数据，新流程改用 TemplateId。</summary>
    public long StandardId { get; set; }
    /// <summary>点检模板Id（Facility_TheTemplateMain，Type=点检）。执行单逐项从该模板明细加载。</summary>
    public long TemplateId { get; set; }
    /// <summary>周期：日/周/月/季/年（班次另由 Shifts 控制）。</summary>
    public string? CycleType { get; set; }
    /// <summary>班次（仅周期=班 时有效，逗号分隔，如「早班,中班,夜班」）。每班每日各生成一张执行单。</summary>
    public string? Shifts { get; set; }
    /// <summary>生效起始日期</summary>
    public DateTime? PlanDate { get; set; }
    /// <summary>截止日期（空=长期有效）</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>已废弃：旧的「生成期数」，滚动生成下不再使用，保留列以兼容历史数据。</summary>
    public int Periods { get; set; } = 1;
    /// <summary>已废弃：旧的执行人姓名，改为按角色分配。保留列以兼容历史数据。</summary>
    public string? Executor { get; set; }
    /// <summary>已废弃：旧的执行人员工Id。保留列以兼容历史数据。</summary>
    public long? ExecutorId { get; set; }
    public int ConfirmStatus { get; set; }
    /// <summary>1=启用（参与滚动生成），0=停用</summary>
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

/// <summary>点检计划-角色关联（计划按角色分配，当班的对应角色人员均可执行）</summary>
[Table("Inspect_PlanRole")]
public class Inspect_PlanRole : Entity
{
    public long PlanId { get; set; }
    public long RoleId { get; set; }
}

/// <summary>点检记录（执行单）。由计划按「设备×日期×班次」逐张生成；ExecTime 为空=待执行，非空=已完成。执行人在提交时由当前登录人回填。</summary>
[Table("Inspect_Record")]
public class Inspect_Record : Entity
{
    public string? RecordNo { get; set; }
    public long? PlanId { get; set; }
    public long? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    /// <summary>班次（生成时写入，对应计划班次之一；非班次周期为空）</summary>
    public string? Shift { get; set; }
    /// <summary>执行人（提交时回填当前登录人）</summary>
    public string? Executor { get; set; }
    /// <summary>计划日期（待执行排程用）</summary>
    public DateTime? PlanDate { get; set; }
    public DateTime? ExecTime { get; set; }
    public int Result { get; set; }       // 0正常/1异常
    public string? Remark { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>点检记录明细（逐项结果）。控件类型与上下限随执行单留痕，供自动判定与历史追溯。</summary>
[Table("Inspect_RecordSub")]
public class Inspect_RecordSub : Entity
{
    public long RecordId { get; set; }
    public string? ItemName { get; set; }
    /// <summary>点检方法（来自模板明细 HMethods）</summary>
    public string? Method { get; set; }
    /// <summary>判定标准描述（来自模板明细 HStandard）</summary>
    public string? Standard { get; set; }
    /// <summary>控件类型：1=数值型(按上下限判定) / 0=是否型(选「是」=合格)</summary>
    public int ControlType { get; set; }
    /// <summary>数值型上限</summary>
    public decimal? MaxValue { get; set; }
    /// <summary>数值型下限</summary>
    public decimal? MinValue { get; set; }
    /// <summary>实测值（数值型填数值，是否型填 是/否）</summary>
    public string? ResultValue { get; set; }
    /// <summary>是否合格（由系统按控件类型+上下限自动判定）</summary>
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
