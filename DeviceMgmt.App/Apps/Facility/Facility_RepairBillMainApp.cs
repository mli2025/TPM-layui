using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_RepairBillMainApp : BaseApp<Facility_RepairBillMain>
{
    private readonly IRepository<Facility_RepairBillSub> _subRepo;

    public Facility_RepairBillMainApp(
        IUnitWork unitWork,
        IRepository<Facility_RepairBillMain> repository,
        IRepository<Facility_RepairBillSub> subRepo) : base(unitWork, repository)
    {
        _subRepo = subRepo;
    }

    public RepairBillDetail? GetWithSubs(long id)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return null;
        var subs = _subRepo.Find("[MainId]=@mid", new { mid = id }, "[Sort] ASC, [Id] ASC").ToList();
        return new RepairBillDetail { Main = main, Subs = subs };
    }

    public long SaveBill(Facility_RepairBillMain main, List<Facility_RepairBillSub> subs, long currentUserId)
    {
        var now = DateTime.Now;
        // 以"行是否真实存在"判定新增/更新，避免前端误带 Id 时走更新分支、
        // 更新到不存在的行（影响 0 行、不报错）造成"提示成功但库里没有"。
        var existing = main.Id > 0 ? Repository.FindSingle(main.Id) : null;
        if (existing == null)
        {
            main.Id = 0;
            if (string.IsNullOrEmpty(main.BillNo)) main.BillNo = NextRepairBillNo();
            if (main.BillDate == null) main.BillDate = now;
            if (main.Status == null) main.Status = 0;
            main.FGC_Creator = currentUserId.ToString();
            main.FGC_CreateDate = now.ToString("yyyy/MM/dd HH:mm:ss");
            main.FGC_LastModifier = currentUserId.ToString();
            main.FGC_LastModifyDate = now.ToString("yyyy/MM/dd HH:mm:ss");
            Repository.Insert(main);
        }
        else
        {
            main.FGC_LastModifier = currentUserId.ToString();
            main.FGC_LastModifyDate = now.ToString("yyyy/MM/dd HH:mm:ss");
            Repository.Update(main);
            var oldSubIds = _subRepo.Find("[MainId]=@mid", new { mid = main.Id }).Select(x => x.Id).ToArray();
            if (oldSubIds.Length > 0) _subRepo.Delete(oldSubIds);
        }

        if (subs != null)
        {
            int sort = 1;
            foreach (var s in subs)
            {
                s.Id = 0;
                s.MainId = main.Id;
                s.Sort = sort++;
                _subRepo.Insert(s);
            }
        }
        return main.Id;
    }

    public (bool ok, string msg) Dispatch(long id, string repairStaff, long currentUserId, string? dispatchUser,
        DateTime? dispatchDate = null, DateTime? expectedFinishDate = null, string? dispatchRemark = null)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return (false, "报修单不存在");
        if ((main.Status ?? 0) >= 1) return (false, "该报修单已派工，无法重复派工");
        var now = DateTime.Now;
        main.RepairStaff = repairStaff;
        main.Dispatch = dispatchUser ?? currentUserId.ToString();
        main.DispatchDate = dispatchDate ?? now;
        // 期望完成时间无独立列，复用 ResponseDate（活字格里语义相近）
        if (expectedFinishDate.HasValue) main.ResponseDate = expectedFinishDate.Value;
        // 派工备注追加到 Remark 顶部，保留历史
        if (!string.IsNullOrWhiteSpace(dispatchRemark))
        {
            var stamp = $"[派工@{now:yyyy-MM-dd HH:mm}] {dispatchRemark}";
            main.Remark = string.IsNullOrWhiteSpace(main.Remark) ? stamp : stamp + "\n" + main.Remark;
        }
        main.Status = 1;
        main.FGC_LastModifier = currentUserId.ToString();
        main.FGC_LastModifyDate = now.ToString("yyyy/MM/dd HH:mm:ss");
        Repository.Update(main);
        return (true, "ok");
    }

    public (int success, int fail, List<string> errors) BatchDispatch(long[] ids, string repairStaff, long currentUserId,
        string? dispatchUser, DateTime? dispatchDate, DateTime? expectedFinishDate, string? dispatchRemark)
    {
        int ok = 0, fail = 0;
        var errors = new List<string>();
        foreach (var id in ids ?? Array.Empty<long>())
        {
            var (success, msg) = Dispatch(id, repairStaff, currentUserId, dispatchUser, dispatchDate, expectedFinishDate, dispatchRemark);
            if (success) ok++;
            else { fail++; errors.Add($"#{id}: {msg}"); }
        }
        return (ok, fail, errors);
    }

    /// <summary>
    /// Pending(未关闭) 维修任务在每个维修人员上的计数（按 RepairStaff 编码分组）
    /// </summary>
    public Dictionary<string, int> GetPendingCountByStaff()
    {
        var rows = Repository.Query<StaffCountRow>(
            "SELECT RepairStaff AS Staff, COUNT(*) AS Cnt FROM [Facility_RepairBillMain] " +
            "WHERE [Status] IN (1,2) AND [RepairStaff] IS NOT NULL AND LTRIM(RTRIM([RepairStaff]))<>'' " +
            "GROUP BY RepairStaff").ToList();
        var dict = new Dictionary<string, int>();
        foreach (var r in rows)
        {
            if (!string.IsNullOrWhiteSpace(r.Staff)) dict[r.Staff!] = r.Cnt;
        }
        return dict;
    }

    private class StaffCountRow
    {
        public string? Staff { get; set; }
        public int Cnt { get; set; }
    }

    public (bool ok, string msg) DeleteWithGuard(long id)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return (false, "报修单不存在");
        if ((main.Status ?? 0) >= 1) return (false, "已派工的报修单不允许删除");
        var subIds = _subRepo.Find("[MainId]=@mid", new { mid = id }).Select(x => x.Id).ToArray();
        if (subIds.Length > 0) _subRepo.Delete(subIds);
        Repository.Delete(id);
        return (true, "ok");
    }

    private string NextRepairBillNo()
    {
        var last = Repository.Query<string>(
            "SELECT TOP 1 [BillNo] FROM [Facility_RepairBillMain] WHERE [BillNo] LIKE 'EMR%' ORDER BY [Id] DESC")
            .FirstOrDefault();
        long n = 0;
        if (!string.IsNullOrEmpty(last) && last.Length > 3 && long.TryParse(last.Substring(3), out var parsed)) n = parsed;
        return $"EMR{(n + 1):D9}";
    }
}

public class RepairBillDetail
{
    public Facility_RepairBillMain Main { get; set; } = new();
    public List<Facility_RepairBillSub> Subs { get; set; } = new();
}
