using System.Text;
using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Common;

namespace DeviceMgmt.Web.Services.Import;

public sealed class ImportResult
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Fail { get; set; }
    public int Skip { get; set; }
    public List<string> Errors { get; } = new();
}

/// <summary>
/// 通用批量导入服务：模板下载 + 解析(Excel/CSV) + 逐行校验入库 + 写导入日志。
/// 通过 IImportHandler 扩展支持任意业务。
/// </summary>
public sealed class ImportService
{
    private readonly Dictionary<string, IImportHandler> _handlers;
    private readonly IRepository<Sys_ImportLog> _logRepo;
    private readonly ILogger<ImportService> _logger;

    public ImportService(IEnumerable<IImportHandler> handlers, IRepository<Sys_ImportLog> logRepo, ILogger<ImportService> logger)
    {
        _handlers = handlers.ToDictionary(h => h.BizType, StringComparer.OrdinalIgnoreCase);
        _logRepo = logRepo;
        _logger = logger;
    }

    public IEnumerable<(string BizType, string Name)> ListBiz()
        => _handlers.Values.Select(h => (h.BizType, h.DisplayName));

    public IImportHandler? GetHandler(string bizType)
        => _handlers.TryGetValue(bizType, out var h) ? h : null;

    public byte[]? BuildTemplate(string bizType)
    {
        var h = GetHandler(bizType);
        return h == null ? null : NPOIHelper.BuildTemplate(h.Headers, h.DisplayName);
    }

    public ImportResult Import(string bizType, Stream stream, string fileName, long? operatorId)
    {
        var result = new ImportResult();
        var handler = GetHandler(bizType);
        if (handler == null) { result.Errors.Add("不支持的导入类型: " + bizType); return result; }

        List<Dictionary<string, string>> rows;
        try { (_, rows) = NPOIHelper.ReadRows(stream, fileName); }
        catch (Exception ex) { result.Errors.Add("文件解析失败: " + ex.Message); return result; }

        result.Total = rows.Count;
        var lineNo = 1;
        foreach (var row in rows)
        {
            lineNo++;
            try
            {
                var (ok, skip, error) = handler.ImportRow(row);
                if (ok) result.Success++;
                else if (skip) { result.Skip++; if (!string.IsNullOrEmpty(error)) result.Errors.Add($"第{lineNo}行: {error}"); }
                else { result.Fail++; result.Errors.Add($"第{lineNo}行: {error}"); }
            }
            catch (Exception ex)
            {
                result.Fail++;
                var msg = FacilityResourceDetailSaveHelper.ToFriendlyMessage(ex);
                result.Errors.Add($"第{lineNo}行: {msg}");
            }
        }

        try
        {
            var detail = string.Join(Environment.NewLine, result.Errors.Take(200));
            if (detail.Length > 4000) detail = detail[..4000];
            _logRepo.Insert(new Sys_ImportLog
            {
                BizType = bizType,
                FileName = fileName,
                TotalCount = result.Total,
                SuccessCount = result.Success,
                FailCount = result.Fail,
                SkipCount = result.Skip,
                ErrorDetail = detail,
                OperatorId = operatorId,
                CreateDate = DateTime.Now
            });
        }
        catch (Exception ex) { _logger.LogWarning(ex, "ImportLog write failed"); }

        return result;
    }
}
