using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Inspect;

/// <summary>点检标准（主子）</summary>
public class Inspect_StandardApp : BaseApp<Inspect_Standard>
{
    private readonly IRepository<Inspect_StandardSub> _subRepo;

    public Inspect_StandardApp(IUnitWork unitWork, IRepository<Inspect_Standard> repository, IRepository<Inspect_StandardSub> subRepo)
        : base(unitWork, repository) { _subRepo = subRepo; }

    public List<Inspect_StandardSub> GetSubs(long mainId)
        => _subRepo.Find("[MainId]=@m", new { m = mainId }, "[Sort] ASC,[Id] ASC").ToList();

    public long Save(Inspect_Standard main, IEnumerable<Inspect_StandardSub>? subs)
    {
        if (main.Id == 0)
        {
            main.CreateDate = DateTime.Now;
            if (main.Status == 0) main.Status = 1;
            if (string.IsNullOrWhiteSpace(main.StdNo)) main.StdNo = "IS" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Repository.Insert(main);
        }
        else Repository.Update(main);
        var existed = _subRepo.Find("[MainId]=@m", new { m = main.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _subRepo.Delete(existed);
        var sort = 1;
        foreach (var s in subs ?? Enumerable.Empty<Inspect_StandardSub>())
        {
            if (string.IsNullOrWhiteSpace(s.ItemName)) continue;
            s.Id = 0; s.MainId = main.Id; s.Sort = sort++;
            _subRepo.Insert(s);
        }
        return main.Id;
    }

    public void DeleteCascade(long id)
    {
        var subIds = _subRepo.Find("[MainId]=@m", new { m = id }).Select(x => x.Id).ToArray();
        if (subIds.Length > 0) _subRepo.Delete(subIds);
        Repository.Delete(id);
    }
}

/// <summary>点检计划：模板 × 多设备 × 周期 × 班次 × 时间范围，按角色分配；保存即一次性生成时间范围内全部点检执行单（Inspect_Record）。</summary>
public class Inspect_PlanApp : BaseApp<Inspect_Plan>
{
    private readonly IRepository<Inspect_PlanDevice> _planDevRepo;
    private readonly IRepository<Inspect_PlanRole> _planRoleRepo;
    private readonly IRepository<Inspect_Record> _recordRepo;

    public Inspect_PlanApp(IUnitWork unitWork, IRepository<Inspect_Plan> repository,
        IRepository<Inspect_PlanDevice> planDevRepo, IRepository<Inspect_PlanRole> planRoleRepo,
        IRepository<Inspect_Record> recordRepo) : base(unitWork, repository)
    {
        _planDevRepo = planDevRepo;
        _planRoleRepo = planRoleRepo;
        _recordRepo = recordRepo;
    }

    public List<Inspect_PlanDevice> GetDevices(long planId)
        => _planDevRepo.Find("[PlanId]=@p", new { p = planId }, "[Id] ASC").ToList();

    public long[] GetRoleIds(long planId)
        => _planRoleRepo.Find("[PlanId]=@p", new { p = planId }).Select(x => x.RoleId).ToArray();

    /// <summary>保存计划（含设备、角色关联），并按时间范围生成执行单。
    /// 新建：生成全范围；编辑：先清除本计划下未执行的待办，再按新范围重新生成（已完成的保留）。</summary>
    public long SavePlan(Inspect_Plan m, IEnumerable<Inspect_PlanDevice>? devices, IEnumerable<long>? roleIds)
    {
        var devList = (devices ?? Enumerable.Empty<Inspect_PlanDevice>())
            .Where(d => d.FacilityId > 0)
            .GroupBy(d => d.FacilityId).Select(g => g.First()).ToList();
        var roleList = (roleIds ?? Enumerable.Empty<long>()).Where(r => r > 0).Distinct().ToList();

        var isNew = m.Id == 0;
        if (isNew)
        {
            m.CreateDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(m.PlanNo)) m.PlanNo = "IP" + DateTime.Now.ToString("yyyyMMddHHmmss");
            if (m.Status == 0) m.Status = 1;
            Repository.Insert(m);
        }
        else Repository.Update(m);

        // 重存设备关联
        var oldDev = _planDevRepo.Find("[PlanId]=@p", new { p = m.Id }).Select(x => x.Id).ToArray();
        if (oldDev.Length > 0) _planDevRepo.Delete(oldDev);
        foreach (var d in devList)
            _planDevRepo.Insert(new Inspect_PlanDevice { PlanId = m.Id, FacilityId = d.FacilityId, FacilityName = d.FacilityName });

        // 重存角色关联
        var oldRole = _planRoleRepo.Find("[PlanId]=@p", new { p = m.Id }).Select(x => x.Id).ToArray();
        if (oldRole.Length > 0) _planRoleRepo.Delete(oldRole);
        foreach (var rid in roleList)
            _planRoleRepo.Insert(new Inspect_PlanRole { PlanId = m.Id, RoleId = rid });

        // 编辑时：清除本计划下尚未执行的待办（已完成的保留），再按新范围重新生成
        if (!isNew)
        {
            var pendingIds = _recordRepo.Find("[PlanId]=@p AND [ExecTime] IS NULL", new { p = m.Id })
                .Select(x => x.Id).ToArray();
            if (pendingIds.Length > 0) _recordRepo.Delete(pendingIds);
        }
        GenerateRecords(m, devList);
        return m.Id;
    }

    /// <summary>按「设备 × 时间范围内每个到期日 × 每个班次」一次性生成待执行单（已存在相同键则跳过）。</summary>
    private int GenerateRecords(Inspect_Plan plan, List<Inspect_PlanDevice> devices)
    {
        if (devices.Count == 0) return 0;
        var start = (plan.PlanDate ?? DateTime.Now.Date).Date;
        var end = (plan.EndDate ?? start).Date;
        if (end < start) end = start;

        var cycle = string.IsNullOrWhiteSpace(plan.CycleType) ? "日" : plan.CycleType!;
        var shifts = ParseShifts(plan.Shifts);
        var dates = OccurrenceDates(start, end, cycle);
        if (dates.Count == 0) return 0;

        // 已存在执行单去重（兼容重复保存）
        var existed = _recordRepo.Find("[PlanId]=@p", new { p = plan.Id }).ToList();
        var keys = new HashSet<string>(existed.Select(r => RecKey(r.FacilityId, r.PlanDate, r.Shift)));

        var created = 0; var seq = 0;
        foreach (var dev in devices)
            foreach (var d in dates)
                foreach (var sh in shifts)
                {
                    var key = RecKey(dev.FacilityId, d, sh);
                    if (!keys.Add(key)) continue;
                    seq++; created++;
                    _recordRepo.Insert(new Inspect_Record
                    {
                        RecordNo = $"IR{DateTime.Now:yyyyMMddHHmmss}{seq:D4}",
                        PlanId = plan.Id,
                        FacilityId = dev.FacilityId,
                        FacilityName = dev.FacilityName,
                        Shift = string.IsNullOrEmpty(sh) ? null : sh,
                        PlanDate = d,
                        ExecTime = null,
                        Result = 0,
                        CreateDate = DateTime.Now
                    });
                }
        return created;
    }

    private static string RecKey(long? facilityId, DateTime? planDate, string? shift)
        => $"{facilityId}|{planDate?.ToString("yyyyMMdd")}|{shift ?? ""}";

    /// <summary>解析班次：勾选的班次列表（备注性质，同一天每班各一张）；未选则返回单元素空串（无班次维度）。</summary>
    private static List<string> ParseShifts(string? shifts)
    {
        var arr = (shifts ?? "")
            .Split(new[] { ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).Where(s => s.Length > 0).Distinct().ToList();
        return arr.Count == 0 ? new List<string> { "" } : arr;
    }

    /// <summary>按周期从起始日推算落在 [start, end] 内的到期日期。</summary>
    private static List<DateTime> OccurrenceDates(DateTime start, DateTime end, string cycle)
    {
        var list = new List<DateTime>();
        for (var i = 0; i < 20000; i++)
        {
            var d = AddCycle(start, cycle, i);
            if (d > end) break;
            list.Add(d);
        }
        return list;
    }

    private static DateTime AddCycle(DateTime baseDate, string cycle, int i)
    {
        return (cycle ?? "").Trim() switch
        {
            "日" => baseDate.AddDays(i),
            "周" => baseDate.AddDays(7 * i),
            "月" => baseDate.AddMonths(i),
            "季" or "季度" => baseDate.AddMonths(3 * i),
            "年" => baseDate.AddYears(i),
            _ => baseDate.AddDays(i)
        };
    }

    /// <summary>某月计划（日历视图用）</summary>
    public List<Inspect_Plan> GetByMonth(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        return Repository.Find("[PlanDate]>=@s AND [PlanDate]<@e", new { s = start, e = end }, "[PlanDate] ASC").ToList();
    }
}

/// <summary>点检记录执行（含逐项结果与异常处置）</summary>
public class Inspect_RecordApp : BaseApp<Inspect_Record>
{
    private readonly IRepository<Inspect_RecordSub> _subRepo;
    private readonly IRepository<Inspect_Disposal> _dispRepo;
    private readonly IRepository<Facility_ResourceDetail> _deviceRepo;

    public Inspect_RecordApp(IUnitWork unitWork, IRepository<Inspect_Record> repository,
        IRepository<Inspect_RecordSub> subRepo, IRepository<Inspect_Disposal> dispRepo,
        IRepository<Facility_ResourceDetail> deviceRepo)
        : base(unitWork, repository) { _subRepo = subRepo; _dispRepo = dispRepo; _deviceRepo = deviceRepo; }

    public List<Inspect_RecordSub> GetSubs(long recordId)
        => _subRepo.Find("[RecordId]=@r", new { r = recordId }, "[Id] ASC").ToList();

    public List<Inspect_Disposal> GetDisposals(long recordId)
        => _dispRepo.Find("[RecordId]=@r", new { r = recordId }, "[Id] ASC").ToList();

    /// <summary>提交已被他人完成时返回该值（并发防重）。</summary>
    public const long AlreadyDone = -2;

    /// <summary>提交点检单：写记录 + 逐项结果；含异常自动置 Result=1。
    /// 多人可见同一待执行单，提交时用「WHERE ExecTime IS NULL」原子抢占，已被他人完成则返回 AlreadyDone。</summary>
    public long Submit(Inspect_Record rec, IEnumerable<Inspect_RecordSub>? items)
    {
        var list = (items ?? Enumerable.Empty<Inspect_RecordSub>()).ToList();
        // 系统按控件类型+上下限自动判定合格/异常，不采信前端传入的 IsNormal
        foreach (var s in list) s.IsNormal = Judge(s);
        var result = list.Any(x => !x.IsNormal) ? 1 : 0;

        if (rec.Id == 0)
        {
            rec.Result = result;
            if (rec.ExecTime == null) rec.ExecTime = DateTime.Now;
            rec.CreateDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(rec.RecordNo)) rec.RecordNo = "IR" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Repository.Insert(rec);
        }
        else
        {
            var exist = Repository.FindSingle(rec.Id);
            if (exist == null) return -1;
            if (exist.ExecTime != null) return AlreadyDone; // 已完成，拒绝重复提交

            // 原子抢占：仅当仍为待执行（ExecTime IS NULL）才更新，避免并发重复保存
            var executor = string.IsNullOrWhiteSpace(rec.Executor) ? exist.Executor : rec.Executor;
            var affected = Repository.ExecuteSql(
                "UPDATE [Inspect_Record] SET [Executor]=@ex,[Remark]=@rk,[Result]=@rs,[ExecTime]=getdate() WHERE [Id]=@id AND [ExecTime] IS NULL",
                new { ex = executor, rk = rec.Remark, rs = result, id = rec.Id });
            if (affected == 0) return AlreadyDone; // 抢占失败，已被他人完成
            rec = Repository.FindSingle(rec.Id) ?? exist;
        }

        var existed = _subRepo.Find("[RecordId]=@r", new { r = rec.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _subRepo.Delete(existed);
        foreach (var s in list)
        {
            s.Id = 0; s.RecordId = rec.Id;
            _subRepo.Insert(s);
        }

        // 回写设备「最后点检日期」
        if (rec.FacilityId.HasValue && rec.FacilityId.Value > 0)
            _deviceRepo.ExecuteSql("UPDATE [Facility_ResourceDetail] SET [LastCheckDate]=getdate() WHERE [Id]=@id",
                new { id = rec.FacilityId.Value });

        return rec.Id;
    }

    /// <summary>自动判定单项是否合格：数值型或已配置上下限按 [MinValue,MaxValue] 区间判；其余按「是」=合格。
    /// 兼容控件类型未正确配置的历史数据：只要设置了上下限且实测值为数值，即按区间判定，避免区间内误判异常。</summary>
    private static bool Judge(Inspect_RecordSub s)
    {
        var val = (s.ResultValue ?? "").Trim();
        if (s.ControlType == 1 || s.MinValue.HasValue || s.MaxValue.HasValue)
        {
            if (decimal.TryParse(val, out var v))
            {
                if (s.MinValue.HasValue && v < s.MinValue.Value) return false;
                if (s.MaxValue.HasValue && v > s.MaxValue.Value) return false;
                return true;
            }
            if (s.ControlType == 1) return false; // 数值型未填/非法 → 异常
        }
        // 是否型：是/合格/正常/OK/√ 视为合格
        return val is "是" or "合格" or "正常" or "OK" or "ok" or "√" or "Y" or "y";
    }

    /// <summary>异常处置分流（5 类）</summary>
    public long Dispatch(Inspect_Disposal d)
    {
        d.CreateDate = DateTime.Now;
        _dispRepo.Insert(d);
        return d.Id;
    }
}
