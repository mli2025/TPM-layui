/* ============================================================
   DeviceMgmt schema DDL (idempotent)
   Target DB: wantong_mes_20250211
   Generated from DeviceMgmt.Repository/Domain/*.cs
   Strategy: skip if a table OR view with the same name exists
   ============================================================ */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[Basic_Employee]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Basic_Employee]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Basic_Employee] (
        [Id] BIGINT NOT NULL,
    [EmployeeNumber] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
    [DeptId] BIGINT NOT NULL,
    [Status] INT NOT NULL,
        CONSTRAINT [PK_Basic_Employee] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Basic_EquipmentResources]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Basic_EquipmentResources]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Basic_EquipmentResources] (
        [Id] BIGINT NOT NULL,
    [Code] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [Status] SMALLINT NULL,
    [EnterDate] DATETIME NULL,
    [DeptId] INT NULL,
        CONSTRAINT [PK_Basic_EquipmentResources] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Basic_Mold]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Basic_Mold]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Basic_Mold] (
        [Id] BIGINT NOT NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Code] NVARCHAR(MAX) NULL,
    [Model] NVARCHAR(MAX) NULL,
    [MoldType] NVARCHAR(MAX) NULL,
    [ConnectFacilityType] NVARCHAR(MAX) NULL,
    [WarehouseId] BIGINT NULL,
    [WareAreaId] BIGINT NULL,
    [WareArea] NVARCHAR(MAX) NULL,
    [SupplierId] BIGINT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [MaxUseQty] INT NULL,
    [MaxUseDay] INT NULL,
    [AlarmQty] INT NULL,
    [AlarmDay] INT NULL,
    [TotalUseQty] INT NULL,
    [LastRepairTime] DATETIME NULL,
    [LastRepairUserId] BIGINT NULL,
    [FacilityId] BIGINT NULL,
    [ResourceId] BIGINT NULL,
    [NowUseQty] INT NULL,
    [Status] INT NULL,
    [TheQtyTemplateMainId] BIGINT NULL,
    [TheDayTemplateMainId] BIGINT NULL,
    [QiangXueQty] INT NULL,
    [Type] NVARCHAR(MAX) NULL,
    [GWThickness] DECIMAL(18,4) NULL,
    [ThickenedFlag] INT NULL,
    [StockQty] DECIMAL(18,4) NOT NULL,
    [MoldQty] DECIMAL(18,4) NOT NULL,
    [SpotCheckFlag] INT NOT NULL,
    [CleanFlag] INT NOT NULL,
    [ResetFlag] INT NOT NULL,
    [AllUseQty] INT NOT NULL,
    [LastUseQty] INT NOT NULL,
        CONSTRAINT [PK_Basic_Mold] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Basic_MoldMaterial]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Basic_MoldMaterial]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Basic_MoldMaterial] (
        [Id] BIGINT NOT NULL,
    [MoldId] BIGINT NOT NULL,
    [MaterialId] BIGINT NOT NULL,
    [Qty] DECIMAL(18,4) NOT NULL,
        CONSTRAINT [PK_Basic_MoldMaterial] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Basic_Spare]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Basic_Spare]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Basic_Spare] (
        [Id] BIGINT NOT NULL,
    [Code] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Specs] NVARCHAR(MAX) NULL,
    [SafeStock] DECIMAL(18,4) NULL,
    [Remark] INT NULL,
    [Status] INT NULL,
    [Leibie] NVARCHAR(MAX) NULL,
    [Danjia] DECIMAL(18,4) NULL,
    [Kehu] NVARCHAR(MAX) NULL,
    [Danwei] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Basic_Spare] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_BillMain]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_BillMain]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_BillMain] (
        [Id] BIGINT NOT NULL,
    [BillNo] NVARCHAR(MAX) NULL,
    [BillDate] DATETIME NULL,
    [BillType] NVARCHAR(MAX) NULL,
    [BeginDate] DATETIME NULL,
    [EndDate] DATETIME NULL,
    [FacilityID] BIGINT NULL,
    [TempID] BIGINT NULL,
    [MaintainType] NVARCHAR(MAX) NULL,
    [Status] INT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [LastMaintainTime] DATETIME NULL,
    [Dispatch] NVARCHAR(MAX) NULL,
    [DispatchDate] DATETIME NULL,
    [RepairStaff] NVARCHAR(MAX) NULL,
    [RepairStaffDate] DATETIME NULL,
    [Checker] NVARCHAR(MAX) NULL,
    [CheckDate] DATETIME NULL,
    [Closer] NVARCHAR(MAX) NULL,
    [CloseDate] DATETIME NULL,
    [Maintenance_level] INT NULL,
    [IsOK] INT NULL,
    [Amount] DECIMAL(18,4) NOT NULL,
    [Files] NVARCHAR(MAX) NULL,
    [CreateUserId] BIGINT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [LastUpdateUserId] BIGINT NOT NULL,
    [LastUpdateDate] DATETIME NOT NULL,
    [CheckerUserId] BIGINT NOT NULL,
    [FGC_Creator] NVARCHAR(MAX) NULL,
    [FGC_CreateDate] NVARCHAR(MAX) NULL,
    [FGC_LastModifier] NVARCHAR(MAX) NULL,
    [FGC_LastModifyDate] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Facility_BillMain] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_BillSub]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_BillSub]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_BillSub] (
        [Id] BIGINT NOT NULL,
    [MainId] BIGINT NOT NULL,
    [Project] NVARCHAR(MAX) NULL,
    [CheckMethod] NVARCHAR(MAX) NULL,
    [UpkeepMethod] NVARCHAR(MAX) NULL,
    [Result] NVARCHAR(MAX) NULL,
    [ControlType] INT NOT NULL,
    [MaxValue] DECIMAL(18,4) NULL,
    [MinValue] DECIMAL(18,4) NULL,
    [StdMaxValue] DECIMAL(18,4) NULL,
    [StdMinValue] DECIMAL(18,4) NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [WXFlage] INT NOT NULL,
        CONSTRAINT [PK_Facility_BillSub] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_DATA]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_DATA]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_DATA] (
        [Id] BIGINT NOT NULL,
    [HResourceId] INT NOT NULL,
    [HWorkDate] DATETIME NOT NULL,
    [HStatus] INT NOT NULL,
    [HStatusStr] NVARCHAR(MAX) NULL,
    [HWorkQty] INT NOT NULL,
    [HStopTimes] INT NOT NULL,
        CONSTRAINT [PK_Facility_DATA] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_DATA_History]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_DATA_History]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_DATA_History] (
        [Id] BIGINT NOT NULL,
    [HResourceId] INT NOT NULL,
    [HWorkDate] DATETIME NOT NULL,
    [HStatusStr] NVARCHAR(MAX) NULL,
    [HWorkQty] INT NOT NULL,
    [HStopTimes] INT NOT NULL,
        CONSTRAINT [PK_Facility_DATA_History] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_Item]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_Item]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_Item] (
        [Id] BIGINT NOT NULL,
    [Type] SMALLINT NOT NULL,
    [Project] NVARCHAR(MAX) NULL,
    [CheckMethod] NVARCHAR(MAX) NULL,
    [UpkeepMethod] NVARCHAR(MAX) NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [Status] SMALLINT NULL,
    [FacilityType] NVARCHAR(MAX) NULL,
    [ControlType] INT NOT NULL,
    [MaxValue] DECIMAL(18,4) NULL,
    [MinValue] DECIMAL(18,4) NULL,
    [StdMaxValue] DECIMAL(18,4) NULL,
    [StdMinValue] DECIMAL(18,4) NULL,
    [Maintenance_level] INT NULL,
    [Standardvalue] DECIMAL(18,4) NULL,
    [WXFlage] INT NOT NULL,
        CONSTRAINT [PK_Facility_Item] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_OutQC]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_OutQC]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_OutQC] (
        [Id] BIGINT NOT NULL,
    [FacilityId] BIGINT NOT NULL,
    [OutDate] DATETIME NOT NULL,
    [EmpId] BIGINT NOT NULL,
    [SupplierId] BIGINT NOT NULL,
    [InspectionAddress] NVARCHAR(MAX) NULL,
    [AcceptancePersonnel] BIGINT NULL,
    [AcceptanceTime] DATETIME NULL,
    [AcceptanceDocuments] NVARCHAR(MAX) NULL,
    [Status] INT NOT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [CreateUserId] BIGINT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [LastUpdateUserId] BIGINT NOT NULL,
    [LastUpdateDate] DATETIME NOT NULL,
        CONSTRAINT [PK_Facility_OutQC] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_OutsourcingMaintenance]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_OutsourcingMaintenance]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_OutsourcingMaintenance] (
        [Id] BIGINT NOT NULL,
    [BillMainid] INT NOT NULL,
    [Number] NVARCHAR(MAX) NULL,
    [MaintenanceType] NVARCHAR(MAX) NULL,
    [Status] INT NULL,
    [Creater] NVARCHAR(MAX) NULL,
    [Acceptance] NVARCHAR(MAX) NULL,
    [Appendix] NVARCHAR(MAX) NULL,
    [EstimatedTime] DATETIME NULL,
    [CreaterTime] DATETIME NULL,
    [AcceptanceTime] DATETIME NULL,
    [SupplierID] INT NULL,
    [Maintainer] NVARCHAR(MAX) NULL,
    [FacilityId] BIGINT NOT NULL,
    [CreateUserId] BIGINT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [LastUpdateUserId] BIGINT NOT NULL,
    [LastUpdateDate] DATETIME NOT NULL,
        CONSTRAINT [PK_Facility_OutsourcingMaintenance] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_OutsourcingRepair]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_OutsourcingRepair]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_OutsourcingRepair] (
        [Id] BIGINT NOT NULL,
    [RepairBillMainid] INT NOT NULL,
    [Number] NVARCHAR(MAX) NULL,
    [Unit] NVARCHAR(MAX) NULL,
    [FaultDescription] NVARCHAR(MAX) NULL,
    [Status] INT NULL,
    [Creater] NVARCHAR(MAX) NULL,
    [Acceptance] NVARCHAR(MAX) NULL,
    [Appendix] NVARCHAR(MAX) NULL,
    [CreaterDate] DATETIME NULL,
    [AcceptanceDate] DATETIME NULL,
    [AcceptanceComments] NVARCHAR(MAX) NULL,
    [FaultLocation] NVARCHAR(MAX) NULL,
    [FaultCategory] NVARCHAR(MAX) NULL,
    [Amount] DECIMAL(18,4) NOT NULL,
        CONSTRAINT [PK_Facility_OutsourcingRepair] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_Process]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_Process]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_Process] (
        [Id] BIGINT NOT NULL,
    [FacilityId] BIGINT NULL,
    [Type] NVARCHAR(MAX) NULL,
    [Date] DATETIME NULL,
    [FDesc] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Facility_Process] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_RepairBillMain]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_RepairBillMain]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_RepairBillMain] (
        [Id] BIGINT NOT NULL,
    [BillNo] NVARCHAR(MAX) NULL,
    [BillDate] DATETIME NULL,
    [FacilityId] BIGINT NULL,
    [Descr] NVARCHAR(MAX) NULL,
    [RepairTime] INT NULL,
    [Status] INT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [LastRepairEnd] DATETIME NULL,
    [Dispatch] NVARCHAR(MAX) NULL,
    [DispatchDate] DATETIME NULL,
    [RepairStaff] NVARCHAR(MAX) NULL,
    [RepairBeginDate] DATETIME NULL,
    [RepairEndDate] DATETIME NULL,
    [Checker] NVARCHAR(MAX) NULL,
    [CheckDate] DATETIME NULL,
    [Closer] NVARCHAR(MAX) NULL,
    [CloseDate] DATETIME NULL,
    [Maker] NVARCHAR(MAX) NULL,
    [ResponseDate] DATETIME NULL,
    [OutsourcingFlag] INT NULL,
    [OutsourcingCreateDate] DATETIME NULL,
    [OutsourcingLastDate] DATETIME NULL,
    [FaultCategory] NVARCHAR(MAX) NULL,
    [FaultLocation] NVARCHAR(MAX) NULL,
    [ProduceComfirm] NVARCHAR(MAX) NULL,
    [EquipmentComfirm] NVARCHAR(MAX) NULL,
    [QualityComfirm] NVARCHAR(MAX) NULL,
    [ComfirmFlag] INT NOT NULL,
    [ProduceComfirmTime] DATETIME NULL,
    [EquipmentComfirmTime] DATETIME NULL,
    [QualityComfirmTime] DATETIME NULL,
    [ReviewerUserId] BIGINT NOT NULL,
    [ReviewDateTime] DATETIME NULL,
    [ReviewRemark] NVARCHAR(MAX) NULL,
    [FGC_Creator] NVARCHAR(MAX) NULL,
    [FGC_CreateDate] NVARCHAR(MAX) NULL,
    [FGC_LastModifier] NVARCHAR(MAX) NULL,
    [FGC_LastModifyDate] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Facility_RepairBillMain] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_RepairBillSub]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_RepairBillSub]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_RepairBillSub] (
        [Id] BIGINT NOT NULL,
    [MainId] BIGINT NULL,
    [ReasonId] BIGINT NULL,
    [Sort] INT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [Descr] NVARCHAR(MAX) NULL,
    [FaultAnalysis] NVARCHAR(MAX) NULL,
    [PreventiveMeasure] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Facility_RepairBillSub] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_RepairEmp]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_RepairEmp]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_RepairEmp] (
        [Id] BIGINT NOT NULL,
    [WorkCenterId] BIGINT NOT NULL,
    [EmpId] BIGINT NOT NULL,
    [EmpId1] BIGINT NOT NULL,
    [EmpId2] BIGINT NOT NULL,
    [EmpId3] BIGINT NOT NULL,
    [Time0] INT NOT NULL,
    [Time1] INT NOT NULL,
    [Status] INT NOT NULL,
    [CreateUserId] BIGINT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [LastUpdateUserId] BIGINT NOT NULL,
    [LastUpdateDate] DATETIME NOT NULL,
    [BYSHUserId] BIGINT NOT NULL,
        CONSTRAINT [PK_Facility_RepairEmp] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_ResourceDetail]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_ResourceDetail]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_ResourceDetail] (
        [Id] BIGINT NOT NULL,
    [FacilityCode] NVARCHAR(MAX) NULL,
    [FacilityName] NVARCHAR(MAX) NULL,
    [FacilityType] NVARCHAR(MAX) NULL,
    [ResourceId] BIGINT NOT NULL,
    [Manufacturer] NVARCHAR(MAX) NULL,
    [Supplier] NVARCHAR(MAX) NULL,
    [ManufacturerDate] DATETIME NOT NULL,
    [ManufactureCountry] NVARCHAR(MAX) NULL,
    [Model] NVARCHAR(MAX) NULL,
    [ExpireDate] DATETIME NULL,
    [PurchasePrice] DECIMAL(18,4) NOT NULL,
    [PurchaseDate] DATETIME NULL,
    [SerialNumber] NVARCHAR(MAX) NULL,
    [EquipmentManual] NVARCHAR(MAX) NULL,
    [EquipmentDrawing] NVARCHAR(MAX) NULL,
    [Location] NVARCHAR(MAX) NULL,
    [DeptId] BIGINT NOT NULL,
    [AssetNumber] NVARCHAR(MAX) NULL,
    [Voltage] INT NULL,
    [Size] NVARCHAR(MAX) NULL,
    [Weight] INT NULL,
    [The5STemplateMainId] BIGINT NULL,
    [TheTemplateMainId] BIGINT NULL,
    [UseCondition] NVARCHAR(MAX) NULL,
    [LastCheckDate] DATETIME NULL,
    [NextCheckDate] DATETIME NULL,
    [LastRepairDate] DATETIME NULL,
    [AssetManager] NVARCHAR(MAX) NULL,
    [FacilitySign] NVARCHAR(MAX) NULL,
    [Continuous_WorkTime] INT NULL,
    [RunTime] INT NOT NULL,
    [ElectrifyTime] INT NOT NULL,
    [Continuous_Qty] INT NOT NULL,
    [Status] INT NOT NULL,
    [InWarehouseUserId] BIGINT NULL,
    [InWarehouseDate] DATETIME NULL,
    [CreateDate] DATETIME NULL,
    [CreateUserId] BIGINT NULL,
    [TerminalAddress] NVARCHAR(MAX) NULL,
    [FormulaIds] NVARCHAR(MAX) NULL,
    [MonthTempId] BIGINT NULL,
    [SeasonTempId] BIGINT NULL,
    [HalfYearTempId] BIGINT NULL,
    [WeekTempId] BIGINT NULL,
    [YearTempId] BIGINT NULL,
    [LastMonthMainTainDate] DATETIME NULL,
    [LastYSeasonMainTainDate] DATETIME NULL,
    [LastHalfYearMainTainDate] DATETIME NULL,
    [LastYearMainTainDate] DATETIME NULL,
    [Type] INT NOT NULL,
    [Standard] NVARCHAR(MAX) NULL,
    [Keeper] NVARCHAR(MAX) NULL,
    [MonthPlanDay] BIGINT NOT NULL,
    [MonthWeek] INT NOT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [AcceptanceDate] DATETIME NULL,
    [NWXCode] NVARCHAR(MAX) NULL,
    [KeyFlag] INT NOT NULL,
    [StandardYears] DECIMAL(18,4) NOT NULL,
    [EntityId] BIGINT NOT NULL,
    [ManufactureNumber] NVARCHAR(MAX) NULL,
    [EquipmentBodyNumber] NVARCHAR(MAX) NULL,
    [MeasurementRange] NVARCHAR(MAX) NULL,
    [Resolution] NVARCHAR(MAX) NULL,
    [Accuracy] NVARCHAR(MAX) NULL,
    [CalibrationDate] DATETIME NULL,
    [CalibrationCycle] NVARCHAR(MAX) NULL,
    [CalibrationExpirationDate] DATETIME NULL,
    [CalibrationExpirationWarningDays] INT NULL,
    [Custodian] NVARCHAR(MAX) NULL,
    [ActualValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Facility_ResourceDetail] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_ResourceDetailGather]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_ResourceDetailGather]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_ResourceDetailGather] (
        [Id] BIGINT NOT NULL,
    [FacilityId] BIGINT NOT NULL,
    [Date] DATETIME NOT NULL,
    [Status] SMALLINT NOT NULL,
    [CreateUserId] BIGINT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [LastUpdateUserId] BIGINT NOT NULL,
    [LastUpdateDate] DATETIME NOT NULL,
        CONSTRAINT [PK_Facility_ResourceDetailGather] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_Status_History]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_Status_History]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_Status_History] (
        [Id] BIGINT NOT NULL,
    [HResourceId] INT NOT NULL,
    [HOPStr] NVARCHAR(MAX) NULL,
    [HWorkDate] DATETIME NOT NULL,
    [HOperator] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Facility_Status_History] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_TheTemplateMain]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_TheTemplateMain]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_TheTemplateMain] (
        [Id] BIGINT NOT NULL,
    [HNumber] NVARCHAR(MAX) NULL,
    [HName] NVARCHAR(MAX) NULL,
    [Maker] NVARCHAR(MAX) NULL,
    [Checker] NVARCHAR(MAX) NULL,
    [CheckDate] DATETIME NULL,
    [CloseMan] NVARCHAR(MAX) NULL,
    [CloseDate] DATETIME NULL,
    [Hdate] DATETIME NULL,
    [Status] SMALLINT NULL,
    [Type] SMALLINT NOT NULL,
    [OutsourcingFlag] INT NULL,
    [MaintenanceType] NVARCHAR(MAX) NULL,
    [AlertDays] INT NULL,
    [Files] NVARCHAR(MAX) NULL,
    [FGC_Creator] NVARCHAR(MAX) NULL,
    [FGC_CreateDate] NVARCHAR(MAX) NULL,
    [FGC_LastModifier] NVARCHAR(MAX) NULL,
    [FGC_LastModifyDate] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Facility_TheTemplateMain] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Facility_TheTemplateSub]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Facility_TheTemplateSub]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Facility_TheTemplateSub] (
        [Id] BIGINT NOT NULL,
    [HInspectionItemID] BIGINT NOT NULL,
    [HRemark] NVARCHAR(MAX) NULL,
    [ControlType] INT NULL,
    [MaxValue] DECIMAL(18,4) NULL,
    [MinValue] DECIMAL(18,4) NULL,
    [StdMaxValue] DECIMAL(18,4) NULL,
    [StdMinValue] DECIMAL(18,4) NULL,
    [MainId] BIGINT NOT NULL,
    [HContent] NVARCHAR(MAX) NULL,
    [HMethods] NVARCHAR(MAX) NULL,
    [HStandard] NVARCHAR(MAX) NULL,
    [Maintenance_level] INT NULL,
        CONSTRAINT [PK_Facility_TheTemplateSub] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_BillMain]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_BillMain]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_BillMain] (
        [Id] BIGINT NOT NULL,
    [BillNo] NVARCHAR(MAX) NULL,
    [BillDate] DATETIME NULL,
    [BillType] NVARCHAR(MAX) NULL,
    [BeginDate] DATETIME NULL,
    [EndDate] DATETIME NULL,
    [MoldID] BIGINT NULL,
    [TempID] BIGINT NULL,
    [MaintainType] NVARCHAR(MAX) NULL,
    [Status] INT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [LastMaintainTime] DATETIME NULL,
    [Dispatch] NVARCHAR(MAX) NULL,
    [DispatchDate] DATETIME NULL,
    [RepairStaff] NVARCHAR(MAX) NULL,
    [RepairStaffDate] DATETIME NULL,
    [Checker] NVARCHAR(MAX) NULL,
    [CheckDate] DATETIME NULL,
    [Closer] NVARCHAR(MAX) NULL,
    [CloseDate] DATETIME NULL,
    [QXYId] BIGINT NOT NULL,
    [CreateUserld] BIGINT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [LastUpdateUserld] BIGINT NOT NULL,
    [LastUpdateDate] DATETIME NOT NULL,
    [FGC_Creator] NVARCHAR(MAX) NULL,
    [FGC_CreateDate] NVARCHAR(MAX) NULL,
    [FGC_LastModifier] NVARCHAR(MAX) NULL,
    [FGC_LastModifyDate] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Mold_BillMain] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_BillSub]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_BillSub]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_BillSub] (
        [Id] BIGINT NOT NULL,
    [MainId] BIGINT NOT NULL,
    [Project] NVARCHAR(MAX) NULL,
    [CheckMethod] NVARCHAR(MAX) NULL,
    [UpkeepMethod] NVARCHAR(MAX) NULL,
    [Result] NVARCHAR(MAX) NULL,
    [ControlType] INT NOT NULL,
    [MaxValue] DECIMAL(18,4) NULL,
    [MinValue] DECIMAL(18,4) NULL,
    [StdMaxValue] DECIMAL(18,4) NULL,
    [StdMinValue] DECIMAL(18,4) NULL,
        CONSTRAINT [PK_Mold_BillSub] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_InOut]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_InOut]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_InOut] (
        [Id] BIGINT NOT NULL,
    [MoldId] BIGINT NOT NULL,
    [Status] SMALLINT NOT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [Qty] DECIMAL(18,4) NOT NULL,
    [Type] NVARCHAR(MAX) NULL,
    [PersonId] BIGINT NOT NULL,
    [Date] DATETIME NOT NULL,
    [CreateUserld] BIGINT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [LastUpdateUserld] BIGINT NOT NULL,
    [LastUpdateDate] DATETIME NOT NULL,
    [AreaId] BIGINT NOT NULL,
        CONSTRAINT [PK_Mold_InOut] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_Item]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_Item]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_Item] (
        [Id] BIGINT NOT NULL,
    [Type] SMALLINT NOT NULL,
    [Project] NVARCHAR(MAX) NULL,
    [CheckMethod] NVARCHAR(MAX) NULL,
    [UpkeepMethod] NVARCHAR(MAX) NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [Status] SMALLINT NULL,
    [MoldType] INT NULL,
    [ControlType] INT NOT NULL,
    [MaxValue] DECIMAL(18,4) NULL,
    [MinValue] DECIMAL(18,4) NULL,
    [StdMaxValue] DECIMAL(18,4) NULL,
    [StdMinValue] DECIMAL(18,4) NULL,
        CONSTRAINT [PK_Mold_Item] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_OnOff]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_OnOff]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_OnOff] (
        [Id] BIGINT NOT NULL,
    [MoldId] BIGINT NOT NULL,
    [OnPersonId] BIGINT NULL,
    [OnDate] DATETIME NULL,
    [OffPersonId] BIGINT NULL,
    [OffDate] DATETIME NULL,
    [ResourceId] BIGINT NULL,
    [Status] SMALLINT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [UseQty] DECIMAL(18,4) NOT NULL,
        CONSTRAINT [PK_Mold_OnOff] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_OnOffSub]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_OnOffSub]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_OnOffSub] (
        [Id] BIGINT NOT NULL,
    [MainId] BIGINT NOT NULL,
    [TaskBillId] BIGINT NOT NULL,
    [PlanId] BIGINT NOT NULL,
    [ReportId] BIGINT NOT NULL,
    [BarcodeCP] NVARCHAR(MAX) NULL,
    [MoldId] BIGINT NOT NULL,
    [Qty] DECIMAL(18,4) NOT NULL,
    [Status] INT NOT NULL,
    [CreateUserId] BIGINT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [LastUpdateUserId] BIGINT NOT NULL,
    [LastUpdateDate] DATETIME NOT NULL,
    [UpReportQty] DECIMAL(18,4) NOT NULL,
    [DropReportQty] DECIMAL(18,4) NOT NULL,
        CONSTRAINT [PK_Mold_OnOffSub] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_RepairBill]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_RepairBill]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_RepairBill] (
        [Id] BIGINT NOT NULL,
    [BillNo] NVARCHAR(MAX) NULL,
    [BillDate] DATETIME NOT NULL,
    [MoldId] BIGINT NOT NULL,
    [Status] INT NOT NULL,
    [Descr] NVARCHAR(MAX) NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [LastRepairEnd] DATETIME NULL,
    [RepairStaff] NVARCHAR(MAX) NULL,
    [RepairBeginDate] DATETIME NULL,
    [RepairEndDate] DATETIME NULL,
        CONSTRAINT [PK_Mold_RepairBill] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_TheTemplateMain]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_TheTemplateMain]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_TheTemplateMain] (
        [Id] BIGINT NOT NULL,
    [HNumber] NVARCHAR(MAX) NULL,
    [HName] NVARCHAR(MAX) NULL,
    [Maker] NVARCHAR(MAX) NULL,
    [Checker] NVARCHAR(MAX) NULL,
    [CheckDate] DATETIME NULL,
    [CloseMan] NVARCHAR(MAX) NULL,
    [CloseDate] DATETIME NULL,
    [Hdate] DATETIME NULL,
    [Status] SMALLINT NULL,
    [Type] SMALLINT NOT NULL,
    [MoldType] INT NULL,
    [FGC_Creator] NVARCHAR(MAX) NULL,
    [FGC_CreateDate] NVARCHAR(MAX) NULL,
    [FGC_LastModifier] NVARCHAR(MAX) NULL,
    [FGC_LastModifyDate] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Mold_TheTemplateMain] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Mold_TheTemplateSub]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Mold_TheTemplateSub]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mold_TheTemplateSub] (
        [Id] BIGINT NOT NULL,
    [HInspectionItemID] BIGINT NOT NULL,
    [HRemark] NVARCHAR(MAX) NULL,
    [ControlType] INT NULL,
    [MaxValue] DECIMAL(18,4) NULL,
    [MinValue] DECIMAL(18,4) NULL,
    [StdMaxValue] DECIMAL(18,4) NULL,
    [StdMinValue] DECIMAL(18,4) NULL,
    [MainId] BIGINT NOT NULL,
    [HContent] NVARCHAR(MAX) NULL,
    [HMethods] NVARCHAR(MAX) NULL,
    [HStandard] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Mold_TheTemplateSub] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[OEE_Rate]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[OEE_Rate]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[OEE_Rate] (
        [Id] BIGINT NOT NULL,
    [DeptId] INT NOT NULL,
    [ResourceId] INT NOT NULL,
    [WorkDate] DATETIME NOT NULL,
    [ClassId] INT NOT NULL,
    [TaskBillId] INT NOT NULL,
    [MaterialId] INT NOT NULL,
    [stdTimes] DECIMAL(18,4) NOT NULL,
    [ReportQty] INT NOT NULL,
    [StopStart] TIME(7) NOT NULL,
    [StopEnd] TIME(7) NOT NULL,
        CONSTRAINT [PK_OEE_Rate] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[OEE_Scrap]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[OEE_Scrap]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[OEE_Scrap] (
        [Id] BIGINT NOT NULL,
    [DeptId] INT NOT NULL,
    [ResourceId] INT NOT NULL,
    [WorkDate] DATETIME NOT NULL,
    [ClassId] INT NOT NULL,
    [TaskBillId] INT NOT NULL,
    [ScrapReasonId] INT NOT NULL,
    [ScrapQty] INT NOT NULL,
        CONSTRAINT [PK_OEE_Scrap] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[OEE_StopTimes]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[OEE_StopTimes]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[OEE_StopTimes] (
        [Id] BIGINT NOT NULL,
    [DeptId] INT NOT NULL,
    [ResourceId] INT NOT NULL,
    [WorkDate] DATETIME NOT NULL,
    [ClassId] INT NOT NULL,
    [StopReasonId] INT NOT NULL,
    [StopStart] TIME(7) NOT NULL,
    [StopEnd] TIME(7) NOT NULL,
    [StopTimes] DECIMAL(18,4) NOT NULL,
        CONSTRAINT [PK_OEE_StopTimes] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[OEE_TotalTimes]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[OEE_TotalTimes]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[OEE_TotalTimes] (
        [Id] BIGINT NOT NULL,
    [DeptId] INT NOT NULL,
    [ResourceId] INT NOT NULL,
    [WorkDate] DATETIME NOT NULL,
    [ClassId] INT NOT NULL,
    [TotalTimes] DECIMAL(18,4) NOT NULL,
    [NotHavTaskTimes] DECIMAL(18,4) NOT NULL,
        CONSTRAINT [PK_OEE_TotalTimes] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[rpt_OEE]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[rpt_OEE]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[rpt_OEE] (
        [Id] BIGINT NOT NULL,
    [rptId] NVARCHAR(MAX) NULL,
    [RowId] INT NOT NULL,
    [FName] NVARCHAR(MAX) NULL,
    [TotalData] DECIMAL(18,4) NOT NULL,
    [H1] DECIMAL(18,4) NOT NULL,
    [H2] DECIMAL(18,4) NOT NULL,
    [H3] DECIMAL(18,4) NOT NULL,
    [H4] DECIMAL(18,4) NOT NULL,
    [H5] DECIMAL(18,4) NOT NULL,
    [H6] DECIMAL(18,4) NOT NULL,
    [H7] DECIMAL(18,4) NOT NULL,
    [H8] DECIMAL(18,4) NOT NULL,
    [H9] DECIMAL(18,4) NOT NULL,
    [H10] DECIMAL(18,4) NOT NULL,
    [H11] DECIMAL(18,4) NOT NULL,
    [H12] DECIMAL(18,4) NOT NULL,
    [H13] DECIMAL(18,4) NOT NULL,
    [H14] DECIMAL(18,4) NOT NULL,
    [H15] DECIMAL(18,4) NOT NULL,
    [H16] DECIMAL(18,4) NOT NULL,
    [H17] DECIMAL(18,4) NOT NULL,
    [H18] DECIMAL(18,4) NOT NULL,
    [H19] DECIMAL(18,4) NOT NULL,
    [H20] DECIMAL(18,4) NOT NULL,
    [H21] DECIMAL(18,4) NOT NULL,
    [H22] DECIMAL(18,4) NOT NULL,
    [H23] DECIMAL(18,4) NOT NULL,
    [H24] DECIMAL(18,4) NOT NULL,
    [H25] DECIMAL(18,4) NOT NULL,
    [H26] DECIMAL(18,4) NOT NULL,
    [H27] DECIMAL(18,4) NOT NULL,
    [H28] DECIMAL(18,4) NOT NULL,
    [H29] DECIMAL(18,4) NOT NULL,
    [H30] DECIMAL(18,4) NOT NULL,
    [H31] DECIMAL(18,4) NOT NULL,
        CONSTRAINT [PK_rpt_OEE] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Spare_InvoiceData]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Spare_InvoiceData]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Spare_InvoiceData] (
        [Id] BIGINT NOT NULL,
    [SpareId] INT NULL,
    [Code] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Specs] NVARCHAR(MAX) NULL,
    [SafeStock] NVARCHAR(MAX) NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [Status] NVARCHAR(MAX) NULL,
    [Leibie] NVARCHAR(MAX) NULL,
    [Danjia] DECIMAL(18,4) NULL,
    [Kehu] NVARCHAR(MAX) NULL,
    [Danwei] NVARCHAR(MAX) NULL,
    [QCQty] DECIMAL(18,4) NULL,
    [QCJe] DECIMAL(18,4) NULL,
    [InQty] DECIMAL(18,4) NULL,
    [InJe] DECIMAL(18,4) NULL,
    [OutQty] DECIMAL(18,4) NULL,
    [OutJe] DECIMAL(18,4) NULL,
    [JCQty] DECIMAL(18,4) NULL,
    [JCJe] DECIMAL(18,4) NULL,
        CONSTRAINT [PK_Spare_InvoiceData] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Spare_InvoiceMain]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Spare_InvoiceMain]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Spare_InvoiceMain] (
        [Id] BIGINT NOT NULL,
    [BillNo] NVARCHAR(MAX) NULL,
    [BillDate] DATETIME NULL,
    [BillType] BIGINT NULL,
    [WHID] BIGINT NULL,
    [DeptId] BIGINT NULL,
    [toWHID] BIGINT NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [Status] INT NULL,
    [Checker] NVARCHAR(MAX) NULL,
    [CheckDate] DATETIME NULL,
    [Closer] NVARCHAR(MAX) NULL,
    [CloseDate] DATETIME NULL,
    [Renyuan] BIGINT NULL,
    [Type] NVARCHAR(MAX) NULL,
    [BillId] BIGINT NULL,
    [FGC_Creator] NVARCHAR(MAX) NULL,
    [FGC_CreateDate] NVARCHAR(MAX) NULL,
    [FGC_LastModifier] NVARCHAR(MAX) NULL,
    [FGC_LastModifyDate] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Spare_InvoiceMain] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Spare_InvoiceSub]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Spare_InvoiceSub]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Spare_InvoiceSub] (
        [Id] BIGINT NOT NULL,
    [MainId] BIGINT NULL,
    [RowNum] INT NULL,
    [SpareId] BIGINT NULL,
    [Qty] DECIMAL(18,4) NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [Status] INT NULL,
    [RelationQty] DECIMAL(18,4) NULL,
    [NotRelationQty] DECIMAL(18,4) NULL,
    [Minpackage] DECIMAL(18,4) NULL,
    [Jinshouren] NVARCHAR(MAX) NULL,
    [Danwei] NVARCHAR(MAX) NULL,
    [Danjia] DECIMAL(18,4) NULL,
    [Kehu] NVARCHAR(MAX) NULL,
    [Xindanjia] DECIMAL(18,4) NULL,
    [Bumen] NVARCHAR(MAX) NULL,
    [jine] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Spare_InvoiceSub] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Spare_NowQuan]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Spare_NowQuan]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Spare_NowQuan] (
        [Id] BIGINT NOT NULL,
    [SpareId] BIGINT NULL,
    [WarehouseId] BIGINT NULL,
    [AreaId] BIGINT NULL,
    [Qty] DECIMAL(18,4) NULL,
    [Danjiaid] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Spare_NowQuan] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Sys_Dept]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_Dept]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_Dept] (
        [Id] BIGINT NOT NULL,
    [DeptNumber] NVARCHAR(MAX) NULL,
    [DeptName] NVARCHAR(MAX) NULL,
    [ParentId] BIGINT NOT NULL,
    [Status] INT NOT NULL,
        CONSTRAINT [PK_Sys_Dept] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Sys_Module]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_Module]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_Module] (
        [Id] BIGINT NOT NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Code] NVARCHAR(MAX) NULL,
    [Url] NVARCHAR(MAX) NULL,
    [ParentId] BIGINT NOT NULL,
    [Sort] INT NOT NULL,
    [Status] INT NOT NULL,
    [Icon] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Sys_Module] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Sys_ModuleButtons]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_ModuleButtons]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_ModuleButtons] (
        [Id] BIGINT NOT NULL,
    [ModuleId] BIGINT NOT NULL,
    [DomId] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Sys_ModuleButtons] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Sys_RoleModule]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_RoleModule]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_RoleModule] (
        [Id] BIGINT NOT NULL,
    [RoleId] BIGINT NOT NULL,
    [ModuleId] BIGINT NOT NULL,
        CONSTRAINT [PK_Sys_RoleModule] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Sys_Role]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_Role]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_Role] (
        [Id] BIGINT NOT NULL,
    [Name] NVARCHAR(MAX) NULL,
    [Status] INT NOT NULL,
        CONSTRAINT [PK_Sys_Role] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Sys_UserRole]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_UserRole]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_UserRole] (
        [Id] BIGINT NOT NULL,
    [UserId] BIGINT NOT NULL,
    [RoleId] BIGINT NOT NULL,
        CONSTRAINT [PK_Sys_UserRole] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Sys_User]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_User]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_User] (
        [Id] BIGINT NOT NULL,
    [Account] NVARCHAR(MAX) NULL,
    [Password] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
    [EmployeeId] BIGINT NOT NULL,
    [DeptId] BIGINT NOT NULL,
    [Status] INT NOT NULL,
    [CreateDate] DATETIME NOT NULL,
        CONSTRAINT [PK_Sys_User] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_dianjiandept]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_dianjiandept]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_dianjiandept] (
        [Id] BIGINT NOT NULL,
    [Code] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_v_dianjiandept] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_Facility_BillMain]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_Facility_BillMain]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_Facility_BillMain] (
        [Id] BIGINT NOT NULL,
        CONSTRAINT [PK_v_Facility_BillMain] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_Facility_RepairHistory]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_Facility_RepairHistory]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_Facility_RepairHistory] (
        [Id] BIGINT NOT NULL,
    [BillNo] NVARCHAR(MAX) NULL,
    [RecordDate] DATETIME NULL,
    [EmpName] NVARCHAR(MAX) NULL,
    [FacilityId] BIGINT NULL,
    [Status] INT NOT NULL,
    [ReasonId] BIGINT NULL,
        CONSTRAINT [PK_v_Facility_RepairHistory] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_Facility_ResourceDetailGather]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_Facility_ResourceDetailGather]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_Facility_ResourceDetailGather] (
        [Id] BIGINT NOT NULL,
    [FacilityCode] NVARCHAR(MAX) NULL,
    [FacilityName] NVARCHAR(MAX) NULL,
    [DeptId] BIGINT NOT NULL,
    [Status] SMALLINT NOT NULL,
        CONSTRAINT [PK_v_Facility_ResourceDetailGather] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_Facility_ResourceDetailStatus]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_Facility_ResourceDetailStatus]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_Facility_ResourceDetailStatus] (
        [Id] BIGINT NOT NULL,
    [Status] INT NOT NULL,
    [LastSpotCheck] DATETIME NULL,
        CONSTRAINT [PK_v_Facility_ResourceDetailStatus] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_FacilityDD]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_FacilityDD]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_FacilityDD] (
        [Id] BIGINT NOT NULL,
    [ResourceId] BIGINT NOT NULL,
    [FacilityCode] NVARCHAR(MAX) NULL,
    [FacilityName] NVARCHAR(MAX) NULL,
    [BillDate] DATETIME NOT NULL,
        CONSTRAINT [PK_v_FacilityDD] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_Mold_BillMain]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_Mold_BillMain]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_Mold_BillMain] (
        [Id] BIGINT NOT NULL,
        CONSTRAINT [PK_v_Mold_BillMain] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_MoldDayAlarm]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_MoldDayAlarm]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_MoldDayAlarm] (
        [Id] BIGINT NOT NULL,
    [MaxUseDay] INT NULL,
    [AlarmDay] INT NULL,
    [NowUseDay] INT NULL,
    [LastUseQty] INT NULL,
    [AlarmDayFlag] INT NOT NULL,
        CONSTRAINT [PK_v_MoldDayAlarm] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[v_MoldQtyAlarm]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[v_MoldQtyAlarm]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[v_MoldQtyAlarm] (
        [Id] BIGINT NOT NULL,
    [MaxUseQty] INT NULL,
    [AlarmQty] INT NULL,
    [NowUseQty] INT NULL,
    [LastUseQty] INT NULL,
    [AlarmQtyFlag] INT NOT NULL,
        CONSTRAINT [PK_v_MoldQtyAlarm] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[V_Production_BarcodeSMT]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[V_Production_BarcodeSMT]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[V_Production_BarcodeSMT] (
        [Id] BIGINT NOT NULL,
    [Date] DATETIME NOT NULL,
    [BarCode] NVARCHAR(MAX) NULL,
    [ProcedureNo] NVARCHAR(MAX) NULL,
    [Num] NVARCHAR(MAX) NULL,
    [Remark] NVARCHAR(MAX) NULL,
    [CreateDate] DATETIME NOT NULL,
        CONSTRAINT [PK_V_Production_BarcodeSMT] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[WMS_BarCodeInfo_Spares]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[WMS_BarCodeInfo_Spares]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[WMS_BarCodeInfo_Spares] (
        [Id] BIGINT NOT NULL,
    [BarCode] NVARCHAR(MAX) NULL,
    [Status] INT NOT NULL,
        CONSTRAINT [PK_WMS_BarCodeInfo_Spares] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[WMS_BarCodeInfo_Spares_Sub]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[WMS_BarCodeInfo_Spares_Sub]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[WMS_BarCodeInfo_Spares_Sub] (
        [Id] BIGINT NOT NULL,
    [HSparesBarCode] NVARCHAR(MAX) NULL,
    [BarCode] NVARCHAR(MAX) NULL,
    [Status] INT NOT NULL,
        CONSTRAINT [PK_WMS_BarCodeInfo_Spares_Sub] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Wuziku]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Wuziku]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Wuziku] (
        [Id] BIGINT NOT NULL,
    [Leibie] NVARCHAR(MAX) NULL,
    [Beizhu] NVARCHAR(MAX) NULL,
    [Bianma] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Wuziku] PRIMARY KEY ([Id])
    );
END;
GO

