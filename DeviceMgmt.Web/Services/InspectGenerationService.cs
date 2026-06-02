using DeviceMgmt.App.Apps.Inspect;

namespace DeviceMgmt.Web.Services;

/// <summary>
/// 点检执行单滚动生成后台任务（方案 C）。
/// 计划只保存「设备 × 周期 × 班次」的循环规则与角色分配，本服务每隔一段时间
/// 调用 <see cref="Inspect_PlanApp.GenerateDueForAllPlans"/> 按规则补齐「当期与回溯窗口内到期」的待执行单。
/// 生成逻辑幂等（已存在则跳过），因此可安全地周期性重复执行。
/// </summary>
public class InspectGenerationService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InspectGenerationService> _logger;

    public InspectGenerationService(IServiceProvider serviceProvider, ILogger<InspectGenerationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 启动后稍作延迟，待应用与数据库就绪
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var planApp = scope.ServiceProvider.GetRequiredService<Inspect_PlanApp>();
                var created = planApp.GenerateDueForAllPlans();
                if (created > 0)
                    _logger.LogInformation("Inspection rolling generation created {Count} execution records", created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inspection rolling generation failed");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (TaskCanceledException) { break; }
        }
    }
}
