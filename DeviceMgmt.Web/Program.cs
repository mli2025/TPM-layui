using DeviceMgmt.App.Apps.Basic;
using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Apps.Special;
using DeviceMgmt.App.Apps.Safety;
using DeviceMgmt.App.Apps.Meter;
using DeviceMgmt.App.Apps.Energy;
using DeviceMgmt.App.Apps.Workflow;
using DeviceMgmt.App.Interface;
using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Interface;
using Infrastructure.Cache;
using Microsoft.Data.SqlClient;
using DeviceMgmt.Web.Serialization;
using DeviceMgmt.Web.Services;
using DeviceMgmt.Web.Middleware;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;

// ------------------------------------------------------------------
// Serilog 必须最早装配，确保启动期日志也能输出到控制台和滚动文件
// ------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "arbore-tpm")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(AppContext.BaseDirectory, "logs", "tpm-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("arbore TPM starting up");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, sp, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(sp)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "arbore-tpm"));

    var discreteConn = BuildConnectionStringFromDbEnv();
    if (!string.IsNullOrEmpty(discreteConn))
    {
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = discreteConn
        });
    }
    else if (IsRunningInContainer()
             && !string.Equals(Environment.GetEnvironmentVariable("SKIP_SQL_ENCRYPT_PATCH"), "true", StringComparison.OrdinalIgnoreCase))
    {
        var patched = TryPatchSqlEncryptOffForLinux(builder.Configuration["ConnectionStrings:Default"]);
        if (!string.IsNullOrEmpty(patched))
        {
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = patched
            });
        }
    }

    builder.Services.AddControllersWithViews()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            // 即便所有表已切到 IDENTITY，将 long 序列化为字符串依然作为防御性约定保留
            options.SerializerSettings.Converters.Add(new LongAsStringJsonConverter());
            options.SerializerSettings.Converters.Add(new NullableLongAsStringJsonConverter());
        });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheContext, MemoryCacheContext>();

    builder.Services.AddScoped<IUnitWork, UnitWork>();
    // 字段级审计上下文（全局自动审计：URS 301-306）
    builder.Services.AddScoped<IAuditContext, DeviceMgmt.Web.Services.HttpAuditContext>();
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    // System
    builder.Services.AddScoped<EmployeeApp>();
    builder.Services.AddScoped<ModuleApp>();
    builder.Services.AddScoped<UserApp>();
    builder.Services.AddScoped<RoleApp>();
    builder.Services.AddScoped<DeptApp>();
    builder.Services.AddScoped<UserGroupApp>();
    builder.Services.AddScoped<OperationLogApp>();
    builder.Services.AddScoped<NotifyApp>();
    builder.Services.AddScoped<LoginLogApp>();
    builder.Services.AddScoped<AccountLockApp>();
    builder.Services.AddScoped<IAuth, AuthApp>();

    // 特种设备 / 安全附件 / 计量器具 / 能源（URS 全新模块）
    builder.Services.AddScoped<Special_EquipmentApp>();
    builder.Services.AddScoped<Special_InspectPlanApp>();
    builder.Services.AddScoped<Special_InspectRecordApp>();
    builder.Services.AddScoped<Safety_AccessoryApp>();
    builder.Services.AddScoped<Safety_CheckPlanApp>();
    builder.Services.AddScoped<Safety_CheckRecordApp>();
    builder.Services.AddScoped<MeterApp>();
    builder.Services.AddScoped<Meter_CalibPlanApp>();
    builder.Services.AddScoped<Meter_CalibRecordApp>();
    builder.Services.AddScoped<Meter_SendOutApp>();
    builder.Services.AddScoped<Meter_InOutApp>();
    builder.Services.AddScoped<Energy_PointApp>();
    builder.Services.AddScoped<Energy_SummaryApp>();
    builder.Services.AddScoped<Energy_AlarmRuleApp>();
    builder.Services.AddScoped<Energy_AlarmRecordApp>();
    builder.Services.AddScoped<WorkflowApp>();
    builder.Services.AddSingleton<SettingService>();
    builder.Services.AddScoped<AttachmentService>();
    builder.Services.AddScoped<VersionService>();
    builder.Services.AddScoped<OperationLogService>();
    builder.Services.AddScoped<AuditService>();
    builder.Services.AddScoped<AuditTrailApp>();
    builder.Services.AddScoped<ImportLogApp>();
    builder.Services.AddScoped<ReportApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Maint.Maint_StandardApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Maint.Maint_DelayApplyApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Maint.Maint_QualificationApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Inspect.Inspect_StandardApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Inspect.Inspect_PlanApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Inspect.Inspect_RecordApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Archive.Facility_AcceptanceApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Archive.Facility_StockCheckApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Archive.Facility_AssetCardApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Archive.Facility_CertApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Archive.Facility_LabelApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Archive.Facility_LubeStandardApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Archive.Facility_LubeRecordApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Repair.Facility_RepairTemplateApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Repair.Facility_RepairCostApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Repair.Facility_AlarmRuleApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Repair.Facility_AlarmRecordApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Repair.RepairStatApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Spare.Spare_AlarmConfigApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Spare.Spare_LifeCycleApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Spare.Spare_StockCheckApp>();
    builder.Services.AddScoped<DeviceMgmt.Web.Services.Import.ImportService>();
    builder.Services.AddScoped<DeviceMgmt.Web.Services.Import.IImportHandler, DeviceMgmt.Web.Services.Import.SpareImportHandler>();
    builder.Services.AddScoped<DeviceMgmt.Web.Services.Import.IImportHandler, DeviceMgmt.Web.Services.Import.FacilityImportHandler>();

    // Facility
    builder.Services.AddScoped<Facility_ResourceDetailApp>();
    builder.Services.AddScoped<Facility_BillMainApp>();
    builder.Services.AddScoped<Facility_BillSubApp>();
    builder.Services.AddScoped<Facility_TheTemplateMainApp>();
    builder.Services.AddScoped<Facility_TheTemplateSubApp>();
    builder.Services.AddScoped<Facility_RepairBillMainApp>();
    builder.Services.AddScoped<Facility_RepairBillSubApp>();
    builder.Services.AddScoped<Facility_ItemApp>();

    // Spare
    builder.Services.AddScoped<Basic_SpareApp>();
    builder.Services.AddScoped<Spare_InvoiceMainApp>();
    builder.Services.AddScoped<Spare_InvoiceSubApp>();
    builder.Services.AddScoped<Spare_NowQuanApp>();
    builder.Services.AddScoped<DeviceMgmt.App.Apps.Spare.Basic_WarehouseApp>();

    var app = builder.Build();

    // 启动时一次性加载所有全局设置并放入缓存
    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<SettingService>().Reload();
        Log.Information("Global settings loaded into memory cache");
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }

    app.UseSerilogRequestLogging(opts =>
    {
        opts.GetLevel = (httpCtx, elapsed, ex) =>
        {
            if (ex != null || httpCtx.Response.StatusCode >= 500) return LogEventLevel.Error;
            if (httpCtx.Response.StatusCode >= 400) return LogEventLevel.Warning;
            return LogEventLevel.Information;
        };
    });

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "arbore TPM terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static string? BuildConnectionStringFromDbEnv()
{
    var server = Environment.GetEnvironmentVariable("DB_SERVER")?.Trim();
    if (string.IsNullOrEmpty(server)) return null;

    var port = Environment.GetEnvironmentVariable("DB_PORT")?.Trim() ?? "1433";
    var database = Environment.GetEnvironmentVariable("DB_NAME")?.Trim() ?? "TPM";
    var user = Environment.GetEnvironmentVariable("DB_USER")?.Trim() ?? "sa";
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

    string dataSource;
    if (server.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
        dataSource = server;
    else if (server.Contains(',', StringComparison.Ordinal))
        dataSource = $"tcp:{server}";
    else
        dataSource = $"tcp:{server},{port}";

    var csb = new SqlConnectionStringBuilder
    {
        DataSource = dataSource,
        InitialCatalog = database,
        UserID = user,
        Password = password,
    };

    var enc = Environment.GetEnvironmentVariable("DB_ENCRYPT")?.Trim();
    if (string.IsNullOrEmpty(enc)) enc = "False";
    if (bool.TryParse(enc, out var encBool))
        csb["Encrypt"] = encBool ? "Mandatory" : "False";
    else
    {
        enc = enc.Replace('-', '_');
        csb["Encrypt"] = enc.ToUpperInvariant() switch
        {
            "MANDATORY" or "TRUE" or "YES" or "1" => "Mandatory",
            "STRICT" => "Strict",
            "OPTIONAL" => "Optional",
            "FALSE" or "NO" or "0" or "OFF" => "False",
            _ => "False"
        };
    }

    var trust = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE")?.Trim();
    if (string.IsNullOrEmpty(trust)) trust = "true";
    csb.TrustServerCertificate = trust.Equals("true", StringComparison.OrdinalIgnoreCase) || trust == "1";

    return csb.ConnectionString;
}

static bool IsRunningInContainer() =>
    string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);

static string? TryPatchSqlEncryptOffForLinux(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString)) return null;
    try
    {
        var sb = new SqlConnectionStringBuilder(connectionString);
        sb["Encrypt"] = "False";
        sb.TrustServerCertificate = true;
        return sb.ConnectionString;
    }
    catch
    {
        return null;
    }
}
