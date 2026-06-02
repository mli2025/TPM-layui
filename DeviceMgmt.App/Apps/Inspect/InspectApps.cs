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

/// <summary>点检计划：标准 × 多设备 × 周期 × 班次的循环规则，按角色分配；由滚动后台任务逐日生成点检执行单（Inspect_Record）。</summary>
public class Inspect_PlanApp : BaseApp<Inspect_Plan>
{
    /// <summary>滚动生成的回溯天数：每次只补齐「最近 N 天内到期且尚未生成」的执行单，既能体现漏检又不至于无限膨胀。</summary>
    private const int LookbackDays = 30;

    private readonly IRepository<Inspect_PlanDevice> _planDevRepo;
    private readonly IRepository<Inspect_PlanRole> _planRoleRepo;
    private readonly IRepository<Inspect_Record> _recordRepo;
    private readonly IRepository<Inspect_Standard> _stdRepo;

    public Inspect_PlanApp(IUnitWork unitWork, IRepository<Inspect_Plan> repository,
        IRepository<Inspect_PlanDevice> planDevRepo, IRepository<Inspect_PlanRole> planRoleRepo,
        IRepository<Inspect_Record> recordRepo, IRepository<Inspect_Standard> stdRepo) : base(unitWork, repository)
    {
        _planDevRepo = planDevRepo;
        _planRoleRepo = planRoleRepo;
        _recordRepo = recordRepo;
        _stdRepo = stdRepo;
    }

    public List<Inspect_PlanDevice> GetDevices(long planId)
        => _planDevRepo.Find("[PlanId]=@p", new { p = planId }, "[Id] ASC").ToList();

    public long[] GetRoleIds(long planId)
        => _planRoleRepo.Find("[PlanId]=@p", new { p = planId }).Select(x => x.RoleId).ToArray();

    /// <summary>保存计划（含设备、角色关联），并立即补齐当期与回溯窗口内到期的执行单。</summary>
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

        // 重存设备关联（编辑时先清旧关联，不动已生成的历史执行单）
        var oldDev = _planDevRepo.Find("[PlanId]=@p", new { p = m.Id }).Select(x => x.Id).ToArray();
        if (oldDev.Length > 0) _planDevRepo.Delete(oldDev);
        foreach (var d in devList)
            _planDevRepo.Insert(new Inspect_PlanDevice { PlanId = m.Id, FacilityId = d.FacilityId, FacilityName = d.FacilityName });

        // 重存角色关联
        var oldRole = _planRoleRepo.Find("[PlanId]=@p", new { p = m.Id }).Select(x => x.Id).ToArray();
        if (oldRole.Length > 0) _planRoleRepo.Delete(oldRole);
        foreach (var rid in roleList)
            _planRoleRepo.Insert(new Inspect_PlanRole { PlanId = m.Id, RoleId = rid });

        // 保存即补齐到期执行单，便于立即测试；后续由后台任务每日滚动补齐
        if (m.Status == 1) GenerateDueForPlan(m, devList);
        return m.Id;
    }

    /// <summary>遍历所有启用计划，补齐到期执行单（供后台滚动任务调用）。返回新生成的执行单数量。</summary>
    public int GenerateDueForAllPlans()
    {
        var plans = Repository.Find("[Status]=1").ToList();
        var total = 0;
        foreach (var p in plans) total += GenerateDueForPlan(p, null);
        return total;
    }

    /// <summary>按「设备 × 到期日期 × 班次」补齐回溯窗口内尚未生成的待执行单（幂等：已存在则跳过）。</summary>
    public int GenerateDueForPlan(Inspect_Plan plan, List<Inspect_PlanDevice>? devicesArg)
    {
        var today = DateTime.Now.Date;
        var start = (plan.PlanDate ?? today).Date;
        if (start > today) return 0; // 尚未生效

        var windowStart = start;
        var lookbackStart = today.AddDays(-LookbackDays);
        if (windowStart < lookbackStart) windowStart = lookbackStart;

        var hardEnd = today;
        if (plan.EndDate.HasValue && plan.EndDate.Value.Date < hardEnd) hardEnd = plan.EndDate.Value.Date;
        if (windowStart > hardEnd) return 0;

        var devices = devicesArg ?? _planDevRepo.Find("[PlanId]=@p", new { p = plan.Id }).ToList();
        if (devices.Count == 0) return 0;

        var cycle = !string.IsNullOrWhiteSpace(plan.CycleType)
            ? plan.CycleType!
            : (_stdRepo.FindSingle(plan.StandardId)?.CycleType ?? "日");
        var shifts = ParseShifts(plan.Shifts, cycle);
        var dates = OccurrenceDates(start, windowStart, hardEnd, cycle);
        if (dates.Count == 0) return 0;

        // 一次性取窗口内已存在执行单，构建去重键集合
        var existed = _recordRepo.Find("[PlanId]=@p AND [PlanDate]>=@s AND [PlanDate]<@e",
            new { p = plan.Id, s = windowStart, e = hardEnd.AddDays(1) }).ToList();
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
                        RecordNo = $"IR{DateTime.Now:yyyyMMddHHmmss}{seq:D3}",
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

    /// <summary>解析班次：周期=班 时返回选中的班次列表（未选则给一个默认班）；其它周期返回单元素空串（无班次维度）。</summary>
    private static List<string> ParseShifts(string? shifts, string cycle)
    {
        var c = (cycle ?? "").Trim();
        if (c == "班" || c == "班次")
        {
            var arr = (shifts ?? "")
                .Split(new[] { ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => s.Length > 0).Distinct().ToList();
            if (arr.Count == 0) arr.Add("当班");
            return arr;
        }
        return new List<string> { "" };
    }

    /// <summary>按周期从起始日推算落在 [windowStart, end] 内的到期日期。</summary>
    private static List<DateTime> OccurrenceDates(DateTime start, DateTime windowStart, DateTime end, string cycle)
    {
        var list = new List<DateTime>();
        for (var i = 0; i < 20000; i++)
        {
            var d = AddCycle(start, cycle, i);
            if (d > end) break;
            if (d >= windowStart) list.Add(d);
        }
        return list;
    }

    private static DateTime AddCycle(DateTime baseDate, string cycle, int i)
    {
        return (cycle ?? "").Trim() switch
        {
            "班" or "班次" => baseDate.AddDays(i),
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

    public Inspect_RecordApp(IUnitWork unitWork, IRepository<Inspect_Record> repository,
        IRepository<Inspect_RecordSub> subRepo, IRepository<Inspect_Disposal> dispRepo)
        : base(unitWork, repository) { _subRepo = subRepo; _dispRepo = dispRepo; }

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
        return rec.Id;
    }

    /// <summary>异常处置分流（5 类）</summary>
    public long Dispatch(Inspect_Disposal d)
    {
        d.CreateDate = DateTime.Now;
        _dispRepo.Insert(d);
        return d.Id;
    }
}
