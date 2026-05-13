using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.Web.Services;

public sealed class OperationLogService
{
    private readonly IRepository<Sys_OperationLog> _repo;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<OperationLogService> _logger;

    public OperationLogService(IRepository<Sys_OperationLog> repo, IHttpContextAccessor http, ILogger<OperationLogService> logger)
    {
        _repo = repo;
        _http = http;
        _logger = logger;
    }

    public void Write(string action, string? module = null, string? description = null,
        long? userId = null, string? userAccount = null,
        string? targetType = null, string? targetId = null,
        bool success = true, string? errorMessage = null, int durationMs = 0)
    {
        try
        {
            var ctx = _http.HttpContext;
            var ip = ctx?.Connection.RemoteIpAddress?.ToString();
            var ua = ctx?.Request.Headers.UserAgent.ToString();
            if (ua != null && ua.Length > 300) ua = ua[..300];

            var log = new Sys_OperationLog
            {
                Action = action,
                Module = module,
                Description = description,
                UserId = userId,
                UserAccount = userAccount,
                TargetType = targetType,
                TargetId = targetId,
                IpAddress = ip,
                UserAgent = ua,
                Success = success,
                ErrorMessage = errorMessage,
                DurationMs = durationMs,
                CreateDate = DateTime.Now
            };
            _repo.Insert(log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OperationLog write failed: {Action}", action);
        }
    }
}
