/* =============================================================================
   arbore TPM · Database Schema v2 (clean rebuild, IDENTITY-based)
   Target: SQL Server 2017+
   Strategy:
     - DROP and CREATE database [TPM]
     - All PK use `bigint IDENTITY(1,1)`, no snowflake
     - No FK constraints (validated at application layer to keep migrations easy)
     - Indexes on commonly filtered columns
     - Seeds: admin user, default dept, role, menus, settings, attachment table
   Run as: server-level user with CREATE DATABASE permission (sa or equivalent)
   ============================================================================= */

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ----- 0. Drop & recreate database ----- */
IF DB_ID('TPM') IS NOT NULL
BEGIN
    ALTER DATABASE [TPM] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [TPM];
END
GO

CREATE DATABASE [TPM]
COLLATE Chinese_PRC_CI_AS;
GO

ALTER DATABASE [TPM] SET RECOVERY SIMPLE;
GO

USE [TPM];
GO

/* =============================================================================
   1. System domain (Sys_*)
   ============================================================================= */

CREATE TABLE [Sys_User] (
    [Id]          bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Account]     nvarchar(60)  NOT NULL,
    [Password]    nvarchar(128) NOT NULL,
    [Name]        nvarchar(100) NULL,
    [EmployeeId]  bigint        NOT NULL DEFAULT(0),
    [DeptId]      bigint        NOT NULL DEFAULT(0),
    [Status]      int           NOT NULL DEFAULT(1),
    [CreateDate]  datetime      NOT NULL DEFAULT(getdate())
);
CREATE UNIQUE INDEX UX_Sys_User_Account ON [Sys_User]([Account]);
GO

CREATE TABLE [Sys_Role] (
    [Id]      bigint       IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]    nvarchar(60) NOT NULL,
    [Status]  int          NOT NULL DEFAULT(1)
);
CREATE UNIQUE INDEX UX_Sys_Role_Name ON [Sys_Role]([Name]);
GO

CREATE TABLE [Sys_UserRole] (
    [Id]      bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId]  bigint NOT NULL,
    [RoleId]  bigint NOT NULL
);
CREATE INDEX IX_Sys_UserRole_User ON [Sys_UserRole]([UserId]);
CREATE INDEX IX_Sys_UserRole_Role ON [Sys_UserRole]([RoleId]);
GO

CREATE TABLE [Sys_Dept] (
    [Id]         bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [DeptNumber] nvarchar(40)  NOT NULL DEFAULT(''),
    [DeptName]   nvarchar(100) NOT NULL,
    [ParentId]   bigint        NOT NULL DEFAULT(0),
    [Status]     int           NOT NULL DEFAULT(1)
);
CREATE INDEX IX_Sys_Dept_Parent ON [Sys_Dept]([ParentId]);
GO

CREATE TABLE [Sys_Module] (
    [Id]        bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]      nvarchar(100) NOT NULL,
    [Code]      nvarchar(60)  NOT NULL,
    [Url]       nvarchar(200) NULL,
    [ParentId]  bigint        NOT NULL DEFAULT(0),
    [Sort]      int           NOT NULL DEFAULT(0),
    [Status]    int           NOT NULL DEFAULT(1),
    [Icon]      nvarchar(60)  NULL
);
CREATE UNIQUE INDEX UX_Sys_Module_Code ON [Sys_Module]([Code]);
CREATE INDEX IX_Sys_Module_Parent ON [Sys_Module]([ParentId]);
GO

CREATE TABLE [Sys_ModuleButtons] (
    [Id]        bigint       IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ModuleId]  bigint       NOT NULL,
    [DomId]     nvarchar(60) NOT NULL,
    [Name]      nvarchar(60) NOT NULL
);
CREATE INDEX IX_Sys_ModuleButtons_Module ON [Sys_ModuleButtons]([ModuleId]);
GO

CREATE TABLE [Sys_RoleModule] (
    [Id]        bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RoleId]    bigint NOT NULL,
    [ModuleId]  bigint NOT NULL
);
CREATE INDEX IX_Sys_RoleModule_Role ON [Sys_RoleModule]([RoleId]);
CREATE INDEX IX_Sys_RoleModule_Module ON [Sys_RoleModule]([ModuleId]);
GO

CREATE TABLE [Sys_Setting] (
    [Id]         bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Group]      nvarchar(40)   NOT NULL,
    [Key]        nvarchar(80)   NOT NULL,
    [Value]      nvarchar(1000) NULL,
    [ValueType]  nvarchar(20)   NOT NULL DEFAULT('string'),
    [Title]      nvarchar(100)  NOT NULL,
    [Descr]      nvarchar(300)  NULL,
    [Sort]       int            NOT NULL DEFAULT(0),
    [Editable]   bit            NOT NULL DEFAULT(1),
    [UpdateDate] datetime       NOT NULL DEFAULT(getdate())
);
CREATE UNIQUE INDEX UX_Sys_Setting_Key ON [Sys_Setting]([Key]);
CREATE INDEX IX_Sys_Setting_Group ON [Sys_Setting]([Group], [Sort]);
GO

CREATE TABLE [Sys_Version] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Version]     nvarchar(32)   NOT NULL,
    [ReleaseDate] datetime       NOT NULL DEFAULT(getdate()),
    [Title]       nvarchar(200)  NOT NULL,
    [Content]     nvarchar(max)  NULL,
    [IsCurrent]   bit            NOT NULL DEFAULT(0),
    [Author]      nvarchar(60)   NULL,
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
CREATE UNIQUE INDEX UX_Sys_Version_Ver ON [Sys_Version]([Version]);
CREATE INDEX IX_Sys_Version_Current ON [Sys_Version]([IsCurrent]) WHERE [IsCurrent] = 1;
GO

CREATE TABLE [Sys_Attachment] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BusinessType]  nvarchar(40)   NOT NULL,
    [BusinessId]    bigint         NOT NULL,
    [FileName]      nvarchar(255)  NOT NULL,
    [StoredName]    nvarchar(64)   NOT NULL,
    [RelativePath]  nvarchar(500)  NOT NULL,
    [ContentType]   nvarchar(100)  NOT NULL,
    [FileSize]      bigint         NOT NULL,
    [FileExt]       nvarchar(20)   NOT NULL,
    [Category]      nvarchar(40)   NULL,
    [Sort]          int            NOT NULL DEFAULT(0),
    [UploaderId]    bigint         NULL,
    [UploaderName]  nvarchar(60)   NULL,
    [UploadDate]    datetime       NOT NULL DEFAULT(getdate()),
    [Remark]        nvarchar(300)  NULL,
    [IsDeleted]     bit            NOT NULL DEFAULT(0)
);
CREATE INDEX IX_Sys_Attachment_Biz ON [Sys_Attachment]([BusinessType], [BusinessId], [IsDeleted]);
GO

CREATE TABLE [Sys_OperationLog] (
    [Id]           bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId]       bigint         NULL,
    [UserAccount]  nvarchar(60)   NULL,
    [Action]       nvarchar(40)   NOT NULL,
    [Module]       nvarchar(60)   NULL,
    [TargetType]   nvarchar(40)   NULL,
    [TargetId]     nvarchar(40)   NULL,
    [Description]  nvarchar(500)  NULL,
    [IpAddress]    nvarchar(60)   NULL,
    [UserAgent]    nvarchar(300)  NULL,
    [Success]      bit            NOT NULL DEFAULT(1),
    [ErrorMessage] nvarchar(max)  NULL,
    [DurationMs]   int            NOT NULL DEFAULT(0),
    [CreateDate]   datetime       NOT NULL DEFAULT(getdate())
);
CREATE INDEX IX_Sys_OperationLog_User ON [Sys_OperationLog]([UserId], [CreateDate] DESC);
CREATE INDEX IX_Sys_OperationLog_Action ON [Sys_OperationLog]([Action], [CreateDate] DESC);
GO

/* =============================================================================
   2. Basic data (Basic_*)
   ============================================================================= */

CREATE TABLE [Basic_Employee] (
    [Id]             bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [EmployeeNumber] nvarchar(40)  NOT NULL,
    [Name]           nvarchar(60)  NOT NULL,
    [DeptId]         bigint        NOT NULL DEFAULT(0),
    [Status]         int           NOT NULL DEFAULT(1)
);
CREATE UNIQUE INDEX UX_Basic_Employee_Num ON [Basic_Employee]([EmployeeNumber]);
CREATE INDEX IX_Basic_Employee_Dept ON [Basic_Employee]([DeptId]);
GO

CREATE TABLE [Basic_Spare] (
    [Id]        bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Code]      nvarchar(60)   NULL,
    [Name]      nvarchar(200)  NULL,
    [Specs]     nvarchar(200)  NULL,
    [SafeStock] decimal(18,4)  NULL,
    [Remark]    int            NULL,
    [Status]    int            NULL,
    [Leibie]    nvarchar(60)   NULL,
    [Danjia]    decimal(18,4)  NULL,
    [Kehu]      nvarchar(100)  NULL,
    [Danwei]    nvarchar(20)   NULL
);
CREATE INDEX IX_Basic_Spare_Code ON [Basic_Spare]([Code]);
GO

/* =============================================================================
   3. Facility domain (Facility_*)
   ============================================================================= */

CREATE TABLE [Facility_ResourceDetail] (
    [Id]                              bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FacilityCode]                    nvarchar(60)   NOT NULL DEFAULT(''),
    [FacilityName]                    nvarchar(200)  NOT NULL DEFAULT(''),
    [FacilityType]                    nvarchar(60)   NOT NULL DEFAULT(''),
    [ResourceId]                      bigint         NOT NULL DEFAULT(0),
    [Manufacturer]                    nvarchar(200)  NOT NULL DEFAULT(''),
    [Supplier]                        nvarchar(200)  NOT NULL DEFAULT(''),
    [ManufacturerDate]                datetime       NOT NULL DEFAULT(getdate()),
    [ManufactureCountry]              nvarchar(60)   NOT NULL DEFAULT(''),
    [Model]                           nvarchar(100)  NOT NULL DEFAULT(''),
    [ExpireDate]                      datetime       NULL,
    [PurchasePrice]                   decimal(18,2)  NOT NULL DEFAULT(0),
    [PurchaseDate]                    datetime       NULL,
    [SerialNumber]                    nvarchar(100)  NOT NULL DEFAULT(''),
    [EquipmentManual]                 nvarchar(500)  NULL,
    [EquipmentDrawing]                nvarchar(500)  NULL,
    [Location]                        nvarchar(200)  NOT NULL DEFAULT(''),
    [DeptId]                          bigint         NOT NULL DEFAULT(0),
    [AssetNumber]                     nvarchar(60)   NULL,
    [Voltage]                         int            NULL,
    [Size]                            nvarchar(60)   NULL,
    [Weight]                          int            NULL,
    [The5STemplateMainId]             bigint         NULL,
    [TheTemplateMainId]               bigint         NULL,
    [UseCondition]                    nvarchar(500)  NULL,
    [LastCheckDate]                   datetime       NULL,
    [NextCheckDate]                   datetime       NULL,
    [LastRepairDate]                  datetime       NULL,
    [AssetManager]                    nvarchar(60)   NULL,
    [FacilitySign]                    nvarchar(60)   NOT NULL DEFAULT(''),
    [Continuous_WorkTime]             int            NULL,
    [RunTime]                         int            NOT NULL DEFAULT(0),
    [ElectrifyTime]                   int            NOT NULL DEFAULT(0),
    [Continuous_Qty]                  int            NOT NULL DEFAULT(0),
    [Status]                          int            NOT NULL DEFAULT(0),
    [InWarehouseUserId]               bigint         NULL,
    [InWarehouseDate]                 datetime       NULL,
    [CreateDate]                      datetime       NULL,
    [CreateUserId]                    bigint         NULL,
    [TerminalAddress]                 nvarchar(100)  NULL,
    [FormulaIds]                      nvarchar(500)  NULL,
    [MonthTempId]                     bigint         NULL,
    [SeasonTempId]                    bigint         NULL,
    [HalfYearTempId]                  bigint         NULL,
    [WeekTempId]                      bigint         NULL,
    [YearTempId]                      bigint         NULL,
    [LastMonthMainTainDate]           datetime       NULL,
    [LastYSeasonMainTainDate]         datetime       NULL,
    [LastHalfYearMainTainDate]        datetime       NULL,
    [LastYearMainTainDate]            datetime       NULL,
    [Type]                            int            NOT NULL DEFAULT(0),
    [Standard]                        nvarchar(200)  NOT NULL DEFAULT(''),
    [Keeper]                          nvarchar(60)   NOT NULL DEFAULT(''),
    [MonthPlanDay]                    bigint         NOT NULL DEFAULT(0),
    [MonthWeek]                       int            NOT NULL DEFAULT(0),
    [Remark]                          nvarchar(500)  NULL,
    [AcceptanceDate]                  datetime       NULL,
    [NWXCode]                         nvarchar(60)   NULL,
    [KeyFlag]                         int            NOT NULL DEFAULT(0),
    [StandardYears]                   decimal(18,2)  NOT NULL DEFAULT(0),
    [EntityId]                        bigint         NOT NULL DEFAULT(0),
    [ManufactureNumber]               nvarchar(60)   NULL,
    [EquipmentBodyNumber]             nvarchar(60)   NULL,
    [MeasurementRange]                nvarchar(60)   NULL,
    [Resolution]                      nvarchar(60)   NULL,
    [Accuracy]                        nvarchar(60)   NULL,
    [CalibrationDate]                 datetime       NULL,
    [CalibrationCycle]                nvarchar(20)   NULL,
    [CalibrationExpirationDate]       datetime       NULL,
    [CalibrationExpirationWarningDays] int           NULL,
    [Custodian]                       nvarchar(60)   NULL,
    [ActualValue]                     nvarchar(60)   NULL
);
CREATE UNIQUE INDEX UX_Facility_ResourceDetail_Code ON [Facility_ResourceDetail]([FacilityCode]);
CREATE INDEX IX_Facility_ResourceDetail_Dept ON [Facility_ResourceDetail]([DeptId]);
CREATE INDEX IX_Facility_ResourceDetail_Status ON [Facility_ResourceDetail]([Status]);
GO

CREATE TABLE [Facility_Item] (
    [Id]                bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Type]              smallint       NOT NULL DEFAULT(0),
    [Project]           nvarchar(200)  NULL,
    [CheckMethod]       nvarchar(200)  NULL,
    [UpkeepMethod]      nvarchar(200)  NULL,
    [Remark]            nvarchar(500)  NULL,
    [Status]            smallint       NULL,
    [FacilityType]      nvarchar(60)   NOT NULL DEFAULT(''),
    [ControlType]       int            NOT NULL DEFAULT(0),
    [MaxValue]          decimal(18,4)  NULL,
    [MinValue]          decimal(18,4)  NULL,
    [StdMaxValue]       decimal(18,4)  NULL,
    [StdMinValue]       decimal(18,4)  NULL,
    [Maintenance_level] int            NULL,
    [Standardvalue]     decimal(18,4)  NULL,
    [WXFlage]           int            NOT NULL DEFAULT(0)
);
CREATE INDEX IX_Facility_Item_Type ON [Facility_Item]([Type], [Status]);
GO

CREATE TABLE [Facility_TheTemplateMain] (
    [Id]                bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [HNumber]           nvarchar(60)   NULL,
    [HName]             nvarchar(200)  NULL,
    [Maker]             nvarchar(60)   NULL,
    [Checker]           nvarchar(60)   NULL,
    [CheckDate]         datetime       NULL,
    [CloseMan]          nvarchar(60)   NULL,
    [CloseDate]         datetime       NULL,
    [Hdate]             datetime       NULL,
    [Status]            smallint       NULL,
    [Type]              smallint       NOT NULL DEFAULT(0),
    [OutsourcingFlag]   int            NULL,
    [MaintenanceType]   nvarchar(20)   NULL,
    [AlertDays]         int            NULL,
    [Files]             nvarchar(1000) NULL,
    [FGC_Creator]       nvarchar(60)   NULL,
    [FGC_CreateDate]    nvarchar(60)   NULL,
    [FGC_LastModifier]  nvarchar(60)   NULL,
    [FGC_LastModifyDate] nvarchar(60)  NULL
);
CREATE INDEX IX_Facility_TheTemplateMain_Type ON [Facility_TheTemplateMain]([Type], [MaintenanceType]);
GO

CREATE TABLE [Facility_TheTemplateSub] (
    [Id]                  bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [HInspectionItemID]   bigint         NOT NULL DEFAULT(0),
    [HRemark]             nvarchar(500)  NULL,
    [ControlType]         int            NULL,
    [MaxValue]            decimal(18,4)  NULL,
    [MinValue]            decimal(18,4)  NULL,
    [StdMaxValue]         decimal(18,4)  NULL,
    [StdMinValue]         decimal(18,4)  NULL,
    [MainId]              bigint         NOT NULL DEFAULT(0),
    [HContent]            nvarchar(200)  NOT NULL DEFAULT(''),
    [HMethods]            nvarchar(200)  NOT NULL DEFAULT(''),
    [HStandard]           nvarchar(200)  NOT NULL DEFAULT(''),
    [Maintenance_level]   int            NULL
);
CREATE INDEX IX_Facility_TheTemplateSub_Main ON [Facility_TheTemplateSub]([MainId]);
GO

CREATE TABLE [Facility_BillMain] (
    [Id]                bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BillNo]            nvarchar(60)   NULL,
    [BillDate]          datetime       NULL,
    [BillType]          nvarchar(20)   NULL,
    [BeginDate]         datetime       NULL,
    [EndDate]           datetime       NULL,
    [FacilityID]        bigint         NULL,
    [TempID]            bigint         NULL,
    [MaintainType]      nvarchar(20)   NULL,
    [Status]            int            NULL,
    [Remark]            nvarchar(500)  NULL,
    [LastMaintainTime]  datetime       NULL,
    [Dispatch]          nvarchar(60)   NULL,
    [DispatchDate]      datetime       NULL,
    [RepairStaff]       nvarchar(60)   NULL,
    [RepairStaffDate]   datetime       NULL,
    [Checker]           nvarchar(60)   NULL,
    [CheckDate]         datetime       NULL,
    [Closer]            nvarchar(60)   NULL,
    [CloseDate]         datetime       NULL,
    [Maintenance_level] int            NULL,
    [IsOK]              int            NULL,
    [Amount]            decimal(18,2)  NOT NULL DEFAULT(0),
    [Files]             nvarchar(1000) NULL,
    [CreateUserId]      bigint         NOT NULL DEFAULT(0),
    [CreateDate]        datetime       NOT NULL DEFAULT(getdate()),
    [LastUpdateUserId]  bigint         NOT NULL DEFAULT(0),
    [LastUpdateDate]    datetime       NOT NULL DEFAULT(getdate()),
    [CheckerUserId]     bigint         NOT NULL DEFAULT(0),
    [FGC_Creator]       nvarchar(60)   NULL,
    [FGC_CreateDate]    nvarchar(60)   NULL,
    [FGC_LastModifier]  nvarchar(60)   NULL,
    [FGC_LastModifyDate] nvarchar(60)  NULL
);
CREATE INDEX IX_Facility_BillMain_Facility ON [Facility_BillMain]([FacilityID], [BillType]);
CREATE INDEX IX_Facility_BillMain_Status ON [Facility_BillMain]([Status], [BillDate] DESC);
CREATE INDEX IX_Facility_BillMain_BillNo ON [Facility_BillMain]([BillNo]);
GO

CREATE TABLE [Facility_BillSub] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MainId]        bigint         NOT NULL DEFAULT(0),
    [Project]       nvarchar(200)  NOT NULL DEFAULT(''),
    [CheckMethod]   nvarchar(200)  NOT NULL DEFAULT(''),
    [UpkeepMethod]  nvarchar(200)  NOT NULL DEFAULT(''),
    [Result]        nvarchar(200)  NULL,
    [ControlType]   int            NOT NULL DEFAULT(0),
    [MaxValue]      decimal(18,4)  NULL,
    [MinValue]      decimal(18,4)  NULL,
    [StdMaxValue]   decimal(18,4)  NULL,
    [StdMinValue]   decimal(18,4)  NULL,
    [Remark]        nvarchar(500)  NULL,
    [WXFlage]       int            NOT NULL DEFAULT(0)
);
CREATE INDEX IX_Facility_BillSub_Main ON [Facility_BillSub]([MainId]);
GO

CREATE TABLE [Facility_RepairBillMain] (
    [Id]                    bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BillNo]                nvarchar(60)   NULL,
    [BillDate]              datetime       NULL,
    [FacilityId]            bigint         NULL,
    [Descr]                 nvarchar(500)  NULL,
    [RepairTime]            int            NULL,
    [Status]                int            NULL,
    [Remark]                nvarchar(1000) NULL,
    [LastRepairEnd]         datetime       NULL,
    [Dispatch]              nvarchar(60)   NULL,
    [DispatchDate]          datetime       NULL,
    [RepairStaff]           nvarchar(60)   NULL,
    [RepairBeginDate]       datetime       NULL,
    [RepairEndDate]         datetime       NULL,
    [Checker]               nvarchar(60)   NULL,
    [CheckDate]             datetime       NULL,
    [Closer]                nvarchar(60)   NULL,
    [CloseDate]             datetime       NULL,
    [Maker]                 nvarchar(60)   NULL,
    [ResponseDate]          datetime       NULL,
    [OutsourcingFlag]       int            NULL,
    [OutsourcingCreateDate] datetime       NULL,
    [OutsourcingLastDate]   datetime       NULL,
    [FaultCategory]         nvarchar(60)   NULL,
    [FaultLocation]         nvarchar(60)   NULL,
    [ProduceComfirm]        nvarchar(60)   NULL,
    [EquipmentComfirm]      nvarchar(60)   NULL,
    [QualityComfirm]        nvarchar(60)   NULL,
    [ComfirmFlag]           int            NOT NULL DEFAULT(0),
    [ProduceComfirmTime]    datetime       NULL,
    [EquipmentComfirmTime]  datetime       NULL,
    [QualityComfirmTime]    datetime       NULL,
    [ReviewerUserId]        bigint         NOT NULL DEFAULT(0),
    [ReviewDateTime]        datetime       NULL,
    [ReviewRemark]          nvarchar(500)  NULL,
    [FGC_Creator]           nvarchar(60)   NULL,
    [FGC_CreateDate]        nvarchar(60)   NULL,
    [FGC_LastModifier]      nvarchar(60)   NULL,
    [FGC_LastModifyDate]    nvarchar(60)   NULL
);
CREATE INDEX IX_Facility_RepairBillMain_Facility ON [Facility_RepairBillMain]([FacilityId]);
CREATE INDEX IX_Facility_RepairBillMain_Status ON [Facility_RepairBillMain]([Status], [BillDate] DESC);
CREATE INDEX IX_Facility_RepairBillMain_BillNo ON [Facility_RepairBillMain]([BillNo]);
CREATE INDEX IX_Facility_RepairBillMain_Staff ON [Facility_RepairBillMain]([RepairStaff]);
GO

CREATE TABLE [Facility_RepairBillSub] (
    [Id]                bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MainId]            bigint         NULL,
    [ReasonId]          bigint         NULL,
    [Sort]              int            NULL,
    [Remark]            nvarchar(500)  NULL,
    [Descr]             nvarchar(500)  NULL,
    [FaultAnalysis]     nvarchar(500)  NULL,
    [PreventiveMeasure] nvarchar(500)  NULL
);
CREATE INDEX IX_Facility_RepairBillSub_Main ON [Facility_RepairBillSub]([MainId]);
GO

/* =============================================================================
   4. Spare domain (Spare_*)
   ============================================================================= */

CREATE TABLE [Spare_InvoiceMain] (
    [Id]                bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BillNo]            nvarchar(60)   NULL,
    [BillDate]          datetime       NULL,
    [BillType]          bigint         NULL,
    [WHID]              bigint         NULL,
    [DeptId]            bigint         NULL,
    [toWHID]            bigint         NULL,
    [Remark]            nvarchar(500)  NULL,
    [Status]            int            NULL,
    [Checker]           nvarchar(60)   NULL,
    [CheckDate]         datetime       NULL,
    [Closer]            nvarchar(60)   NULL,
    [CloseDate]         datetime       NULL,
    [Renyuan]           bigint         NULL,
    [Type]              nvarchar(20)   NULL,
    [BillId]            bigint         NULL,
    [FGC_Creator]       nvarchar(60)   NULL,
    [FGC_CreateDate]    nvarchar(60)   NULL,
    [FGC_LastModifier]  nvarchar(60)   NULL,
    [FGC_LastModifyDate] nvarchar(60)  NULL
);
CREATE INDEX IX_Spare_InvoiceMain_Type ON [Spare_InvoiceMain]([BillType], [Status]);
CREATE INDEX IX_Spare_InvoiceMain_BillNo ON [Spare_InvoiceMain]([BillNo]);
GO

CREATE TABLE [Spare_InvoiceSub] (
    [Id]              bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MainId]          bigint         NULL,
    [RowNum]          int            NULL,
    [SpareId]         bigint         NULL,
    [Qty]             decimal(18,4)  NULL,
    [Remark]          nvarchar(500)  NULL,
    [Status]          int            NULL,
    [RelationQty]     decimal(18,4)  NULL,
    [NotRelationQty]  decimal(18,4)  NULL,
    [Minpackage]      decimal(18,4)  NULL,
    [Jinshouren]      nvarchar(60)   NULL,
    [Danwei]          nvarchar(20)   NULL,
    [Danjia]          decimal(18,4)  NULL,
    [Kehu]            nvarchar(100)  NULL,
    [Xindanjia]       decimal(18,4)  NULL,
    [Bumen]           nvarchar(100)  NULL,
    [jine]            nvarchar(40)   NULL
);
CREATE INDEX IX_Spare_InvoiceSub_Main ON [Spare_InvoiceSub]([MainId]);
GO

CREATE TABLE [Spare_NowQuan] (
    [Id]           bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SpareId]      bigint         NULL,
    [WarehouseId]  bigint         NULL,
    [AreaId]       bigint         NULL,
    [Qty]          decimal(18,4)  NULL,
    [Danjiaid]     nvarchar(40)   NULL
);
CREATE INDEX IX_Spare_NowQuan_Spare ON [Spare_NowQuan]([SpareId]);
GO

PRINT '==== Tables created ====';
GO

/* =============================================================================
   5. Seed data
   ============================================================================= */

/* --- 5.1 default departments --- */
INSERT INTO [Sys_Dept] ([DeptNumber], [DeptName], [ParentId], [Status]) VALUES
 (N'001',     N'arbore',     0, 1),
 (N'001-01',  N'生产部',    1, 1),
 (N'001-02',  N'设备部',    1, 1),
 (N'001-03',  N'质量部',    1, 1),
 (N'001-04',  N'仓储部',    1, 1),
 (N'001-05',  N'信息部',    1, 1);
GO

/* --- 5.2 admin role + waes user --- */
INSERT INTO [Sys_Role] ([Name], [Status]) VALUES (N'admin', 1);
GO

-- Password 'waes+123456' -> MD5 lowercase (matches Infrastructure.DEncrypt.DesEncrypt.Md5)
INSERT INTO [Sys_User] ([Account], [Password], [Name], [EmployeeId], [DeptId], [Status])
VALUES (N'waes', N'2a1470c83c808553fbcf8395ea731779', N'系统管理员', 0,
        (SELECT TOP 1 [Id] FROM [Sys_Dept] WHERE [DeptName]=N'信息部'), 1);
GO

INSERT INTO [Sys_UserRole] ([UserId], [RoleId])
SELECT u.[Id], r.[Id] FROM [Sys_User] u, [Sys_Role] r
WHERE u.[Account]=N'waes' AND r.[Name]=N'admin';
GO

/* --- 5.3 menus (parent first, then children referenced via subquery) --- */
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon]) VALUES
 (N'设备台账',   'ledger',      NULL, 0, 1,  1, 'package'),
 (N'设备保养',   'maintenance', NULL, 0, 2,  1, 'wrench'),
 (N'设备维修',   'repair',      NULL, 0, 3,  1, 'hammer'),
 (N'设备点检',   'inspection',  NULL, 0, 4,  1, 'clipboard-check'),
 (N'备品备件',   'spare',       NULL, 0, 5,  1, 'boxes'),
 (N'系统管理',   'system',      NULL, 0, 90, 1, 'settings'),
 (N'移动端入口', 'mobile',      '/m', 0, 99, 1, 'smartphone');
GO

-- children, ParentId resolved via SELECT
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'设备资源台账', 'res-detail',  '/Facility_ResourceDetail/Index', [Id], 1, 1, NULL FROM [Sys_Module] WHERE [Code]='ledger';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'保养项目',     'mt-item',     '/Facility_Item/Index',           [Id], 1, 1, NULL FROM [Sys_Module] WHERE [Code]='maintenance';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'保养模板',     'mt-template', '/Facility_TheTemplateMain/Index',[Id], 2, 1, NULL FROM [Sys_Module] WHERE [Code]='maintenance';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'保养工单',     'mt-bill',     '/Facility_BillMain/Index',       [Id], 3, 1, NULL FROM [Sys_Module] WHERE [Code]='maintenance';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'维修工单',     'rp-bill',     '/Facility_RepairBillMain/Index', [Id], 1, 1, NULL FROM [Sys_Module] WHERE [Code]='repair';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'点检项目',     'chk-item',    '/Facility_CheckItem/Index',      [Id], 1, 1, NULL FROM [Sys_Module] WHERE [Code]='inspection';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'点检模板',     'chk-template','/Facility_CheckTemplate/Index',  [Id], 2, 1, NULL FROM [Sys_Module] WHERE [Code]='inspection';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'点检工单',     'chk-bill',    '/Facility_CheckBill/Index',      [Id], 3, 1, NULL FROM [Sys_Module] WHERE [Code]='inspection';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'备件主数据',   'spare-basic', '/Basic_Spare/Index',             [Id], 1, 1, NULL FROM [Sys_Module] WHERE [Code]='spare';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'库存查询',     'spare-stock', '/Spare_NowQuan/Index',           [Id], 2, 1, NULL FROM [Sys_Module] WHERE [Code]='spare';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'入库单',       'spare-in',    '/Spare_InvoiceMain/In',          [Id], 3, 1, NULL FROM [Sys_Module] WHERE [Code]='spare';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'出库单',       'spare-out',   '/Spare_InvoiceMain/Out',         [Id], 4, 1, NULL FROM [Sys_Module] WHERE [Code]='spare';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'全部单据',     'spare-bills', '/Spare_InvoiceMain/Index',       [Id], 5, 1, NULL FROM [Sys_Module] WHERE [Code]='spare';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'用户管理',     'sys-user',    '/Sys_User/Index',                [Id], 1, 1, NULL FROM [Sys_Module] WHERE [Code]='system';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'角色管理',     'sys-role',    '/Sys_Role/Index',                [Id], 2, 1, NULL FROM [Sys_Module] WHERE [Code]='system';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'部门管理',     'sys-dept',    '/Sys_Dept/Index',                [Id], 3, 1, NULL FROM [Sys_Module] WHERE [Code]='system';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'菜单管理',     'sys-module',  '/Sys_Module/Index',              [Id], 4, 1, NULL FROM [Sys_Module] WHERE [Code]='system';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'全局设置',     'sys-setting', '/Sys_Setting/Index',             [Id], 5, 1, NULL FROM [Sys_Module] WHERE [Code]='system';
INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT N'版本记录',     'sys-version', '/Sys_Version/Index',             [Id], 6, 1, NULL FROM [Sys_Module] WHERE [Code]='system';
GO

/* --- 5.4 bind all menus to admin role --- */
INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin'), m.[Id]
FROM [Sys_Module] m;
GO

/* --- 5.5 default settings --- */
INSERT INTO [Sys_Setting] ([Group], [Key], [Value], [ValueType], [Title], [Sort]) VALUES
 ('display', 'dateFormat',      'yyyy-MM-dd',          'string', N'日期格式',       1),
 ('display', 'dateTimeFormat',  'yyyy-MM-dd HH:mm:ss', 'string', N'时间格式',       2),
 ('display', 'timeZone',        'Asia/Shanghai',       'string', N'时区',           3),
 ('display', 'pageSize',        '20',                  'int',    N'列表默认分页',   4),
 ('system',  'companyName',     N'arbore',             'string', N'公司名',         1),
 ('system',  'systemTitle',     N'arbore TPM 设备全生命周期管理系统', 'string', N'系统标题', 2),
 ('system',  'theme',           'forest',              'string', N'主题',           3),
 ('system',  'loginBgColor',    '#1F4D3B',             'string', N'登录页主色',     4),
 ('attachment', 'storageRoot',  N'D:\arbore-tpm\uploads', 'string', N'附件存储根目录', 1),
 ('attachment', 'maxFileSize',  '20971520',            'int',    N'单文件最大字节(20MB)', 2),
 ('attachment', 'allowedExt',   'jpg,jpeg,png,gif,webp,pdf,docx,xlsx,dwg,zip,txt,bmp', 'string', N'允许扩展名', 3),
 ('attachment', 'maxPerBusiness','20',                 'int',    N'单据最多附件数', 4),
 ('attachment', 'thumbnailWidth','200',                'int',    N'缩略图宽度',     5);
GO

/* --- 5.6 initial version --- */
INSERT INTO [Sys_Version] ([Version], [ReleaseDate], [Title], [Content], [IsCurrent], [Author]) VALUES
 ('v1.0.0', '2026-05-12', N'初始版本',
  N'## 首版功能清单' + CHAR(10) +
  N'- 设备台账（含 6 个保养/点检模板挂载、维修历史）' + CHAR(10) +
  N'- 设备保养（项目/模板/工单 + 看板/日历/甘特视图）' + CHAR(10) +
  N'- 设备维修（工单 + 派工 + 批量派工 + 移动端报修）' + CHAR(10) +
  N'- 设备点检（项目/模板/工单 只读）' + CHAR(10) +
  N'- 备品备件（备件主数据 / 库存查询 / 入库 / 出库 / 全部单据）' + CHAR(10) +
  N'- 系统管理（用户 / 角色 / 部门 / 菜单 / 全局设置 / 版本记录）' + CHAR(10) +
  N'- 移动端（保养扫码 / 报修拍照上传）' + CHAR(10) +
  N'- 通用附件子系统（路径全局可配）' + CHAR(10) +
  N'- Serilog 结构化日志（控制台 + 按天滚动文件）',
  0, N'arbore');

INSERT INTO [Sys_Version] ([Version], [ReleaseDate], [Title], [Content], [IsCurrent], [Author]) VALUES
 ('v1.1.0', getdate(), N'保养工单全生命周期 + 附件组件兼容性修复',
  N'## 新增功能' + CHAR(10) +
  N'- 保养派工：列表行级「派工」按钮 + 顶部「批量派工」（状态=新建才允许），员工列表带部门 + 当前待办负载着色' + CHAR(10) +
  N'- 状态流转：派工(0→1) → 开始保养(1→2) → 完工(2→3) → 审核通过(3→4)，每步都有状态守卫' + CHAR(10) +
  N'- 查看视图按状态自动渲染对应动作按钮（开始保养 / 完工填结果 / 审核），无需切页面' + CHAR(10) +
  N'- 完工时收集「保养结果（正常/异常）」+「完工备注」，自动写 EndDate / LastMaintainTime / IsOK / 备注追加' + CHAR(10) +
  N'- 保养人列从工号显示升级为「姓名 (工号)」，从 Basic_Employee 实时映射' + CHAR(10) +
  N'## Bug 修复' + CHAR(10) +
  N'- BatchGenerate 不再让 BeginDate/EndDate = BillDate 占位，按周期自动生成执行窗口：' + CHAR(10) +
  N'  * WEEK   = 周一 00:00 ~ 周日 23:59:59' + CHAR(10) +
  N'  * MONTH  = 月初 00:00 ~ 月末 23:59:59' + CHAR(10) +
  N'  * QUARTER= 季初 00:00 ~ 季末 23:59:59' + CHAR(10) +
  N'  * YEAR   = 1.1 00:00 ~ 12.31 23:59:59' + CHAR(10) +
  N'- 看板 / 日历 / 甘特点击单据进入查看视图修复（ID 类型不匹配导致点击无响应）' + CHAR(10) +
  N'- 通用附件组件「上传附件」按钮无响应修复：内部自调 layui.use([upload, layer])，' + CHAR(10) +
  N'  不再依赖业务页面 layui.use 列表是否包含 upload；上传增加 loading 反馈与失败提示' + CHAR(10) +
  N'## 体验增强' + CHAR(10) +
  N'- 甘特图横条按 [BeginDate, EndDate] 跨格渲染 + 同设备多单 lane stacking，不再只显示在 BillDate 单格内',
  1, N'arbore');
GO

PRINT '==== Schema v2 rebuild completed ====';
PRINT 'Login: waes / waes+123456';
PRINT 'Database: TPM (clean, IDENTITY-based, no snowflake)';
GO
