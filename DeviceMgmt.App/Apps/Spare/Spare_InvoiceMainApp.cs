using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class Spare_InvoiceMainApp : BaseApp<Spare_InvoiceMain>
{
    private readonly IRepository<Spare_InvoiceSub> _subRepo;
    private readonly IRepository<Spare_NowQuan> _stockRepo;

    public Spare_InvoiceMainApp(
        IUnitWork unitWork,
        IRepository<Spare_InvoiceMain> repository,
        IRepository<Spare_InvoiceSub> subRepo,
        IRepository<Spare_NowQuan> stockRepo) : base(unitWork, repository)
    {
        _subRepo = subRepo;
        _stockRepo = stockRepo;
    }

    public SpareInvoiceDetail? GetWithSubs(long id)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return null;
        var subs = _subRepo.Find("[MainId]=@m", new { m = id }, "[RowNum] ASC, [Id] ASC").ToList();
        return new SpareInvoiceDetail { Main = main, Subs = subs };
    }

    public long SaveInvoice(Spare_InvoiceMain main, List<Spare_InvoiceSub> subs, long uid)
    {
        var now = DateTime.Now;
        if (main.Id == 0)
        {
            if (string.IsNullOrEmpty(main.BillNo)) main.BillNo = NextBillNo(main.BillType ?? 1);
            if (main.BillDate == null) main.BillDate = now;
            if (main.Status == null) main.Status = 0;
            main.FGC_Creator = uid.ToString();
            main.FGC_CreateDate = now.ToString("yyyy/MM/dd HH:mm:ss");
            Repository.Insert(main);
        }
        else
        {
            main.FGC_LastModifier = uid.ToString();
            main.FGC_LastModifyDate = now.ToString("yyyy/MM/dd HH:mm:ss");
            Repository.Update(main);
            var oldIds = _subRepo.Find("[MainId]=@m", new { m = main.Id }).Select(x => x.Id).ToArray();
            if (oldIds.Length > 0) _subRepo.Delete(oldIds);
        }
        int row = 1;
        foreach (var s in subs ?? new List<Spare_InvoiceSub>())
        {
            s.Id = 0;
            s.MainId = main.Id;
            s.RowNum = row++;
            _subRepo.Insert(s);
        }
        return main.Id;
    }

    public (bool ok, string msg) Audit(long id, long uid, string? auditor)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return (false, "单据不存在");
        if ((main.Status ?? 0) >= 1) return (false, "已审核，无法重复审核");
        var subs = _subRepo.Find("[MainId]=@m", new { m = id }).ToList();
        if (subs.Count == 0) return (false, "明细为空，无法审核");

        var sign = main.BillType == 1 ? 1 : main.BillType == 2 ? -1 : 0;
        if (sign == 0) return (false, "未知单据类型，BillType=" + main.BillType);

        foreach (var s in subs)
        {
            if (!s.SpareId.HasValue || !s.Qty.HasValue) continue;
            var qty = s.Qty.Value * sign;
            var stock = _stockRepo.FindSingle("[SpareId]=@s AND [WarehouseId]=@w",
                new { s = s.SpareId.Value, w = main.WHID ?? 0 });
            if (stock == null)
            {
                _stockRepo.Insert(new Spare_NowQuan
                {
                    SpareId = s.SpareId.Value,
                    WarehouseId = main.WHID ?? 0,
                    Qty = qty
                });
            }
            else
            {
                stock.Qty = (stock.Qty ?? 0) + qty;
                _stockRepo.Update(stock);
            }
        }

        main.Status = 1;
        main.Checker = auditor;
        main.CheckDate = DateTime.Now;
        main.FGC_LastModifier = uid.ToString();
        main.FGC_LastModifyDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        Repository.Update(main);
        return (true, "ok");
    }

    public (bool ok, string msg) DeleteWithGuard(long id)
    {
        var main = Repository.FindSingle(id);
        if (main == null) return (false, "单据不存在");
        if ((main.Status ?? 0) >= 1) return (false, "已审核单据不允许删除");
        var subIds = _subRepo.Find("[MainId]=@m", new { m = id }).Select(x => x.Id).ToArray();
        if (subIds.Length > 0) _subRepo.Delete(subIds);
        Repository.Delete(id);
        return (true, "ok");
    }

    private string NextBillNo(long billType)
    {
        var prefix = billType == 1 ? "IN" : billType == 2 ? "OUT" : "BIL";
        var last = Repository.Query<string>(
            $"SELECT TOP 1 [BillNo] FROM [Spare_InvoiceMain] WHERE [BillNo] LIKE '{prefix}%' ORDER BY [Id] DESC")
            .FirstOrDefault();
        long n = 0;
        if (!string.IsNullOrEmpty(last) && last.Length > prefix.Length
            && long.TryParse(last.Substring(prefix.Length), out var parsed)) n = parsed;
        return $"{prefix}{(n + 1):D8}";
    }
}

public class SpareInvoiceDetail
{
    public Spare_InvoiceMain Main { get; set; } = new();
    public List<Spare_InvoiceSub> Subs { get; set; } = new();
}
