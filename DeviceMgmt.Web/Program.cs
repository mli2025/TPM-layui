using DeviceMgmt.App.Apps.Basic;
using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Apps.Mold;
using DeviceMgmt.App.Interface;
using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Interface;
using Infrastructure.Cache;
using Microsoft.Data.SqlClient;
using DeviceMgmt.Web.Serialization;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 若设置 DB_SERVER，则用 DB_* 环境变量组装连接串并覆盖 Default（Encrypt=false 时须用 False，不能用 Optional，否则仍会 TLS 握手）
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
        // 雪花 Id 超过 JS 安全整数，必须序列化为字符串，否则编辑/查询会 not found、部门树错乱
        options.SerializerSettings.Converters.Add(new LongAsStringJsonConverter());
        options.SerializerSettings.Converters.Add(new NullableLongAsStringJsonConverter());
    });

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheContext, MemoryCacheContext>();

builder.Services.AddScoped<IUnitWork, UnitWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<EmployeeApp>();
builder.Services.AddScoped<ModuleApp>();
builder.Services.AddScoped<UserApp>();
builder.Services.AddScoped<RoleApp>();
builder.Services.AddScoped<DeptApp>();
builder.Services.AddScoped<IAuth, AuthApp>();

builder.Services.AddScoped<Basic_EquipmentResourcesApp>();
builder.Services.AddScoped<Facility_ResourceDetailApp>();
builder.Services.AddScoped<Facility_ResourceDetailStatusApp>();
builder.Services.AddScoped<Facility_ResourceDetailGatherViewApp>();
builder.Services.AddScoped<Facility_BillMainViewApp>();
builder.Services.AddScoped<Facility_BillMainApp>();
builder.Services.AddScoped<Facility_BillSubApp>();
builder.Services.AddScoped<Facility_TheTemplateMainApp>();
builder.Services.AddScoped<Facility_TheTemplateSubApp>();
builder.Services.AddScoped<Facility_OutsourcingMaintenanceApp>();
builder.Services.AddScoped<Facility_RepairBillMainApp>();
builder.Services.AddScoped<Facility_RepairBillSubApp>();
builder.Services.AddScoped<Facility_RepairEmpApp>();
builder.Services.AddScoped<Facility_OutsourcingRepairApp>();
builder.Services.AddScoped<Facility_RepairHistoryApp>();
builder.Services.AddScoped<Facility_DATAApp>();
builder.Services.AddScoped<Facility_DATA_HistoryApp>();
builder.Services.AddScoped<Facility_ItemApp>();
builder.Services.AddScoped<Facility_ProcessApp>();
builder.Services.AddScoped<DianJianDeptApp>();
builder.Services.AddScoped<FacilityDDApp>();
builder.Services.AddScoped<Facility_Status_HistoryApp>();
builder.Services.AddScoped<Facility_OutQCApp>();
builder.Services.AddScoped<Facility_ResourceDetailGatherApp>();
builder.Services.AddScoped<OEE_RateApp>();
builder.Services.AddScoped<OEE_ScrapApp>();
builder.Services.AddScoped<OEE_StopTimesApp>();
builder.Services.AddScoped<OEE_TotalTimesApp>();
builder.Services.AddScoped<Rpt_OEEApp>();
builder.Services.AddScoped<Production_BarcodeSMTApp>();

builder.Services.AddScoped<Basic_SpareApp>();
builder.Services.AddScoped<Spare_InvoiceMainApp>();
builder.Services.AddScoped<Spare_InvoiceSubApp>();
builder.Services.AddScoped<Spare_InvoiceDataApp>();
builder.Services.AddScoped<Spare_NowQuanApp>();
builder.Services.AddScoped<WMS_BarCodeInfo_SparesApp>();
builder.Services.AddScoped<WMS_BarCodeInfo_Spares_SubApp>();
builder.Services.AddScoped<WuzikuApp>();

builder.Services.AddScoped<Basic_MoldApp>();
builder.Services.AddScoped<Basic_MoldMaterialApp>();
builder.Services.AddScoped<Mold_BillMainApp>();
builder.Services.AddScoped<Mold_BillSubApp>();
builder.Services.AddScoped<Mold_InOutApp>();
builder.Services.AddScoped<Mold_OnOffApp>();
builder.Services.AddScoped<Mold_OnOffSubApp>();
builder.Services.AddScoped<Mold_RepairBillApp>();
builder.Services.AddScoped<Mold_ItemApp>();
builder.Services.AddScoped<Mold_TheTemplateMainApp>();
builder.Services.AddScoped<Mold_TheTemplateSubApp>();
builder.Services.AddScoped<Mold_DayAlarmApp>();
builder.Services.AddScoped<Mold_QtyAlarmApp>();
builder.Services.AddScoped<Mold_BillMainViewApp>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// 若存在 DB_SERVER，则用离散环境变量组装 SQL 连接串（覆盖 appsettings）。
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

