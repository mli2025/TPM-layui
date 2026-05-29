using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Workflow;

/// <summary>
/// 通用工作流引擎（线性节点，按 Sort 流转）。供设备转移/报废、维修、维保、检验、校准、送外检复用。
/// </summary>
public class WorkflowApp
{
    private readonly IRepository<Wf_Template> _tplRepo;
    private readonly IRepository<Wf_Node> _nodeRepo;
    private readonly IRepository<Wf_Instance> _instRepo;
    private readonly IRepository<Wf_ApproveLog> _logRepo;

    public WorkflowApp(
        IRepository<Wf_Template> tplRepo,
        IRepository<Wf_Node> nodeRepo,
        IRepository<Wf_Instance> instRepo,
        IRepository<Wf_ApproveLog> logRepo)
    {
        _tplRepo = tplRepo;
        _nodeRepo = nodeRepo;
        _instRepo = instRepo;
        _logRepo = logRepo;
    }

    // ---------- 模板 ----------
    public Wf_Template? GetTemplate(long id) => _tplRepo.FindSingle(id);

    public IEnumerable<Wf_Node> GetNodes(long templateId)
        => _nodeRepo.Find("[TemplateId]=@t", new { t = templateId }, "[Sort] ASC, [Id] ASC");

    public long SaveTemplate(Wf_Template tpl, IEnumerable<Wf_Node> nodes)
    {
        tpl.Code = (tpl.Code ?? string.Empty).Trim();
        tpl.Name = (tpl.Name ?? string.Empty).Trim();
        var dup = _tplRepo.Count("[Code]=@c AND [Id]<>@id", new { c = tpl.Code, id = tpl.Id });
        if (dup > 0) throw new InvalidOperationException("流程编码已存在");

        if (tpl.Id == 0) { if (tpl.Status == 0) tpl.Status = 1; _tplRepo.Insert(tpl); }
        else _tplRepo.Update(tpl);

        // 节点全量替换
        var existed = _nodeRepo.Find("[TemplateId]=@t", new { t = tpl.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _nodeRepo.Delete(existed);
        var sort = 1;
        foreach (var n in nodes ?? Enumerable.Empty<Wf_Node>())
        {
            if (string.IsNullOrWhiteSpace(n.NodeName)) continue;
            n.Id = 0;
            n.TemplateId = tpl.Id;
            n.Sort = sort;
            if (string.IsNullOrWhiteSpace(n.NodeKey)) n.NodeKey = "n" + sort;
            if (string.IsNullOrWhiteSpace(n.NodeType)) n.NodeType = "approve";
            _nodeRepo.Insert(n);
            sort++;
        }
        return tpl.Id;
    }

    public void DeleteTemplate(long id)
    {
        var nodeIds = _nodeRepo.Find("[TemplateId]=@t", new { t = id }).Select(x => x.Id).ToArray();
        if (nodeIds.Length > 0) _nodeRepo.Delete(nodeIds);
        _tplRepo.Delete(id);
    }

    public IEnumerable<Wf_Template> ActiveTemplates()
        => _tplRepo.Find("[Status]=1", null, "[Id] DESC");

    // ---------- 实例流转 ----------
    public Wf_Instance? GetInstance(long id) => _instRepo.FindSingle(id);

    public IEnumerable<Wf_ApproveLog> GetLogs(long instanceId)
        => _logRepo.Find("[InstanceId]=@i", new { i = instanceId }, "[ApproveTime] ASC, [Id] ASC");

    /// <summary>发起流程：定位首节点，写实例 Status=0。返回实例 Id。</summary>
    public long Start(long templateId, string bizType, long bizId, long initiatorId, string? initiatorName)
    {
        var first = GetNodes(templateId).FirstOrDefault();
        var inst = new Wf_Instance
        {
            TemplateId = templateId,
            BizType = bizType ?? string.Empty,
            BizId = bizId,
            CurrentNode = first?.NodeKey,
            Status = first == null ? 1 : 0,
            InitiatorId = initiatorId,
            InitiatorName = initiatorName,
            StartTime = DateTime.Now,
            EndTime = first == null ? DateTime.Now : null
        };
        _instRepo.Insert(inst);
        return inst.Id;
    }

    /// <summary>同意：记录日志，推进到下一节点；无下一节点则完成。</summary>
    public void Approve(long instanceId, long approverId, string? approverName, string? opinion)
    {
        var inst = _instRepo.FindSingle(instanceId);
        if (inst == null || inst.Status != 0) throw new InvalidOperationException("流程不存在或已结束");

        _logRepo.Insert(new Wf_ApproveLog
        {
            InstanceId = instanceId,
            NodeKey = inst.CurrentNode,
            ApproverId = approverId,
            ApproverName = approverName,
            Result = "agree",
            Opinion = opinion,
            ApproveTime = DateTime.Now
        });

        var nodes = GetNodes(inst.TemplateId).ToList();
        var idx = nodes.FindIndex(n => n.NodeKey == inst.CurrentNode);
        if (idx >= 0 && idx < nodes.Count - 1)
        {
            inst.CurrentNode = nodes[idx + 1].NodeKey;
        }
        else
        {
            inst.Status = 1;
            inst.EndTime = DateTime.Now;
        }
        _instRepo.Update(inst);
    }

    /// <summary>驳回：记录日志，实例置 2。</summary>
    public void Reject(long instanceId, long approverId, string? approverName, string? opinion)
    {
        var inst = _instRepo.FindSingle(instanceId);
        if (inst == null || inst.Status != 0) throw new InvalidOperationException("流程不存在或已结束");

        _logRepo.Insert(new Wf_ApproveLog
        {
            InstanceId = instanceId,
            NodeKey = inst.CurrentNode,
            ApproverId = approverId,
            ApproverName = approverName,
            Result = "reject",
            Opinion = opinion,
            ApproveTime = DateTime.Now
        });
        inst.Status = 2;
        inst.EndTime = DateTime.Now;
        _instRepo.Update(inst);
    }

    /// <summary>发起人撤回（仅进行中）。</summary>
    public void Withdraw(long instanceId, long userId)
    {
        var inst = _instRepo.FindSingle(instanceId);
        if (inst == null || inst.Status != 0) throw new InvalidOperationException("流程不存在或已结束");
        if (inst.InitiatorId != userId) throw new InvalidOperationException("只有发起人可撤回");
        inst.Status = 3;
        inst.EndTime = DateTime.Now;
        _instRepo.Update(inst);
    }
}
