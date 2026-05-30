namespace DeviceMgmt.Web.Services.Import;

/// <summary>
/// 批量导入处理器：每种业务实现一个，定义模板表头与逐行入库逻辑。
/// 新增业务导入只需新增一个 IImportHandler 并在 DI 注册即可。
/// </summary>
public interface IImportHandler
{
    /// <summary>业务类型编码（与前端 bizType 一致，唯一）</summary>
    string BizType { get; }

    /// <summary>中文显示名</summary>
    string DisplayName { get; }

    /// <summary>模板表头（首行）</summary>
    string[] Headers { get; }

    /// <summary>处理单行：返回 (ok, skip, error)。ok=true 成功；skip=true 跳过；否则按 error 记失败</summary>
    (bool ok, bool skip, string? error) ImportRow(IDictionary<string, string> row);
}
