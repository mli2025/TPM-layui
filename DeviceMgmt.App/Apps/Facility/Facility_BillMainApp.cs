using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_BillMainApp : BaseApp<Facility_BillMain>
{
    private readonly IRepository<Facility_BillSub> _subRepo;
    private readonly IRepository<Facility_TheTemplateMain> _tplMainRepo;
    private readonly IRepository<Facility_TheTemplateSub> _tplSubRepo;
    private readonly IRepository<Facility_ResourceDetail> _deviceRepo;

    public Facility_BillMainApp(
        IUnitWork unitWork,
        IRepository<Facility_BillMain> repository,
        IRepository<Facility_BillSub> subRepo,
        IRepository<Facility_TheTemplateMain> tplMainRepo,
        IRepository<Facility_TheTemplateSub> tplSubRepo,
        IRepository<Facility_ResourceDetail> deviceRepo) : base(unitWork, repository)
    {
        _subRepo = subRepo;
        _tplMainRepo = tplMainRepo;
        _tplSubRepo = tplSubRepo;
        _deviceRepo = deviceRepo;
    }

    public BillDetail? GetWithSubs(long id)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return null;
        var subs = _subRepo.Find("[MainId]=@mid", new { mid = id }, "[Id] ASC").ToList();
        return new BillDetail { Main = main, Subs = subs };
    }

    public List<DeviceTemplateStatus> CheckTemplates(long[] deviceIds, string cycle)
    {
        if (deviceIds == null || deviceIds.Length == 0) return new();
        var cycleUpper = (cycle ?? string.Empty).Trim().ToUpperInvariant();
        var devices = _deviceRepo.Find("[Id] IN @ids", new { ids = deviceIds }).ToList();
        var list = new List<DeviceTemplateStatus>();
        foreach (var d in devices)
        {
            long? tempId = cycleUpper switch
            {
                "YEAR"    => d.YearTempId,
                "QUARTER" => d.SeasonTempId,
                "MONTH"   => d.MonthTempId,
                "WEEK"    => d.WeekTempId,
                _ => null
            };
            list.Add(new DeviceTemplateStatus
            {
                DeviceId = d.Id,
                FacilityCode = d.FacilityCode,
                FacilityName = d.FacilityName,
                Cycle = cycleUpper,
                TempId = tempId,
                HasTemplate = tempId.HasValue && tempId.Value > 0
            });
        }
        return list;
    }

    public BatchGenerateResult BatchGenerate(long[] deviceIds, string cycle, int count, DateTime startDate, long currentUserId)
    {
        var result = new BatchGenerateResult();
        if (deviceIds == null || deviceIds.Length == 0)
        {
            result.Message = "未选择设备";
            return result;
        }
        if (count <= 0) count = 1;

        var cycleUpper = (cycle ?? string.Empty).Trim().ToUpperInvariant();
        var statuses = CheckTemplates(deviceIds, cycleUpper);
        var missing = statuses.Where(s => !s.HasTemplate).ToList();
        if (missing.Count > 0)
        {
            result.Message = $"有 {missing.Count} 台设备未配置 {cycleUpper} 模板，无法生成";
            result.MissingDevices = missing;
            return result;
        }

        var tplIds = statuses.Where(s => s.TempId.HasValue).Select(s => s.TempId!.Value).Distinct().ToArray();
        var tplMains = tplIds.Length == 0 ? new List<Facility_TheTemplateMain>() :
            _tplMainRepo.Find("[Id] IN @ids", new { ids = tplIds }).ToList();
        var tplSubs = tplIds.Length == 0 ? new List<Facility_TheTemplateSub>() :
            _tplSubRepo.Find("[MainId] IN @ids", new { ids = tplIds }).ToList();
        var tplMainDic = tplMains.ToDictionary(x => x.Id);
        var tplSubLookup = tplSubs.GroupBy(x => x.MainId).ToDictionary(g => g.Key, g => g.ToList());

        var lastBillNo = ResolveLastBillNoSeq();
        var generated = 0;

        foreach (var s in statuses)
        {
            if (!s.TempId.HasValue) continue;
            var tempId = s.TempId.Value;
            tplMainDic.TryGetValue(tempId, out var tplMain);
            tplSubLookup.TryGetValue(tempId, out var tplSubList);

            for (var i = 0; i < count; i++)
            {
                lastBillNo++;
                var billDate = AddCycle(startDate, cycleUpper, i);
                var (begin, end) = ResolveBillWindow(billDate, cycleUpper);

                var main = new Facility_BillMain
                {
                    BillNo = $"TPM{lastBillNo:D9}",
                    BillDate = billDate,
                    BillType = "MAINTENANCE",
                    BeginDate = begin,
                    EndDate = end,
                    FacilityID = s.DeviceId,
                    TempID = tempId,
                    MaintainType = cycleUpper,
                    Status = 0,
                    Maintenance_level = tplMain?.MaintenanceType == "YEAR" ? 4
                                       : tplMain?.MaintenanceType == "QUARTER" ? 3
                                       : tplMain?.MaintenanceType == "MONTH" ? 2
                                       : 1,
                    Amount = 0,
                    CreateUserId = currentUserId,
                    CreateDate = DateTime.Now,
                    LastUpdateUserId = currentUserId,
                    LastUpdateDate = DateTime.Now,
                    FGC_Creator = currentUserId.ToString(),
                    FGC_CreateDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    FGC_LastModifier = currentUserId.ToString(),
                    FGC_LastModifyDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                };
                var mainId = Repository.Insert(main);

                if (tplSubList != null)
                {
                    foreach (var ts in tplSubList)
                    {
                        var sub = new Facility_BillSub
                        {
                            MainId = mainId,
                            Project = ts.HContent ?? string.Empty,
                            UpkeepMethod = ts.HMethods ?? string.Empty,
                            CheckMethod = ts.HStandard ?? string.Empty,
                            ControlType = ts.ControlType ?? 0,
                            MaxValue = ts.MaxValue,
                            MinValue = ts.MinValue,
                            StdMaxValue = ts.StdMaxValue,
                            StdMinValue = ts.StdMinValue,
                            Remark = ts.HRemark,
                            WXFlage = 0
                        };
                        _subRepo.Insert(sub);
                    }
                }

                generated++;
            }
        }

        result.Success = true;
        result.GeneratedCount = generated;
        result.Message = $"已生成 {generated} 张保养单";
        return result;
    }

    public (bool ok, string msg) UpdateBillWithGuard(Facility_BillMain model)
    {
        var existed = Repository.FindSingle(model.Id);
        if (existed == null) return (false, "保养单不存在");
        if ((existed.Status ?? 0) > 0) return (false, "已派工的保养单不允许修改");
        model.LastUpdateDate = DateTime.Now;
        Repository.Update(model);
        return (true, "ok");
    }

    public (bool ok, string msg) DeleteBillWithGuard(long id)
    {
        var existed = Repository.FindSingle(id);
        if (existed == null) return (false, "保养单不存在");
        if ((existed.Status ?? 0) > 0) return (false, "已派工的保养单不允许删除");
        var subs = _subRepo.Find("[MainId]=@mid", new { mid = id }).Select(x => x.Id).ToArray();
        if (subs.Length > 0) _subRepo.Delete(subs);
        Repository.Delete(id);
        return (true, "ok");
    }

    private long ResolveLastBillNoSeq()
    {
        var last = Repository.Query<string>(
            "SELECT TOP 1 [BillNo] FROM [Facility_BillMain] WHERE [BillNo] LIKE 'TPM%' ORDER BY [Id] DESC")
            .FirstOrDefault();
        if (string.IsNullOrEmpty(last) || last.Length <= 3) return 0;
        var numberPart = last.Substring(3);
        return long.TryParse(numberPart, out var n) ? n : 0;
    }

    private static DateTime AddCycle(DateTime baseDate, string cycle, int i)
        => cycle switch
        {
            "YEAR"    => baseDate.AddYears(i),
            "QUARTER" => baseDate.AddMonths(i * 3),
            "MONTH"   => baseDate.AddMonths(i),
            "WEEK"    => baseDate.AddDays(i * 7),
            _ => baseDate
        };

    /// <summary>
    /// 根据保养周期与计划基准日，推导该单据的执行窗口 [BeginDate, EndDate]。
    /// 不再让两者等于 BillDate（占位用），保证看板/甘特能看到真实跨度。
    /// </summary>
    public static (DateTime Begin, DateTime End) ResolveBillWindow(DateTime billDate, string cycle)
    {
        var d = billDate.Date;
        DateTime begin, end;
        switch ((cycle ?? string.Empty).Trim().ToUpperInvariant())
        {
            case "YEAR":
                begin = new DateTime(d.Year, 1, 1);
                end   = new DateTime(d.Year, 12, 31, 23, 59, 59);
                break;
            case "QUARTER":
                {
                    var qStartMonth = ((d.Month - 1) / 3) * 3 + 1;
                    begin = new DateTime(d.Year, qStartMonth, 1);
                    end   = begin.AddMonths(3).AddSeconds(-1);
                    break;
                }
            case "MONTH":
                begin = new DateTime(d.Year, d.Month, 1);
                end   = begin.AddMonths(1).AddSeconds(-1);
                break;
            case "WEEK":
                {
                    // 以周一为起点（Monday=1）
                    var diff = ((int)d.DayOfWeek + 6) % 7;
                    begin = d.AddDays(-diff);
                    end   = begin.AddDays(7).AddSeconds(-1);
                    break;
                }
            default:
                begin = d;
                end   = d.AddDays(1).AddSeconds(-1);
                break;
        }
        return (begin, end);
    }

    /// <summary>
    /// 派工：状态 0(新建) -> 1(已派工)，允许覆盖 BeginDate/EndDate/Remark
    /// </summary>
    public (bool ok, string msg) Dispatch(long id, string repairStaff, long currentUserId,
        string? dispatchUser = null, DateTime? dispatchDate = null,
        DateTime? beginDate = null, DateTime? endDate = null, string? dispatchRemark = null)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return (false, "保养单不存在");
        if ((main.Status ?? 0) != 0) return (false, "该保养单已派工，无法重复派工");
        if (string.IsNullOrWhiteSpace(repairStaff)) return (false, "请选择被派人员");
        var now = DateTime.Now;
        main.RepairStaff = repairStaff.Trim();
        main.Dispatch = string.IsNullOrWhiteSpace(dispatchUser) ? currentUserId.ToString() : dispatchUser;
        main.DispatchDate = dispatchDate ?? now;
        main.RepairStaffDate = now;
        if (beginDate.HasValue) main.BeginDate = beginDate.Value;
        if (endDate.HasValue) main.EndDate = endDate.Value;
        if (!string.IsNullOrWhiteSpace(dispatchRemark))
        {
            var stamp = $"[派工@{now:yyyy-MM-dd HH:mm}] {dispatchRemark}";
            main.Remark = string.IsNullOrWhiteSpace(main.Remark) ? stamp : stamp + "\n" + main.Remark;
        }
        main.Status = 1;
        main.LastUpdateUserId = currentUserId;
        main.LastUpdateDate = now;
        main.FGC_LastModifier = currentUserId.ToString();
        main.FGC_LastModifyDate = now.ToString("yyyy/MM/dd HH:mm:ss");
        Repository.Update(main);
        return (true, "ok");
    }

    public (int success, int fail, List<string> errors) BatchDispatch(long[] ids, string repairStaff, long currentUserId,
        string? dispatchUser, DateTime? dispatchDate, DateTime? beginDate, DateTime? endDate, string? dispatchRemark)
    {
        int ok = 0, fail = 0;
        var errors = new List<string>();
        foreach (var id in ids ?? Array.Empty<long>())
        {
            var (success, msg) = Dispatch(id, repairStaff, currentUserId, dispatchUser, dispatchDate, beginDate, endDate, dispatchRemark);
            if (success) ok++;
            else { fail++; errors.Add($"#{id}: {msg}"); }
        }
        return (ok, fail, errors);
    }

    /// <summary>
    /// 当前活跃保养单（Status=1 已派工 / 2 保养中）按 RepairStaff 编码分组的负载计数
    /// </summary>
    public Dictionary<string, int> GetPendingCountByStaff()
    {
        var rows = Repository.Query<StaffCountRow>(
            "SELECT RepairStaff AS Staff, COUNT(*) AS Cnt FROM [Facility_BillMain] " +
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
}

public class BillDetail
{
    public Facility_BillMain Main { get; set; } = new();
    public List<Facility_BillSub> Subs { get; set; } = new();
}

public class DeviceTemplateStatus
{
    public long DeviceId { get; set; }
    public string FacilityCode { get; set; } = string.Empty;
    public string FacilityName { get; set; } = string.Empty;
    public string Cycle { get; set; } = string.Empty;
    public long? TempId { get; set; }
    public bool HasTemplate { get; set; }
}

public class BatchGenerateResult
{
    public bool Success { get; set; }
    public int GeneratedCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<DeviceTemplateStatus> MissingDevices { get; set; } = new();
}
