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
        if (main.Id == 0)
        {
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

    public (bool ok, string msg) Dispatch(long id, string repairStaff, long currentUserId, string? dispatchUser)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return (false, "报修单不存在");
        if ((main.Status ?? 0) >= 1) return (false, "该报修单已派工，无法重复派工");
        main.RepairStaff = repairStaff;
        main.Dispatch = dispatchUser ?? currentUserId.ToString();
        main.DispatchDate = DateTime.Now;
        main.Status = 1;
        main.FGC_LastModifier = currentUserId.ToString();
        main.FGC_LastModifyDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        Repository.Update(main);
        return (true, "ok");
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
