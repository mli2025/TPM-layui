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

                var main = new Facility_BillMain
                {
                    BillNo = $"TPM{lastBillNo:D9}",
                    BillDate = billDate,
                    BillType = "MAINTENANCE",
                    BeginDate = billDate,
                    EndDate = billDate,
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
