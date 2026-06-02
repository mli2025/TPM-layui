/* =============================================================================
   arbore TPM · Patch v2.1.3 -> v2.1.4
   Target: SQL Server 2017+   Database: [TPM]
   内容（点检计划改为「角色制 + 班次 + 滚动生成」，URS 701-706）：
     1) Inspect_Plan 新增 CycleType(周期) / Shifts(班次,逗号分隔) / EndDate(截止日期)
        - 不再绑定执行人；执行人在提交时由当前登录人回填
        - Status 语义调整为：1=启用(参与滚动生成) / 0=停用
     2) 新增 Inspect_PlanRole（计划-角色关联，按角色分配，当班对应角色人员均可执行）
     3) Inspect_Record 新增 Shift(班次)；执行单按「设备 × 日期 × 班次」逐张生成
   幂等：均带列存在性 / OBJECT_ID 守卫，可重复执行。
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* ---- 1. Inspect_Plan ---- */
IF COL_LENGTH('Inspect_Plan','CycleType') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [CycleType] nvarchar(20) NULL;
GO
IF COL_LENGTH('Inspect_Plan','Shifts') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [Shifts] nvarchar(200) NULL;
GO
IF COL_LENGTH('Inspect_Plan','EndDate') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [EndDate] datetime NULL;
GO
/* 旧数据迁移：周期为空时取所属标准的巡检周期兜底（无则按「日」） */
UPDATE p
   SET p.[CycleType] = ISNULL(NULLIF(s.[CycleType],''), N'日')
  FROM [Inspect_Plan] p
  LEFT JOIN [Inspect_Standard] s ON s.[Id] = p.[StandardId]
 WHERE p.[CycleType] IS NULL OR p.[CycleType] = '';
GO
/* Status 语义切换为启用/停用：历史计划统一置为启用，纳入滚动生成 */
UPDATE [Inspect_Plan] SET [Status] = 1 WHERE [Status] IS NULL OR [Status] <> 1;
GO

/* ---- 2. Inspect_PlanRole ---- */
IF OBJECT_ID(N'[Inspect_PlanRole]', N'U') IS NULL
BEGIN
    CREATE TABLE [Inspect_PlanRole] (
        [Id]     bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PlanId] bigint NOT NULL,
        [RoleId] bigint NOT NULL
    );
    CREATE INDEX IX_Inspect_PlanRole_Plan ON [Inspect_PlanRole]([PlanId]);
    CREATE INDEX IX_Inspect_PlanRole_Role ON [Inspect_PlanRole]([RoleId]);
END
GO

/* ---- 3. Inspect_Record ---- */
IF COL_LENGTH('Inspect_Record','Shift') IS NULL
    ALTER TABLE [Inspect_Record] ADD [Shift] nvarchar(50) NULL;
GO

/* ---- 版本记录 ---- */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.1.4')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]   = N'点检计划改为角色制 + 班次 + 滚动生成',
           [Content] = N'- 点检计划不再绑定执行人，改为按「角色(可多选)」分配，当班对应角色人员均可执行' + CHAR(10) +
                       N'- 计划支持周期(班/日/周/月/季/年)与班次(早/中/夜等)，按「设备×日期×班次」生成执行单' + CHAR(10) +
                       N'- 执行单由后台滚动任务每日补齐当期与漏检；执行人在提交时回填当前登录人' + CHAR(10) +
                       N'- 多人可见同一待执行单，提交时原子防重，避免重复点检' + CHAR(10) +
                       N'- 移动端按角色加载待办，支持扫码/输入设备编码定位待检单',
           [IsCurrent] = 1, [Author] = N'arbore'
     WHERE [Version] = 'v2.1.4';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.1.4', getdate(),
        N'点检计划改为角色制 + 班次 + 滚动生成',
        N'- 点检计划不再绑定执行人，改为按「角色(可多选)」分配，当班对应角色人员均可执行' + CHAR(10) +
        N'- 计划支持周期(班/日/周/月/季/年)与班次(早/中/夜等)，按「设备×日期×班次」生成执行单' + CHAR(10) +
        N'- 执行单由后台滚动任务每日补齐当期与漏检；执行人在提交时回填当前登录人' + CHAR(10) +
        N'- 多人可见同一待执行单，提交时原子防重，避免重复点检' + CHAR(10) +
        N'- 移动端按角色加载待办，支持扫码/输入设备编码定位待检单',
        1, N'arbore');
END
GO

PRINT '==== v2.1.4 patch applied ====';
GO
