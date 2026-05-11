using DeviceMgmt.App.Apps.Basic;
using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Apps.Mold;
using DeviceMgmt.App.Interface;
using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Interface;
using Infrastructure.Cache;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheContext, MemoryCacheContext>();

builder.Services.AddScoped<IUnitWork, UnitWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<EmployeeApp>();
builder.Services.AddScoped<ModuleApp>();
builder.Services.AddScoped<UserApp>();
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
