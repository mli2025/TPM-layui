/* =============================================================================
   arbore TPM · Patch v2.1.2 -> v2.1.3
   Target: SQL Server 2017+   Database: [TPM]
   内容（点检流程重构，URS 701-706）：
     1) Inspect_Standard 标准去设备化：新增 StdName(标准名称) / FacilityType(适用设备类型) / MakerId(编制人员工Id)
     2) Inspect_Plan 支持多设备×周期：新增 Periods(期数) / ExecutorId(执行人员工Id)
     3) 新增 Inspect_PlanDevice（计划-设备关联，一个计划覆盖多台设备）
     4) Inspect_Record 新增 FacilityName / PlanDate（按设备逐台生成执行单，排程用）
   幂等：均带列存在性 / OBJECT_ID 守卫，可重复执行。
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* ---- 1. Inspect_Standard ---- */
IF COL_LENGTH('Inspect_Standard','StdName') IS NULL
    ALTER TABLE [Inspect_Standard] ADD [StdName] nvarchar(200) NULL;
GO
IF COL_LENGTH('Inspect_Standard','FacilityType') IS NULL
    ALTER TABLE [Inspect_Standard] ADD [FacilityType] nvarchar(100) NULL;
GO
IF COL_LENGTH('Inspect_Standard','MakerId') IS NULL
    ALTER TABLE [Inspect_Standard] ADD [MakerId] bigint NULL;
GO
/* 旧数据迁移：标准名称为空时用原设备名/标准编号兜底 */
UPDATE [Inspect_Standard]
   SET [StdName] = ISNULL(NULLIF([FacilityName],''), [StdNo])
 WHERE [StdName] IS NULL OR [StdName] = '';
GO

/* ---- 2. Inspect_Plan ---- */
IF COL_LENGTH('Inspect_Plan','Periods') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [Periods] int NOT NULL DEFAULT(1);
GO
IF COL_LENGTH('Inspect_Plan','ExecutorId') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [ExecutorId] bigint NULL;
GO

/* ---- 3. Inspect_PlanDevice ---- */
IF OBJECT_ID(N'[Inspect_PlanDevice]', N'U') IS NULL
BEGIN
    CREATE TABLE [Inspect_PlanDevice] (
        [Id]           bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PlanId]       bigint        NOT NULL,
        [FacilityId]   bigint        NOT NULL,
        [FacilityName] nvarchar(200) NULL
    );
    CREATE INDEX IX_Inspect_PlanDevice_Plan ON [Inspect_PlanDevice]([PlanId]);
END
GO

/* ---- 4. Inspect_Record ---- */
IF COL_LENGTH('Inspect_Record','FacilityName') IS NULL
    ALTER TABLE [Inspect_Record] ADD [FacilityName] nvarchar(200) NULL;
GO
IF COL_LENGTH('Inspect_Record','PlanDate') IS NULL
    ALTER TABLE [Inspect_Record] ADD [PlanDate] datetime NULL;
GO

/* ---- 版本记录 ---- */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.1.3')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]   = N'点检流程重构：标准去设备化 + 计划多设备批量生成执行单',
           [Content] = N'- 点检标准改为「点检项组合模板」：新增标准名称/适用设备类型/编制人(员工放大镜)，不再绑定单台设备' + CHAR(10) +
                       N'- 点检计划支持「标准 × 多设备 × 周期」，保存后按每台设备逐张生成点检执行单' + CHAR(10) +
                       N'- 新增 Inspect_PlanDevice 计划设备关联表；Inspect_Record 增设备名与计划日期' + CHAR(10) +
                       N'- PC/移动端执行单统一按记录维度：待执行→逐项填写→提交→异常处置',
           [IsCurrent] = 1, [Author] = N'arbore'
     WHERE [Version] = 'v2.1.3';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.1.3', getdate(),
        N'点检流程重构：标准去设备化 + 计划多设备批量生成执行单',
        N'- 点检标准改为「点检项组合模板」：新增标准名称/适用设备类型/编制人(员工放大镜)，不再绑定单台设备' + CHAR(10) +
        N'- 点检计划支持「标准 × 多设备 × 周期」，保存后按每台设备逐张生成点检执行单' + CHAR(10) +
        N'- 新增 Inspect_PlanDevice 计划设备关联表；Inspect_Record 增设备名与计划日期' + CHAR(10) +
        N'- PC/移动端执行单统一按记录维度：待执行→逐项填写→提交→异常处置',
        1, N'arbore');
END
GO

PRINT '==== v2.1.3 patch applied ====';
GO
