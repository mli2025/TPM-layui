using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>工作流模板（通用提交/审核/批准/派发，多模块复用）</summary>
[Table("Wf_Template")]
public class Wf_Template : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? NodeConfig { get; set; }
    public int Status { get; set; } = 1;
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>流程节点（按 Sort 线性流转）</summary>
[Table("Wf_Node")]
public class Wf_Node : Entity
{
    public long TemplateId { get; set; }
    public string NodeKey { get; set; } = string.Empty;
    public string? NodeName { get; set; }
    public string? NodeType { get; set; }
    public string? ApproveRole { get; set; }
    public int? TimeoutHours { get; set; }
    public int Sort { get; set; }
}

/// <summary>流程实例（关联业务记录）</summary>
[Table("Wf_Instance")]
public class Wf_Instance : Entity
{
    public long TemplateId { get; set; }
    public string BizType { get; set; } = string.Empty;
    public long BizId { get; set; }
    public string? CurrentNode { get; set; }
    public int Status { get; set; }
    public long? InitiatorId { get; set; }
    public string? InitiatorName { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
}

/// <summary>审批日志</summary>
[Table("Wf_ApproveLog")]
public class Wf_ApproveLog : Entity
{
    public long InstanceId { get; set; }
    public string? NodeKey { get; set; }
    public long? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public string? Result { get; set; }
    public string? Opinion { get; set; }
    public DateTime ApproveTime { get; set; } = DateTime.Now;
}
