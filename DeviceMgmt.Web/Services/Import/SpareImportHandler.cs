using System.Globalization;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.Web.Services.Import;

/// <summary>备件主数据导入（参考实现）。编码已存在则更新，否则新增。</summary>
public sealed class SpareImportHandler : IImportHandler
{
    private readonly IRepository<Basic_Spare> _repo;

    public SpareImportHandler(IRepository<Basic_Spare> repo) => _repo = repo;

    public string BizType => "Basic_Spare";
    public string DisplayName => "备件主数据";
    public string[] Headers => new[] { "编码", "名称", "规格", "类别", "单位", "安全库存", "单价", "客户" };

    public (bool ok, bool skip, string? error) ImportRow(IDictionary<string, string> row)
    {
        var code = Get(row, "编码", "Code");
        if (string.IsNullOrWhiteSpace(code)) return (false, true, "编码为空，跳过");

        var entity = _repo.FindSingle("[Code]=@c", new { c = code }) ?? new Basic_Spare();
        entity.Code = code.Trim();
        entity.Name = Get(row, "名称", "Name");
        entity.Specs = Get(row, "规格", "Specs");
        entity.Leibie = Get(row, "类别", "Leibie");
        entity.Danwei = Get(row, "单位", "Danwei");
        entity.SafeStock = ParseDecimal(Get(row, "安全库存", "SafeStock"));
        entity.Danjia = ParseDecimal(Get(row, "单价", "Danjia"));
        entity.Kehu = Get(row, "客户", "Kehu");
        if (entity.Status == null) entity.Status = 1;

        if (entity.Id == 0) _repo.Insert(entity);
        else _repo.Update(entity);
        return (true, false, null);
    }

    private static string? Get(IDictionary<string, string> row, params string[] keys)
    {
        foreach (var k in keys)
            if (row.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v)) return v.Trim();
        return null;
    }

    private static decimal? ParseDecimal(string? s)
        => decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (decimal?)null;
}
