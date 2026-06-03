/* =============================================================================
   arbore TPM · Patch v2.1.4 -> v2.1.5
   Target: SQL Server 2017+   Database: [TPM]
   内容（点检流程回退为「模板制」，URS 701-706）：
     1) 取消点检标准库维护，回到「点检项目(Facility_Item Type=点检) + 点检模板(Facility_TheTemplateMain/Sub Type=点检)」
        - 重新启用菜单 chk-item / chk-template，移除 ins-standard
     2) Inspect_Plan 新增 TemplateId（点检模板Id，替代 StandardId）
     3) Inspect_RecordSub 新增 ControlType / MaxValue / MinValue / Method / Standard（执行单逐项留痕+自动判定）
     4) 执行单生成改为「保存计划即按时间范围全量生成；编辑先清未执行再重生」，移除后台滚动任务
   幂等：均带列存在性 / OBJECT_ID 守卫，可重复执行。
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* ---- 1. Inspect_Plan：新增 TemplateId（兼容历史，保留 StandardId 列不动） ---- */
IF COL_LENGTH('Inspect_Plan','TemplateId') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [TemplateId] bigint NOT NULL CONSTRAINT DF_Inspect_Plan_TemplateId DEFAULT(0);
GO
/* 周期/班次/截止日期列（若未执行 v2.1.4 则补齐） */
IF COL_LENGTH('Inspect_Plan','CycleType') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [CycleType] nvarchar(20) NULL;
GO
IF COL_LENGTH('Inspect_Plan','Shifts') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [Shifts] nvarchar(200) NULL;
GO
IF COL_LENGTH('Inspect_Plan','EndDate') IS NULL
    ALTER TABLE [Inspect_Plan] ADD [EndDate] datetime NULL;
GO

/* ---- 2. Inspect_PlanRole / Inspect_Record.Shift（若未执行 v2.1.4 则补齐） ---- */
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
IF COL_LENGTH('Inspect_Record','Shift') IS NULL
    ALTER TABLE [Inspect_Record] ADD [Shift] nvarchar(50) NULL;
GO

/* ---- 3. Inspect_RecordSub：自动判定所需的控件类型与上下限，方法/标准留痕 ---- */
IF COL_LENGTH('Inspect_RecordSub','ControlType') IS NULL
    ALTER TABLE [Inspect_RecordSub] ADD [ControlType] int NOT NULL CONSTRAINT DF_Inspect_RecordSub_ControlType DEFAULT(0);
GO
IF COL_LENGTH('Inspect_RecordSub','MaxValue') IS NULL
    ALTER TABLE [Inspect_RecordSub] ADD [MaxValue] decimal(18,4) NULL;
GO
IF COL_LENGTH('Inspect_RecordSub','MinValue') IS NULL
    ALTER TABLE [Inspect_RecordSub] ADD [MinValue] decimal(18,4) NULL;
GO
IF COL_LENGTH('Inspect_RecordSub','Method') IS NULL
    ALTER TABLE [Inspect_RecordSub] ADD [Method] nvarchar(500) NULL;
GO
IF COL_LENGTH('Inspect_RecordSub','Standard') IS NULL
    ALTER TABLE [Inspect_RecordSub] ADD [Standard] nvarchar(500) NULL;
GO

/* ---- 4. 菜单：启用 点检项目/点检模板，移除 点检标准库 ---- */
/* 4.1 重新挂载/补插 chk-item、chk-template 到「设备点检 inspection」 */
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'点检项目','chk-item','/Facility_CheckItem/Index',[Id],1,1,'note'
  FROM [Sys_Module] WHERE [Code]='inspection'
   AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='chk-item');
GO
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'点检模板','chk-template','/Facility_CheckTemplate/Index',[Id],2,1,'template-1'
  FROM [Sys_Module] WHERE [Code]='inspection'
   AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='chk-template');
GO
/* 若历史存在但被挂到别处/停用，则修正归属、排序、启用 */
UPDATE m SET m.[ParentId] = p.[Id], m.[Sort] = v.[Sort], m.[Status] = 1,
             m.[Url] = v.[Url], m.[Name] = v.[Name]
  FROM [Sys_Module] m
 INNER JOIN [Sys_Module] p ON p.[Code] = 'inspection'
 INNER JOIN (VALUES
        ('chk-item',     1, '/Facility_CheckItem/Index',     N'点检项目'),
        ('chk-template', 2, '/Facility_CheckTemplate/Index', N'点检模板'),
        ('ins-plan',     3, '/Inspect_Plan/Index',           N'点检计划'),
        ('ins-record',   4, '/Inspect_Record/Index',         N'点检执行单')
       ) v([Code],[Sort],[Url],[Name]) ON m.[Code] = v.[Code];
GO

/* 4.2 移除 点检标准库 ins-standard（菜单 + 权限绑定；页面代码保留供历史查阅） */
DELETE ugm FROM [Sys_UserGroupModule] ugm
 INNER JOIN [Sys_Module] m ON m.[Id] = ugm.[ModuleId]
 WHERE m.[Code] = 'ins-standard';
GO
DELETE rm FROM [Sys_RoleModule] rm
 INNER JOIN [Sys_Module] m ON m.[Id] = rm.[ModuleId]
 WHERE m.[Code] = 'ins-standard';
GO
DELETE b FROM [Sys_ModuleButtons] b
 INNER JOIN [Sys_Module] m ON m.[Id] = b.[ModuleId]
 WHERE m.[Code] = 'ins-standard';
GO
DELETE FROM [Sys_Module] WHERE [Code] = 'ins-standard';
GO

/* 4.3 admin 角色授予新菜单 */
INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT (SELECT TOP 1 [Id] FROM [Sys_Role] WHERE [Name] IN (N'admin',N'系统管理员') ORDER BY [Id]), m.[Id]
  FROM [Sys_Module] m
 WHERE m.[Code] IN ('chk-item','chk-template')
   AND NOT EXISTS (SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.RoleId = (SELECT TOP 1 [Id] FROM [Sys_Role] WHERE [Name] IN (N'admin',N'系统管理员') ORDER BY [Id])
          AND rm.ModuleId = m.[Id]);
GO

/* ---- 版本记录 ---- */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.1.5')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]   = N'点检流程回退为模板制：项目+模板，计划选模板，移动端实测值自动判定',
           [Content] = N'- 取消点检标准库，恢复「点检项目 + 点检模板」（Facility_Item / Facility_TheTemplateMain·Sub，Type=点检）' + CHAR(10) +
                       N'- 点检计划改为选「点检模板」+ 设备范围 + 周期(日/周/月/季/年) + 班次 + 起止日期 + 角色' + CHAR(10) +
                       N'- 保存计划即按「设备×日期×班次」全量生成执行单；编辑先清本计划未执行待办再重生（已完成保留）' + CHAR(10) +
                       N'- PC 点检执行单仅查看；移动端按角色加载超期+当期，录入实测值由系统按上下限/是否自动判定合格或异常' + CHAR(10) +
                       N'- 移动端「已完成」分页懒加载；异常仅记录结果，维修单由工作流自动生成',
           [IsCurrent] = 1, [Author] = N'arbore'
     WHERE [Version] = 'v2.1.5';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.1.5', getdate(),
        N'点检流程回退为模板制：项目+模板，计划选模板，移动端实测值自动判定',
        N'- 取消点检标准库，恢复「点检项目 + 点检模板」（Facility_Item / Facility_TheTemplateMain·Sub，Type=点检）' + CHAR(10) +
        N'- 点检计划改为选「点检模板」+ 设备范围 + 周期(日/周/月/季/年) + 班次 + 起止日期 + 角色' + CHAR(10) +
        N'- 保存计划即按「设备×日期×班次」全量生成执行单；编辑先清本计划未执行待办再重生（已完成保留）' + CHAR(10) +
        N'- PC 点检执行单仅查看；移动端按角色加载超期+当期，录入实测值由系统按上下限/是否自动判定合格或异常' + CHAR(10) +
        N'- 移动端「已完成」分页懒加载；异常仅记录结果，维修单由工作流自动生成',
        1, N'arbore');
END
GO

PRINT '==== v2.1.5 patch applied ====';
GO
