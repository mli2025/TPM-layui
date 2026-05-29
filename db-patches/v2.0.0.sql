/* =============================================================================
   arbore TPM · Patch v1.1.1 -> v2.0.0  (URS 对标 · 全新模块存储设计)
   Target: SQL Server 2017+   Database: [TPM]
   依据：《天台药业设备管理模块 URS 2026.4.29》185 条 / 12 模块
   说明：
     - 只「新增」表与列，不改动现有 24 张表的已有结构
     - 全部 PK = bigint IDENTITY(1,1)，无 FK（应用层校验，沿用 v2 约定）
     - 幂等：每张表用 IF OBJECT_ID(...) IS NULL 守卫，可重复执行
     - 新增菜单一律 [Status]=0（占位，隐藏），对应工作包页面交付后再置 1
     - 采集（能源）由 n8n 团队写入，本脚本只建存储表
   工作包：WP-A 平台底座 / WP-B 设备档案 / WP-C 维修 / WP-D 点巡检 /
           WP-E 维保 / WP-F 特种设备 / WP-G 安全附件 / WP-H 计量器具 /
           WP-I 备件 / WP-J 能源
   ============================================================================= */

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE [TPM];
GO

/* =============================================================================
   WP-A 平台底座补强（URS 201-217 / 301-306 / 401-410 / 1401-1406）
   ============================================================================= */

/* --- A.2 用户权限补全：用户组 / 组成员 / 组-菜单权限 / 登录日志 / 账户锁定 --- */
IF OBJECT_ID(N'[Sys_UserGroup]', N'U') IS NULL
CREATE TABLE [Sys_UserGroup] (
    [Id]          bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]        nvarchar(100) NOT NULL,
    [Descr]       nvarchar(300) NULL,
    [Status]      int           NOT NULL DEFAULT(1),
    [CreateDate]  datetime      NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Sys_UserGroupUser]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sys_UserGroupUser] (
        [Id]       bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [GroupId]  bigint NOT NULL,
        [UserId]   bigint NOT NULL
    );
    CREATE INDEX IX_Sys_UserGroupUser_Group ON [Sys_UserGroupUser]([GroupId]);
    CREATE INDEX IX_Sys_UserGroupUser_User  ON [Sys_UserGroupUser]([UserId]);
END
GO
IF OBJECT_ID(N'[Sys_UserGroupModule]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sys_UserGroupModule] (
        [Id]        bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [GroupId]   bigint NOT NULL,
        [ModuleId]  bigint NOT NULL
    );
    CREATE INDEX IX_Sys_UserGroupModule_Group  ON [Sys_UserGroupModule]([GroupId]);
    CREATE INDEX IX_Sys_UserGroupModule_Module ON [Sys_UserGroupModule]([ModuleId]);
END
GO
IF OBJECT_ID(N'[Sys_LoginLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sys_LoginLog] (
        [Id]          bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]      bigint        NULL,
        [Account]     nvarchar(60)  NULL,
        [LoginTime]   datetime      NOT NULL DEFAULT(getdate()),
        [IpAddress]   nvarchar(60)  NULL,
        [UserAgent]   nvarchar(300) NULL,
        [Success]     bit           NOT NULL DEFAULT(1),
        [FailReason]  nvarchar(200) NULL
    );
    CREATE INDEX IX_Sys_LoginLog_User ON [Sys_LoginLog]([UserId], [LoginTime] DESC);
    CREATE INDEX IX_Sys_LoginLog_Time ON [Sys_LoginLog]([LoginTime] DESC);
END
GO
IF OBJECT_ID(N'[Sys_AccountLock]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sys_AccountLock] (
        [Id]          bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]      bigint        NOT NULL,
        [Account]     nvarchar(60)  NULL,
        [FailCount]   int           NOT NULL DEFAULT(0),
        [LockedAt]    datetime      NULL,
        [UnlockedAt]  datetime      NULL,
        [UnlockedBy]  nvarchar(60)  NULL,
        [IsLocked]    bit           NOT NULL DEFAULT(0)
    );
    CREATE INDEX IX_Sys_AccountLock_User ON [Sys_AccountLock]([UserId]);
END
GO

/* --- A.1 审计追踪：字段级变更明细（旧值/新值/理由），配合现有 Sys_OperationLog --- */
IF OBJECT_ID(N'[Sys_AuditTrail]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sys_AuditTrail] (
        [Id]           bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [LogId]        bigint         NULL,           -- 关联 Sys_OperationLog
        [UserId]       bigint         NULL,
        [UserAccount]  nvarchar(60)   NULL,
        [Module]       nvarchar(60)   NULL,
        [TargetType]   nvarchar(60)   NULL,           -- 业务模块/表
        [TargetId]     nvarchar(60)   NULL,           -- 记录编号
        [ActionType]   nvarchar(20)   NOT NULL,       -- CREATE/UPDATE/DELETE
        [FieldName]    nvarchar(100)  NULL,
        [OldValue]     nvarchar(max)  NULL,
        [NewValue]     nvarchar(max)  NULL,
        [Reason]       nvarchar(500)  NULL,           -- 操作理由
        [IpAddress]    nvarchar(60)   NULL,
        [CreateDate]   datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Sys_AuditTrail_Target ON [Sys_AuditTrail]([TargetType], [TargetId], [CreateDate] DESC);
    CREATE INDEX IX_Sys_AuditTrail_User   ON [Sys_AuditTrail]([UserId], [CreateDate] DESC);
END
GO

/* --- A.3 通知引擎（事件 → 规则 → 记录；多渠道经 n8n 分发） --- */
IF OBJECT_ID(N'[Sys_NotifyTemplate]', N'U') IS NULL
CREATE TABLE [Sys_NotifyTemplate] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Code]        nvarchar(60)   NOT NULL,           -- 触发事件类型编码
    [Name]        nvarchar(100)  NOT NULL,
    [Channels]    nvarchar(100)  NULL,               -- system,email,sms,app 逗号分隔
    [TitleTpl]    nvarchar(300)  NULL,
    [ContentTpl]  nvarchar(max)  NULL,
    [Status]      int            NOT NULL DEFAULT(1),
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Sys_NotifyRule]', N'U') IS NULL
CREATE TABLE [Sys_NotifyRule] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TemplateId]    bigint         NOT NULL,
    [EventType]     nvarchar(60)   NOT NULL,         -- 维保到期/校准到期/库存预警/派工 等
    [Condition]     nvarchar(max)  NULL,             -- JSON 触发条件
    [ReceiverRole]  nvarchar(200)  NULL,             -- 接收角色
    [ReceiverUser]  nvarchar(500)  NULL,             -- 接收人 id 列表
    [AdvanceDays]   int            NOT NULL DEFAULT(0),
    [Channels]      nvarchar(100)  NULL,
    [Status]        int            NOT NULL DEFAULT(1),
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Sys_NotifyRecord]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sys_NotifyRecord] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RuleId]      bigint         NULL,
        [EventType]   nvarchar(60)   NULL,
        [Channel]     nvarchar(20)   NULL,
        [ReceiverId]  bigint         NULL,
        [ReceiverName] nvarchar(60)  NULL,
        [Title]       nvarchar(300)  NULL,
        [Content]     nvarchar(max)  NULL,
        [BizType]     nvarchar(60)   NULL,
        [BizId]       bigint         NULL,
        [SendTime]    datetime       NOT NULL DEFAULT(getdate()),
        [IsRead]      bit            NOT NULL DEFAULT(0),
        [ReadTime]    datetime       NULL,
        [SendStatus]  int            NOT NULL DEFAULT(0)  -- 0待发/1成功/2失败
    );
    CREATE INDEX IX_Sys_NotifyRecord_Receiver ON [Sys_NotifyRecord]([ReceiverId], [IsRead], [SendTime] DESC);
END
GO
IF OBJECT_ID(N'[Sys_NotifyPref]', N'U') IS NULL
CREATE TABLE [Sys_NotifyPref] (
    [Id]        bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId]    bigint        NOT NULL,
    [Channels]  nvarchar(100) NULL,                  -- 用户接收偏好
    [Muted]     bit           NOT NULL DEFAULT(0),
    [UpdateDate] datetime     NOT NULL DEFAULT(getdate())
);
GO

/* --- A.4 工作流引擎（通用提交/审核/批准/派发，多模块复用） --- */
IF OBJECT_ID(N'[Wf_Template]', N'U') IS NULL
CREATE TABLE [Wf_Template] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Code]        nvarchar(60)   NOT NULL,
    [Name]        nvarchar(100)  NOT NULL,
    [Module]      nvarchar(60)   NULL,               -- 适用模块
    [NodeConfig]  nvarchar(max)  NULL,               -- 节点配置 JSON
    [Status]      int            NOT NULL DEFAULT(1),
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Wf_Node]', N'U') IS NULL
BEGIN
    CREATE TABLE [Wf_Node] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TemplateId]  bigint         NOT NULL,
        [NodeKey]     nvarchar(60)   NOT NULL,
        [NodeName]    nvarchar(100)  NULL,
        [NodeType]    nvarchar(20)   NULL,           -- submit/review/approve/dispatch
        [ApproveRole] nvarchar(200)  NULL,
        [TimeoutHours] int           NULL,
        [Sort]        int            NOT NULL DEFAULT(0)
    );
    CREATE INDEX IX_Wf_Node_Template ON [Wf_Node]([TemplateId], [Sort]);
END
GO
IF OBJECT_ID(N'[Wf_Instance]', N'U') IS NULL
BEGIN
    CREATE TABLE [Wf_Instance] (
        [Id]           bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TemplateId]   bigint        NOT NULL,
        [BizType]      nvarchar(60)  NOT NULL,       -- 关联业务表
        [BizId]        bigint        NOT NULL,       -- 关联业务记录
        [CurrentNode]  nvarchar(60)  NULL,
        [Status]       int           NOT NULL DEFAULT(0),  -- 0进行/1完成/2驳回/3撤回
        [InitiatorId]  bigint        NULL,
        [InitiatorName] nvarchar(60) NULL,
        [StartTime]    datetime      NOT NULL DEFAULT(getdate()),
        [EndTime]      datetime      NULL
    );
    CREATE INDEX IX_Wf_Instance_Biz ON [Wf_Instance]([BizType], [BizId]);
END
GO
IF OBJECT_ID(N'[Wf_ApproveLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [Wf_ApproveLog] (
        [Id]          bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [InstanceId]  bigint        NOT NULL,
        [NodeKey]     nvarchar(60)  NULL,
        [ApproverId]  bigint        NULL,
        [ApproverName] nvarchar(60) NULL,
        [Result]      nvarchar(20)  NULL,            -- agree/reject
        [Opinion]     nvarchar(500) NULL,
        [ApproveTime] datetime      NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Wf_ApproveLog_Instance ON [Wf_ApproveLog]([InstanceId], [ApproveTime]);
END
GO

/* --- A.5 电子签名（21 CFR Part 11 / EU Annex 11，封装开源签名服务的落库记录） --- */
IF OBJECT_ID(N'[Sys_ESignature]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sys_ESignature] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BizType]     nvarchar(60)   NOT NULL,
        [BizId]       bigint         NOT NULL,
        [SignerId]    bigint         NULL,
        [SignerName]  nvarchar(60)   NULL,
        [SignMeaning] nvarchar(100)  NULL,           -- 签署含义（编制/复核/批准）
        [SignHash]    nvarchar(256)  NULL,           -- 内容哈希
        [Signature]   nvarchar(max)  NULL,           -- 签名值
        [SignTime]    datetime       NOT NULL DEFAULT(getdate()),
        [IpAddress]   nvarchar(60)   NULL
    );
    CREATE INDEX IX_Sys_ESignature_Biz ON [Sys_ESignature]([BizType], [BizId]);
END
GO

/* --- A.5 报表定义 + 批量导入日志 --- */
IF OBJECT_ID(N'[Sys_ReportDef]', N'U') IS NULL
CREATE TABLE [Sys_ReportDef] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Code]        nvarchar(60)   NOT NULL,
    [Name]        nvarchar(100)  NOT NULL,
    [Module]      nvarchar(60)   NULL,
    [QueryDef]    nvarchar(max)  NULL,               -- 查询条件/字段 JSON
    [ChartDef]    nvarchar(max)  NULL,               -- 图表配置 JSON
    [OwnerId]     bigint         NULL,
    [IsPublic]    bit            NOT NULL DEFAULT(0),
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Sys_ImportLog]', N'U') IS NULL
CREATE TABLE [Sys_ImportLog] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BizType]     nvarchar(60)   NOT NULL,
    [FileName]    nvarchar(255)  NULL,
    [TotalCount]  int            NOT NULL DEFAULT(0),
    [SuccessCount] int           NOT NULL DEFAULT(0),
    [FailCount]   int            NOT NULL DEFAULT(0),
    [SkipCount]   int            NOT NULL DEFAULT(0),
    [ErrorDetail] nvarchar(max)  NULL,
    [OperatorId]  bigint         NULL,
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
PRINT '==== WP-A platform tables ready ====';
GO

/* =============================================================================
   WP-B 设备档案完善（URS 501-532）
   ============================================================================= */

/* 文档版本控制：给现有 Sys_Attachment 增列（幂等） */
IF COL_LENGTH(N'Sys_Attachment', N'Version') IS NULL
    ALTER TABLE [Sys_Attachment] ADD [Version] int NOT NULL DEFAULT(1);
GO
IF COL_LENGTH(N'Sys_Attachment', N'VersionGroup') IS NULL
    ALTER TABLE [Sys_Attachment] ADD [VersionGroup] nvarchar(64) NULL;
GO
IF COL_LENGTH(N'Sys_Attachment', N'IsLatest') IS NULL
    ALTER TABLE [Sys_Attachment] ADD [IsLatest] bit NOT NULL DEFAULT(1);
GO

/* FAT/SAT 验收 + 问题跟踪 */
IF OBJECT_ID(N'[Facility_Acceptance]', N'U') IS NULL
CREATE TABLE [Facility_Acceptance] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BillNo]        nvarchar(60)   NULL,
    [FacilityId]    bigint         NOT NULL,
    [AcceptType]    nvarchar(10)   NULL,             -- FAT/SAT
    [AppearanceOK]  bit            NULL,
    [QtyOK]         bit            NULL,
    [DocOK]         bit            NULL,
    [FunctionOK]    bit            NULL,
    [Result]        int            NOT NULL DEFAULT(0),  -- 0待验/1通过/2不通过
    [AcceptDate]    datetime       NULL,
    [Acceptor]      nvarchar(60)   NULL,
    [Remark]        nvarchar(500)  NULL,
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Facility_AcceptanceIssue]', N'U') IS NULL
BEGIN
    CREATE TABLE [Facility_AcceptanceIssue] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [AcceptId]    bigint         NOT NULL,
        [IssueDesc]   nvarchar(500)  NULL,
        [Solution]    nvarchar(500)  NULL,
        [Owner]       nvarchar(60)   NULL,
        [Status]      int            NOT NULL DEFAULT(0),
        [CloseDate]   datetime       NULL
    );
    CREATE INDEX IX_Facility_AcceptanceIssue_Accept ON [Facility_AcceptanceIssue]([AcceptId]);
END
GO

/* 设备盘点（计划 + 明细） */
IF OBJECT_ID(N'[Facility_StockCheck]', N'U') IS NULL
CREATE TABLE [Facility_StockCheck] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PlanNo]      nvarchar(60)   NULL,
    [PlanName]    nvarchar(200)  NULL,
    [PlanDate]    datetime       NULL,
    [Owner]       nvarchar(60)   NULL,
    [Status]      int            NOT NULL DEFAULT(0),  -- 0计划/1执行中/2完成
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Facility_StockCheckSub]', N'U') IS NULL
BEGIN
    CREATE TABLE [Facility_StockCheckSub] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MainId]      bigint         NOT NULL,
        [FacilityId]  bigint         NOT NULL,
        [RealStatus]  nvarchar(40)   NULL,           -- 实盘状态
        [DiffDesc]    nvarchar(300)  NULL,
        [CheckTime]   datetime       NULL,
        [Checker]     nvarchar(60)   NULL
    );
    CREATE INDEX IX_Facility_StockCheckSub_Main ON [Facility_StockCheckSub]([MainId]);
END
GO

/* 标签（二维码/条码；RFID 按扫码处理） */
IF OBJECT_ID(N'[Facility_Label]', N'U') IS NULL
BEGIN
    CREATE TABLE [Facility_Label] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FacilityId]  bigint         NOT NULL,
        [LabelType]   nvarchar(20)   NULL,           -- qrcode/barcode/nfc/rfid
        [LabelCode]   nvarchar(120)  NULL,
        [GenTime]     datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Facility_Label_Facility ON [Facility_Label]([FacilityId]);
END
GO

/* 证书/许可时效 */
IF OBJECT_ID(N'[Facility_Cert]', N'U') IS NULL
BEGIN
    CREATE TABLE [Facility_Cert] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FacilityId]  bigint         NOT NULL,
        [CertName]    nvarchar(200)  NULL,
        [Issuer]      nvarchar(200)  NULL,
        [EffectDate]  datetime       NULL,
        [ExpireDate]  datetime       NULL,
        [WarnDays]    int            NOT NULL DEFAULT(30),
        [Status]      int            NOT NULL DEFAULT(1)
    );
    CREATE INDEX IX_Facility_Cert_Facility ON [Facility_Cert]([FacilityId]);
    CREATE INDEX IX_Facility_Cert_Expire   ON [Facility_Cert]([ExpireDate]);
END
GO

/* 资产卡片 */
IF OBJECT_ID(N'[Facility_AssetCard]', N'U') IS NULL
CREATE TABLE [Facility_AssetCard] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FacilityId]    bigint         NULL,
    [CardNo]        nvarchar(60)   NULL,
    [AssetName]     nvarchar(200)  NULL,
    [Specs]         nvarchar(200)  NULL,
    [DeptId]        bigint         NULL,
    [Location]      nvarchar(200)  NULL,
    [OriginalValue] decimal(18,2)  NULL,
    [DepreMethod]   nvarchar(60)   NULL,
    [DepreYears]    decimal(18,2)  NULL,
    [NetValue]      decimal(18,2)  NULL,
    [SyncDate]      datetime       NULL,             -- 与财务系统同步时间（n8n/外部）
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO

/* 随机配件 */
IF OBJECT_ID(N'[Facility_Accessory]', N'U') IS NULL
BEGIN
    CREATE TABLE [Facility_Accessory] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FacilityId]  bigint         NOT NULL,
        [Name]        nvarchar(200)  NULL,
        [Specs]       nvarchar(200)  NULL,
        [Qty]         decimal(18,4)  NULL,
        [Remark]      nvarchar(300)  NULL
    );
    CREATE INDEX IX_Facility_Accessory_Facility ON [Facility_Accessory]([FacilityId]);
END
GO

/* 润滑标准 + 润滑记录 */
IF OBJECT_ID(N'[Facility_LubeStandard]', N'U') IS NULL
CREATE TABLE [Facility_LubeStandard] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FacilityType]  nvarchar(60)   NULL,
    [LubePart]      nvarchar(100)  NULL,
    [RecommendOil]  nvarchar(100)  NULL,
    [CycleDays]     int            NULL,
    [Remark]        nvarchar(300)  NULL
);
GO
IF OBJECT_ID(N'[Facility_LubeRecord]', N'U') IS NULL
BEGIN
    CREATE TABLE [Facility_LubeRecord] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FacilityId]  bigint         NOT NULL,
        [LubePart]    nvarchar(100)  NULL,
        [OilModel]    nvarchar(100)  NULL,
        [CycleDays]   int            NULL,
        [LastDate]    datetime       NULL,
        [Executor]    nvarchar(60)   NULL,
        [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Facility_LubeRecord_Facility ON [Facility_LubeRecord]([FacilityId]);
END
GO
PRINT '==== WP-B facility archive tables ready ====';
GO

/* =============================================================================
   WP-C 设备维修增强（URS 601-610）
   ============================================================================= */
IF OBJECT_ID(N'[Repair_Template]', N'U') IS NULL
CREATE TABLE [Repair_Template] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]        nvarchar(100)  NOT NULL,
    [TmplType]    nvarchar(60)   NULL,
    [FieldConfig] nvarchar(max)  NULL,               -- 字段配置 JSON
    [Status]      int            NOT NULL DEFAULT(1),
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
/* 单工单关联多设备 + 费用分摊 */
IF OBJECT_ID(N'[Facility_RepairBillFacility]', N'U') IS NULL
BEGIN
    CREATE TABLE [Facility_RepairBillFacility] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MainId]      bigint         NOT NULL,        -- Facility_RepairBillMain.Id
        [FacilityId]  bigint         NOT NULL,
        [ShareRatio]  decimal(9,4)   NULL,            -- 分摊比例
        [ShareAmount] decimal(18,2)  NULL             -- 分摊金额
    );
    CREATE INDEX IX_Repair_BillFacility_Main ON [Facility_RepairBillFacility]([MainId]);
END
GO
IF OBJECT_ID(N'[Facility_RepairCost]', N'U') IS NULL
BEGIN
    CREATE TABLE [Facility_RepairCost] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MainId]      bigint         NOT NULL,
        [CostType]    nvarchar(60)   NULL,            -- 人工/备件/外委
        [Amount]      decimal(18,2)  NULL,
        [ShareRule]   nvarchar(60)   NULL,            -- 平均/价值比例/工时权重
        [Remark]      nvarchar(300)  NULL
    );
    CREATE INDEX IX_Repair_Cost_Main ON [Facility_RepairCost]([MainId]);
END
GO
/* 报警规则 + 报警记录（异常自动生成预防性工单） */
IF OBJECT_ID(N'[Alarm_Rule]', N'U') IS NULL
CREATE TABLE [Alarm_Rule] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FacilityId]    bigint         NULL,
    [FacilityType]  nvarchar(60)   NULL,
    [ParamName]     nvarchar(100)  NULL,
    [MaxThreshold]  decimal(18,4)  NULL,
    [MinThreshold]  decimal(18,4)  NULL,
    [AlarmLevel]    nvarchar(20)   NULL,             -- 一般/严重/紧急
    [LinkAction]    nvarchar(100)  NULL,             -- 关联动作（生成工单等）
    [Status]        int            NOT NULL DEFAULT(1),
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Alarm_Record]', N'U') IS NULL
BEGIN
    CREATE TABLE [Alarm_Record] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RuleId]      bigint         NULL,
        [FacilityId]  bigint         NULL,
        [AlarmTime]   datetime       NOT NULL DEFAULT(getdate()),
        [ParamValue]  decimal(18,4)  NULL,
        [AlarmLevel]  nvarchar(20)   NULL,
        [HandleStatus] int           NOT NULL DEFAULT(0),
        [RepairBillId] bigint        NULL            -- 关联生成的维修工单
    );
    CREATE INDEX IX_Alarm_Record_Facility ON [Alarm_Record]([FacilityId], [AlarmTime] DESC);
END
GO
/* 维修知识库（故障现象/原因/方案/备件） */
IF OBJECT_ID(N'[Repair_Knowledge]', N'U') IS NULL
CREATE TABLE [Repair_Knowledge] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FacilityType]  nvarchar(60)   NULL,
    [FacilityId]    bigint         NULL,
    [Symptom]       nvarchar(500)  NULL,             -- 故障现象
    [Cause]         nvarchar(500)  NULL,             -- 故障原因
    [Analysis]      nvarchar(max)  NULL,
    [Solution]      nvarchar(max)  NULL,
    [PartChange]    nvarchar(500)  NULL,
    [Tags]          nvarchar(200)  NULL,
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
PRINT '==== WP-C repair tables ready ====';
GO

/* =============================================================================
   WP-D 点巡检重构（URS 701-706）
   ============================================================================= */
IF OBJECT_ID(N'[Inspect_Standard]', N'U') IS NULL
CREATE TABLE [Inspect_Standard] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StdNo]         nvarchar(60)   NULL,
    [FacilityId]    bigint         NULL,
    [FacilityName]  nvarchar(200)  NULL,
    [CycleType]     nvarchar(20)   NULL,             -- 巡检周期
    [Status]        int            NOT NULL DEFAULT(0),  -- 0草稿/1待审/2已审批
    [Maker]         nvarchar(60)   NULL,
    [Checker]       nvarchar(60)   NULL,
    [CheckDate]     datetime       NULL,
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Inspect_StandardSub]', N'U') IS NULL
BEGIN
    CREATE TABLE [Inspect_StandardSub] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MainId]      bigint         NOT NULL,
        [ItemName]    nvarchar(200)  NULL,
        [Method]      nvarchar(200)  NULL,
        [Standard]    nvarchar(200)  NULL,
        [Sort]        int            NOT NULL DEFAULT(0)
    );
    CREATE INDEX IX_Inspect_StandardSub_Main ON [Inspect_StandardSub]([MainId]);
END
GO
IF OBJECT_ID(N'[Inspect_Plan]', N'U') IS NULL
BEGIN
    CREATE TABLE [Inspect_Plan] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PlanNo]      nvarchar(60)   NULL,
        [StandardId]  bigint         NOT NULL,
        [Executor]    nvarchar(60)   NULL,
        [PlanDate]    datetime       NULL,
        [ConfirmStatus] int          NOT NULL DEFAULT(0),  -- 推送确认状态
        [Status]      int            NOT NULL DEFAULT(0),
        [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Inspect_Plan_Date ON [Inspect_Plan]([PlanDate]);
END
GO
IF OBJECT_ID(N'[Inspect_Record]', N'U') IS NULL
BEGIN
    CREATE TABLE [Inspect_Record] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RecordNo]    nvarchar(60)   NULL,
        [PlanId]      bigint         NULL,
        [FacilityId]  bigint         NULL,
        [Executor]    nvarchar(60)   NULL,
        [ExecTime]    datetime       NULL,
        [Result]      int            NOT NULL DEFAULT(0),  -- 0正常/1异常
        [Remark]      nvarchar(500)  NULL,
        [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Inspect_Record_Plan ON [Inspect_Record]([PlanId]);
END
GO
IF OBJECT_ID(N'[Inspect_Disposal]', N'U') IS NULL
BEGIN
    CREATE TABLE [Inspect_Disposal] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RecordId]    bigint         NOT NULL,
        [DisposalType] nvarchar(40)  NULL,           -- 立即维修/计划检修/停产窗口/观察监控/无需处理
        [LinkBillId]  bigint         NULL,           -- 后续工单 id
        [LinkPlanId]  bigint         NULL,           -- 后续计划 id
        [Remark]      nvarchar(500)  NULL,
        [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Inspect_Disposal_Record ON [Inspect_Disposal]([RecordId]);
END
GO
PRINT '==== WP-D inspection tables ready ====';
GO

/* =============================================================================
   WP-E 设备维保增强（URS 801-809）
   ============================================================================= */
IF OBJECT_ID(N'[Maint_Standard]', N'U') IS NULL
CREATE TABLE [Maint_Standard] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StdNo]         nvarchar(60)   NULL,
    [FacilityId]    bigint         NULL,
    [FacilityName]  nvarchar(200)  NULL,
    [FacilityType]  nvarchar(60)   NULL,
    [CycleType]     nvarchar(20)   NULL,             -- 日常/月度/年度
    [EntrustType]   nvarchar(20)   NULL,             -- 自维/外委
    [Status]        int            NOT NULL DEFAULT(1),
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Maint_StandardSub]', N'U') IS NULL
BEGIN
    CREATE TABLE [Maint_StandardSub] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MainId]      bigint         NOT NULL,
        [ItemName]    nvarchar(200)  NULL,
        [SpareId]     bigint         NULL,           -- 消耗备件
        [SpareQty]    decimal(18,4)  NULL,
        [Sort]        int            NOT NULL DEFAULT(0)
    );
    CREATE INDEX IX_Maint_StandardSub_Main ON [Maint_StandardSub]([MainId]);
END
GO
IF OBJECT_ID(N'[Maint_DelayApply]', N'U') IS NULL
BEGIN
    CREATE TABLE [Maint_DelayApply] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BizType]     nvarchar(40)   NULL,           -- 计划/工单
        [BizId]       bigint         NOT NULL,
        [OldDate]     datetime       NULL,
        [NewDate]     datetime       NULL,
        [Reason]      nvarchar(500)  NULL,
        [ApplyUser]   nvarchar(60)   NULL,
        [ApproveStatus] int          NOT NULL DEFAULT(0),
        [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Maint_DelayApply_Biz ON [Maint_DelayApply]([BizType], [BizId]);
END
GO
IF OBJECT_ID(N'[Maint_Qualification]', N'U') IS NULL
BEGIN
    CREATE TABLE [Maint_Qualification] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [QualType]    nvarchar(60)   NULL,           -- 安全合格证/使用许可证/保修证
        [FacilityId]  bigint         NULL,
        [EffectDate]  datetime       NULL,
        [ExpireDate]  datetime       NULL,
        [WarnDays]    int            NOT NULL DEFAULT(30),
        [Status]      int            NOT NULL DEFAULT(1)
    );
    CREATE INDEX IX_Maint_Qualification_Expire ON [Maint_Qualification]([ExpireDate]);
END
GO
PRINT '==== WP-E maintenance tables ready ====';
GO

/* =============================================================================
   WP-F 特种设备管理（URS 901-905）【全新】
   ============================================================================= */
IF OBJECT_ID(N'[Special_Equipment]', N'U') IS NULL
CREATE TABLE [Special_Equipment] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [EquipCode]     nvarchar(60)   NOT NULL,
    [Category]      nvarchar(60)   NULL,             -- 压力容器/电梯/起重机械等
    [RegisterNo]    nvarchar(80)   NULL,             -- 注册登记号
    [UseCertNo]     nvarchar(80)   NULL,             -- 使用证号
    [DesignLife]    decimal(9,2)   NULL,             -- 设计使用年限
    [SafetyLevel]   nvarchar(40)   NULL,             -- 安全状况等级
    [NextInspectDate] datetime     NULL,             -- 下次法定检验日期
    [InspectOrg]    nvarchar(200)  NULL,             -- 检验机构
    [FacilityId]    bigint         NULL,             -- 关联设备档案
    [ExtJson]       nvarchar(max)  NULL,             -- 按类别自定义扩展字段
    [Status]        int            NOT NULL DEFAULT(1),
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
IF COL_LENGTH(N'Special_Equipment', N'EquipCode') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Special_Equipment_Code')
    CREATE UNIQUE INDEX UX_Special_Equipment_Code ON [Special_Equipment]([EquipCode]);
GO
IF OBJECT_ID(N'[Special_InspectPlan]', N'U') IS NULL
BEGIN
    CREATE TABLE [Special_InspectPlan] (
        [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PlanNo]        nvarchar(60)   NULL,
        [EquipId]       bigint         NOT NULL,
        [CycleMonths]   int            NULL,
        [LastInspectDate] datetime     NULL,
        [NextInspectDate] datetime     NULL,
        [Status]        int            NOT NULL DEFAULT(0),  -- 0待检/1在检/2超期
        [Owner]         nvarchar(60)   NULL,
        [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Special_InspectPlan_Equip ON [Special_InspectPlan]([EquipId]);
    CREATE INDEX IX_Special_InspectPlan_Next  ON [Special_InspectPlan]([NextInspectDate]);
END
GO
IF OBJECT_ID(N'[Special_InspectRecord]', N'U') IS NULL
BEGIN
    CREATE TABLE [Special_InspectRecord] (
        [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EquipId]       bigint         NOT NULL,
        [InspectOrg]    nvarchar(200)  NULL,
        [InspectDate]   datetime       NULL,
        [ReportFile]    nvarchar(500)  NULL,
        [Rectification] nvarchar(500)  NULL,         -- 整改通知
        [ReInspect]     nvarchar(500)  NULL,         -- 复检记录
        [Conclusion]    nvarchar(200)  NULL,
        [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Special_InspectRecord_Equip ON [Special_InspectRecord]([EquipId]);
END
GO
PRINT '==== WP-F special equipment tables ready ====';
GO

/* =============================================================================
   WP-G 安全附件管理（URS 1001-1006）【全新】
   ============================================================================= */
IF OBJECT_ID(N'[Safety_Accessory]', N'U') IS NULL
CREATE TABLE [Safety_Accessory] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [AccCode]       nvarchar(60)   NOT NULL,
    [FacilityId]    bigint         NULL,             -- 所属设备
    [Model]         nvarchar(100)  NULL,
    [SetPressure]   nvarchar(60)   NULL,             -- 整定压力
    [CheckRange]    nvarchar(100)  NULL,             -- 检定范围
    [LastCheckDate] datetime       NULL,
    [CheckCycle]    int            NULL,
    [CheckOrg]      nvarchar(200)  NULL,
    [Status]        int            NOT NULL DEFAULT(1),  -- 合格/禁用/更换
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Safety_CheckPlan]', N'U') IS NULL
BEGIN
    CREATE TABLE [Safety_CheckPlan] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [AccId]       bigint         NOT NULL,
        [PlanDate]    datetime       NULL,
        [Owner]       nvarchar(60)   NULL,           -- 送检责任人
        [Status]      int            NOT NULL DEFAULT(0),
        [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Safety_CheckPlan_Acc ON [Safety_CheckPlan]([AccId]);
END
GO
IF OBJECT_ID(N'[Safety_CheckRecord]', N'U') IS NULL
BEGIN
    CREATE TABLE [Safety_CheckRecord] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [AccId]       bigint         NOT NULL,
        [SendDate]    datetime       NULL,
        [TakeDate]    datetime       NULL,
        [CheckResult] nvarchar(60)   NULL,
        [NextCheckDate] datetime     NULL,
        [Remark]      nvarchar(300)  NULL,
        [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Safety_CheckRecord_Acc ON [Safety_CheckRecord]([AccId]);
END
GO
PRINT '==== WP-G safety accessory tables ready ====';
GO

/* =============================================================================
   WP-H 计量器具管理（URS 1101-1117）【全新 · GMP 合规】
   ============================================================================= */
IF OBJECT_ID(N'[Meter]', N'U') IS NULL
CREATE TABLE [Meter] (
    [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MeterCode]     nvarchar(60)   NOT NULL,         -- 唯一编号（自动）
    [Name]          nvarchar(200)  NULL,
    [Model]         nvarchar(100)  NULL,
    [Category]      nvarchar(60)   NULL,
    [Accuracy]      nvarchar(60)   NULL,             -- 精度等级
    [Range]         nvarchar(100)  NULL,             -- 量程
    [DeptId]        bigint         NULL,
    [Location]      nvarchar(200)  NULL,
    [Keeper]        nvarchar(60)   NULL,             -- 保管人
    [Status]        int            NOT NULL DEFAULT(1),  -- 在用/送检/封存/报废
    [CalibCycle]    int            NULL,             -- 校准周期(天)
    [LedgerJson]    nvarchar(max)  NULL,             -- 自定义台账模板字段
    [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
);
GO
IF COL_LENGTH(N'Meter', N'MeterCode') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Meter_Code')
    CREATE UNIQUE INDEX UX_Meter_Code ON [Meter]([MeterCode]);
GO
IF OBJECT_ID(N'[Meter_InOut]', N'U') IS NULL
BEGIN
    CREATE TABLE [Meter_InOut] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MeterId]     bigint         NOT NULL,
        [IoType]      int            NOT NULL,        -- 1入/2出
        [IoTime]      datetime       NOT NULL DEFAULT(getdate()),
        [Operator]    nvarchar(60)   NULL,
        [Remark]      nvarchar(300)  NULL
    );
    CREATE INDEX IX_Meter_InOut_Meter ON [Meter_InOut]([MeterId], [IoTime] DESC);
END
GO
IF OBJECT_ID(N'[Meter_CalibPlan]', N'U') IS NULL
BEGIN
    CREATE TABLE [Meter_CalibPlan] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MeterId]     bigint         NOT NULL,
        [LastCalibDate] datetime     NULL,
        [NextCalibDate] datetime     NULL,
        [Status]      int            NOT NULL DEFAULT(0),
        [Executor]    nvarchar(100)  NULL,           -- 内部计量员/外部服务商
        [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Meter_CalibPlan_Meter ON [Meter_CalibPlan]([MeterId]);
    CREATE INDEX IX_Meter_CalibPlan_Next  ON [Meter_CalibPlan]([NextCalibDate]);
END
GO
IF OBJECT_ID(N'[Meter_CalibRecord]', N'U') IS NULL
BEGIN
    CREATE TABLE [Meter_CalibRecord] (
        [Id]            bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MeterId]       bigint         NOT NULL,
        [CalibDate]     datetime       NULL,
        [Executor]      nvarchar(100)  NULL,
        [Regulation]    nvarchar(200)  NULL,         -- 依据规程
        [EnvCondition]  nvarchar(200)  NULL,         -- 环境条件
        [StdDevice]     nvarchar(200)  NULL,         -- 标准器
        [MeasureData]   nvarchar(max)  NULL,         -- 测量数据
        [Uncertainty]   nvarchar(100)  NULL,         -- 不确定度
        [Conclusion]    nvarchar(40)   NULL,         -- 合格/不合格/限制使用
        [ValidDate]     datetime       NULL,         -- 有效期
        [CertFile]      nvarchar(500)  NULL,         -- 证书附件
        [Reviewer]      nvarchar(60)   NULL,         -- 复核人
        [ReviewDate]    datetime       NULL,
        [IsEffective]   bit            NOT NULL DEFAULT(0),  -- 复核确认后生效
        [CreateDate]    datetime       NOT NULL DEFAULT(getdate())
    );
    CREATE INDEX IX_Meter_CalibRecord_Meter ON [Meter_CalibRecord]([MeterId], [CalibDate] DESC);
END
GO
IF OBJECT_ID(N'[Meter_SendOut]', N'U') IS NULL
CREATE TABLE [Meter_SendOut] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ApplyNo]     nvarchar(60)   NULL,
    [SendDate]    datetime       NULL,
    [ServiceOrg]  nvarchar(200)  NULL,              -- 接收服务商
    [ApproveStatus] int          NOT NULL DEFAULT(0),
    [Applicant]   nvarchar(60)   NULL,
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Meter_SendOutSub]', N'U') IS NULL
BEGIN
    CREATE TABLE [Meter_SendOutSub] (
        [Id]        bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MainId]    bigint NOT NULL,
        [MeterId]   bigint NOT NULL
    );
    CREATE INDEX IX_Meter_SendOutSub_Main ON [Meter_SendOutSub]([MainId]);
END
GO
PRINT '==== WP-H meter tables ready ====';
GO

/* =============================================================================
   WP-I 备件管理增强（URS 1201-1210）
   ============================================================================= */
/* 库存阈值/预约扩展（现有 Spare_NowQuan 增列） */
IF COL_LENGTH(N'Spare_NowQuan', N'SafeStock') IS NULL
    ALTER TABLE [Spare_NowQuan] ADD [SafeStock] decimal(18,4) NULL;
GO
IF COL_LENGTH(N'Spare_NowQuan', N'ReserveQty') IS NULL
    ALTER TABLE [Spare_NowQuan] ADD [ReserveQty] decimal(18,4) NULL;
GO
IF OBJECT_ID(N'[Spare_DemandPlan]', N'U') IS NULL
CREATE TABLE [Spare_DemandPlan] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SpareId]     bigint         NOT NULL,
    [DemandType]  nvarchar(20)   NULL,              -- 即时/长期
    [PredictBase] nvarchar(200)  NULL,              -- 预测依据
    [PlanQty]     decimal(18,4)  NULL,
    [GenTime]     datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Spare_LifeCycle]', N'U') IS NULL
BEGIN
    CREATE TABLE [Spare_LifeCycle] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SpareId]     bigint         NOT NULL,
        [FacilityId]  bigint         NULL,           -- 安装设备
        [InstallDate] datetime       NULL,
        [ReplaceDate] datetime       NULL,
        [UsedHours]   decimal(18,2)  NULL,
        [PredictLife] decimal(18,2)  NULL            -- 预测剩余寿命
    );
    CREATE INDEX IX_Spare_LifeCycle_Spare ON [Spare_LifeCycle]([SpareId]);
END
GO
IF OBJECT_ID(N'[Spare_StockCheck]', N'U') IS NULL
CREATE TABLE [Spare_StockCheck] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PlanNo]      nvarchar(60)   NULL,
    [PlanDate]    datetime       NULL,
    [Owner]       nvarchar(60)   NULL,
    [Status]      int            NOT NULL DEFAULT(0),
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
IF OBJECT_ID(N'[Spare_StockCheckSub]', N'U') IS NULL
BEGIN
    CREATE TABLE [Spare_StockCheckSub] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MainId]      bigint         NOT NULL,
        [SpareId]     bigint         NOT NULL,
        [BookQty]     decimal(18,4)  NULL,           -- 账面
        [RealQty]     decimal(18,4)  NULL,           -- 实盘
        [DiffQty]     decimal(18,4)  NULL
    );
    CREATE INDEX IX_Spare_StockCheckSub_Main ON [Spare_StockCheckSub]([MainId]);
END
GO
PRINT '==== WP-I spare tables ready ====';
GO

/* =============================================================================
   WP-J 能源管理（URS 1301-1306）【采集由 n8n 写入，本系统只建存储 + 读库】
   ============================================================================= */
IF OBJECT_ID(N'[Energy_Point]', N'U') IS NULL
CREATE TABLE [Energy_Point] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PointCode]   nvarchar(60)   NOT NULL,          -- 计量点编号
    [MediaType]   nvarchar(20)   NULL,              -- 电/水/蒸汽
    [MeterModel]  nvarchar(100)  NULL,
    [Protocol]    nvarchar(40)   NULL,              -- OPC/Modbus 等（n8n 采集用）
    [SampleRate]  int            NULL,              -- 采集频率(秒)
    [DeptId]      bigint         NULL,
    [Status]      int            NOT NULL DEFAULT(1),
    [CreateDate]  datetime       NOT NULL DEFAULT(getdate())
);
GO
IF COL_LENGTH(N'Energy_Point', N'PointCode') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Energy_Point_Code')
    CREATE UNIQUE INDEX UX_Energy_Point_Code ON [Energy_Point]([PointCode]);
GO
IF OBJECT_ID(N'[Energy_RealtimeData]', N'U') IS NULL
BEGIN
    CREATE TABLE [Energy_RealtimeData] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PointId]     bigint         NOT NULL,
        [Ts]          datetime       NOT NULL,        -- 时间戳
        [InstValue]   decimal(18,4)  NULL,            -- 瞬时值
        [AccuValue]   decimal(18,4)  NULL             -- 累计值
    );
    CREATE INDEX IX_Energy_RealtimeData_Point ON [Energy_RealtimeData]([PointId], [Ts] DESC);
END
GO
IF OBJECT_ID(N'[Energy_Summary]', N'U') IS NULL
BEGIN
    CREATE TABLE [Energy_Summary] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PointId]     bigint         NOT NULL,
        [Dimension]   nvarchar(10)   NULL,            -- hour/day/month/year
        [DeptId]      bigint         NULL,
        [StatDate]    datetime       NULL,
        [SummaryValue] decimal(18,4) NULL
    );
    CREATE INDEX IX_Energy_Summary_Point ON [Energy_Summary]([PointId], [Dimension], [StatDate]);
END
GO
IF OBJECT_ID(N'[Energy_AlarmRule]', N'U') IS NULL
CREATE TABLE [Energy_AlarmRule] (
    [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PointId]     bigint         NOT NULL,
    [Threshold]   decimal(18,4)  NULL,
    [AlarmLevel]  nvarchar(20)   NULL,
    [NotifyWay]   nvarchar(100)  NULL,
    [NotifyUser]  nvarchar(500)  NULL,
    [Status]      int            NOT NULL DEFAULT(1)
);
GO
IF OBJECT_ID(N'[Energy_AlarmRecord]', N'U') IS NULL
BEGIN
    CREATE TABLE [Energy_AlarmRecord] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PointId]     bigint         NOT NULL,
        [AlarmTime]   datetime       NOT NULL DEFAULT(getdate()),
        [AlarmLevel]  nvarchar(20)   NULL,
        [AlarmValue]  decimal(18,4)  NULL,
        [HandleStatus] int           NOT NULL DEFAULT(0)
    );
    CREATE INDEX IX_Energy_AlarmRecord_Point ON [Energy_AlarmRecord]([PointId], [AlarmTime] DESC);
END
GO
IF OBJECT_ID(N'[Energy_RunTime]', N'U') IS NULL
BEGIN
    CREATE TABLE [Energy_RunTime] (
        [Id]          bigint         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FacilityId]  bigint         NOT NULL,
        [WorkSection] nvarchar(100)  NULL,           -- 工段
        [Product]     nvarchar(100)  NULL,
        [PeriodStart] datetime       NULL,
        [PeriodEnd]   datetime       NULL,
        [RunHours]    decimal(18,2)  NULL
    );
    CREATE INDEX IX_Energy_RunTime_Facility ON [Energy_RunTime]([FacilityId]);
END
GO
PRINT '==== WP-J energy tables ready ====';
GO

/* =============================================================================
   菜单种子（新模块菜单先置 Status=0 占位，对应工作包页面交付后置 1）
   ============================================================================= */

/* --- 顶级新模块 --- */
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='special')
    INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
    VALUES (N'特种设备','special',NULL,0,6,0,'shield-alert');
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='safety')
    INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
    VALUES (N'安全附件','safety',NULL,0,7,0,'life-buoy');
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='meter')
    INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
    VALUES (N'计量器具','meter',NULL,0,8,0,'ruler');
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='energy')
    INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
    VALUES (N'能源管理','energy',NULL,0,9,0,'zap');
GO

/* --- 子菜单（均 Status=0 占位） --- */
-- 特种设备
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'特种设备台账','special-equip','/Special_Equipment/Index',[Id],1,0,NULL FROM [Sys_Module] WHERE [Code]='special'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='special-equip');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'法定检验计划','special-plan','/Special_InspectPlan/Index',[Id],2,0,NULL FROM [Sys_Module] WHERE [Code]='special'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='special-plan');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'检验记录','special-record','/Special_InspectRecord/Index',[Id],3,0,NULL FROM [Sys_Module] WHERE [Code]='special'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='special-record');
GO
-- 安全附件
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'安全附件台账','safety-acc','/Safety_Accessory/Index',[Id],1,0,NULL FROM [Sys_Module] WHERE [Code]='safety'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='safety-acc');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'检定计划','safety-plan','/Safety_CheckPlan/Index',[Id],2,0,NULL FROM [Sys_Module] WHERE [Code]='safety'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='safety-plan');
GO
-- 计量器具
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'器具档案','meter-archive','/Meter/Index',[Id],1,0,NULL FROM [Sys_Module] WHERE [Code]='meter'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='meter-archive');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'校准计划','meter-calib','/Meter_CalibPlan/Index',[Id],2,0,NULL FROM [Sys_Module] WHERE [Code]='meter'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='meter-calib');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'校准记录','meter-record','/Meter_CalibRecord/Index',[Id],3,0,NULL FROM [Sys_Module] WHERE [Code]='meter'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='meter-record');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'送外检','meter-sendout','/Meter_SendOut/Index',[Id],4,0,NULL FROM [Sys_Module] WHERE [Code]='meter'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='meter-sendout');
GO
-- 能源管理
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'实时监控','energy-dash','/Energy_Point/Dashboard',[Id],1,0,NULL FROM [Sys_Module] WHERE [Code]='energy'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='energy-dash');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'计量点配置','energy-point','/Energy_Point/Index',[Id],2,0,NULL FROM [Sys_Module] WHERE [Code]='energy'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='energy-point');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'能耗统计','energy-stat','/Energy_Summary/Index',[Id],3,0,NULL FROM [Sys_Module] WHERE [Code]='energy'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='energy-stat');
GO
-- 系统管理新增：审计日志 / 通知中心 / 用户组 / 工作流（均 Status=0 占位）
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'审计日志','sys-audit','/Sys_OperationLog/Index',[Id],8,0,NULL FROM [Sys_Module] WHERE [Code]='system'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='sys-audit');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'用户组','sys-usergroup','/Sys_UserGroup/Index',[Id],9,0,NULL FROM [Sys_Module] WHERE [Code]='system'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='sys-usergroup');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'通知中心','sys-notify','/Sys_NotifyRecord/Index',[Id],10,0,NULL FROM [Sys_Module] WHERE [Code]='system'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='sys-notify');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'工作流模板','sys-workflow','/Wf_Template/Index',[Id],11,0,NULL FROM [Sys_Module] WHERE [Code]='system'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='sys-workflow');
GO

/* 把新增菜单绑定给 admin 角色（页面虽 Status=0 隐藏，但权限先就位） */
INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin'), m.[Id]
  FROM [Sys_Module] m
 WHERE m.[Code] IN ('special','safety','meter','energy',
        'special-equip','special-plan','special-record',
        'safety-acc','safety-plan',
        'meter-archive','meter-calib','meter-record','meter-sendout',
        'energy-dash','energy-point','energy-stat',
        'sys-audit','sys-usergroup','sys-notify','sys-workflow')
   AND NOT EXISTS (SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.RoleId = (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin')
          AND rm.ModuleId = m.[Id]);
GO

/* n8n 集成配置项（OCR/AI/通知 webhook） */
INSERT INTO [Sys_Setting] ([Group],[Key],[Value],[ValueType],[Title],[Sort])
SELECT 'n8n','n8nOcrWebhook','','string',N'n8n OCR/智能解析 Webhook',1
 WHERE NOT EXISTS (SELECT 1 FROM [Sys_Setting] WHERE [Key]='n8nOcrWebhook');
INSERT INTO [Sys_Setting] ([Group],[Key],[Value],[ValueType],[Title],[Sort])
SELECT 'n8n','n8nNotifyWebhook','','string',N'n8n 通知分发 Webhook',2
 WHERE NOT EXISTS (SELECT 1 FROM [Sys_Setting] WHERE [Key]='n8nNotifyWebhook');
INSERT INTO [Sys_Setting] ([Group],[Key],[Value],[ValueType],[Title],[Sort])
SELECT 'n8n','n8nAiAgentUrl','','string',N'n8n AI 问答嵌入路径',3
 WHERE NOT EXISTS (SELECT 1 FROM [Sys_Setting] WHERE [Key]='n8nAiAgentUrl');
INSERT INTO [Sys_Setting] ([Group],[Key],[Value],[ValueType],[Title],[Sort])
SELECT 'n8n','n8nApiKey','','string',N'n8n API Key',4
 WHERE NOT EXISTS (SELECT 1 FROM [Sys_Setting] WHERE [Key]='n8nApiKey');
GO
PRINT '==== Menu & setting seeds ready ====';
GO

/* =============================================================================
   版本记录 v2.0.0
   ============================================================================= */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.0.0')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]       = N'URS 对标：全新模块数据库存储设计（DDL 落地）',
           [Content]     =
              N'## 本次范围（仅新增表/列，不动现有 24 表）' + CHAR(10) +
              N'- WP-A 平台底座：用户组/登录日志/账户锁定、审计明细(Sys_AuditTrail)、通知引擎(4表)、工作流引擎(4表)、电子签名、报表定义、导入日志' + CHAR(10) +
              N'- WP-B 设备档案：验收FAT/SAT+问题、盘点(主子)、标签、证书、资产卡片、随机配件、润滑标准/记录；附件增版本号链' + CHAR(10) +
              N'- WP-C 维修：工单模板、多设备分摊、费用明细、报警规则/记录、维修知识库' + CHAR(10) +
              N'- WP-D 点巡检：巡检标准(主子)、计划、记录、异常处置分流' + CHAR(10) +
              N'- WP-E 维保：维保标准(主子)、延期申请、资质监控' + CHAR(10) +
              N'- WP-F 特种设备：台账+检验计划+检验记录' + CHAR(10) +
              N'- WP-G 安全附件：台账+检定计划+检定记录' + CHAR(10) +
              N'- WP-H 计量器具：器具+出入库+校准计划/记录+送外检(主子)' + CHAR(10) +
              N'- WP-I 备件：需求计划+生命周期+盘点(主子)，NowQuan 增安全阈值/预约量' + CHAR(10) +
              N'- WP-J 能源：计量点+时序+汇总+报警规则/记录+运行时长（采集由 n8n 写入）' + CHAR(10) +
              N'## 菜单' + CHAR(10) +
              N'- 新增特种设备/安全附件/计量器具/能源 4 个顶级 + 审计/用户组/通知/工作流，均 Status=0 占位，页面交付后置 1' + CHAR(10) +
              N'- 新增 n8n 集成配置项（OCR/通知/AI/ApiKey）' ,
           [IsCurrent]   = 1,
           [Author]      = N'arbore'
     WHERE [Version] = 'v2.0.0';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.0.0', getdate(),
        N'URS 对标：全新模块数据库存储设计（DDL 落地）',
        N'## 本次范围（仅新增表/列，不动现有 24 表）' + CHAR(10) +
        N'- WP-A 平台底座：用户组/登录日志/账户锁定、审计明细(Sys_AuditTrail)、通知引擎(4表)、工作流引擎(4表)、电子签名、报表定义、导入日志' + CHAR(10) +
        N'- WP-B 设备档案：验收FAT/SAT+问题、盘点(主子)、标签、证书、资产卡片、随机配件、润滑标准/记录；附件增版本号链' + CHAR(10) +
        N'- WP-C 维修：工单模板、多设备分摊、费用明细、报警规则/记录、维修知识库' + CHAR(10) +
        N'- WP-D 点巡检：巡检标准(主子)、计划、记录、异常处置分流' + CHAR(10) +
        N'- WP-E 维保：维保标准(主子)、延期申请、资质监控' + CHAR(10) +
        N'- WP-F 特种设备：台账+检验计划+检验记录' + CHAR(10) +
        N'- WP-G 安全附件：台账+检定计划+检定记录' + CHAR(10) +
        N'- WP-H 计量器具：器具+出入库+校准计划/记录+送外检(主子)' + CHAR(10) +
        N'- WP-I 备件：需求计划+生命周期+盘点(主子)，NowQuan 增安全阈值/预约量' + CHAR(10) +
        N'- WP-J 能源：计量点+时序+汇总+报警规则/记录+运行时长（采集由 n8n 写入）' + CHAR(10) +
        N'## 菜单' + CHAR(10) +
        N'- 新增特种设备/安全附件/计量器具/能源 4 个顶级 + 审计/用户组/通知/工作流，均 Status=0 占位，页面交付后置 1' + CHAR(10) +
        N'- 新增 n8n 集成配置项（OCR/通知/AI/ApiKey）',
        1, N'arbore');
END
GO

PRINT '==== Patch v2.0.0 applied ====';
PRINT 'New module tables created (WP-A ~ WP-J). New menus seeded as Status=0 placeholders.';
GO
