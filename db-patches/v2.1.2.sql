/* =============================================================================
   arbore TPM · Patch v2.1.1 -> v2.1.2
   Target: SQL Server 2017+   Database: [TPM]
   内容：
     1) 点检新流程菜单迁至「设备点检」顶级模块（ins-standard / ins-plan / ins-record）
     2) 停用旧版点检菜单（chk-item / chk-template / chk-bill）
   幂等：可重复执行。
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* 新流程菜单挂到 设备点检 inspection */
UPDATE m SET m.[ParentId] = p.[Id], m.[Sort] = v.[Sort]
  FROM [Sys_Module] m
 INNER JOIN [Sys_Module] p ON p.[Code] = 'inspection'
 INNER JOIN (VALUES
        ('ins-standard', 1),
        ('ins-plan',     2),
        ('ins-record',   3)
       ) v([Code],[Sort]) ON m.[Code] = v.[Code];
GO

/* 若新菜单尚未创建（仅执行过 schema 未执行 v2.1.0 批次6 的环境）则补插 */
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'点检标准库','ins-standard','/Inspect_Standard/Index',[Id],1,1,'template-1'
  FROM [Sys_Module] WHERE [Code]='inspection'
   AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='ins-standard');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'点检计划','ins-plan','/Inspect_Plan/Index',[Id],2,1,'date'
  FROM [Sys_Module] WHERE [Code]='inspection'
   AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='ins-plan');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'点检执行单','ins-record','/Inspect_Record/Index',[Id],3,1,'form'
  FROM [Sys_Module] WHERE [Code]='inspection'
   AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='ins-record');
GO

/* 从设备维修下移除重复挂载（仅改 ParentId，不删记录） */
UPDATE m SET m.[ParentId] = p.[Id]
  FROM [Sys_Module] m
 INNER JOIN [Sys_Module] p ON p.[Code] = 'inspection'
 WHERE m.[Code] IN ('ins-standard','ins-plan','ins-record')
   AND m.[ParentId] <> p.[Id];
GO

/* 停用旧版点检（Facility 只读链路） */
UPDATE [Sys_Module] SET [Status] = 0 WHERE [Code] IN ('chk-item','chk-template','chk-bill');
GO

INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT (SELECT TOP 1 [Id] FROM [Sys_Role] WHERE [Name] IN (N'admin',N'系统管理员') ORDER BY [Id]), m.[Id]
  FROM [Sys_Module] m
 WHERE m.[Code] IN ('ins-standard','ins-plan','ins-record')
   AND NOT EXISTS (SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.RoleId = (SELECT TOP 1 [Id] FROM [Sys_Role] WHERE [Name] IN (N'admin',N'系统管理员') ORDER BY [Id])
          AND rm.ModuleId = m.[Id]);
GO

UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.1.2')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]   = N'点检菜单独立：新流程迁至设备点检，停用旧点检项目/模板/工单',
           [Content] = N'- 保留 URS 701-706 点检标准库 / 点检计划 / 点检执行单，菜单挂「设备点检」' + CHAR(10) +
                       N'- 停用旧版点检项目、点检模板、点检工单（Facility 只读链路）' + CHAR(10) +
                       N'- PC 与移动端统一使用「点检执行单」名称',
           [IsCurrent] = 1, [Author] = N'arbore'
     WHERE [Version] = 'v2.1.2';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.1.2', getdate(),
        N'点检菜单独立：新流程迁至设备点检，停用旧点检项目/模板/工单',
        N'- 保留 URS 701-706 点检标准库 / 点检计划 / 点检执行单，菜单挂「设备点检」' + CHAR(10) +
        N'- 停用旧版点检项目、点检模板、点检工单（Facility 只读链路）' + CHAR(10) +
        N'- PC 与移动端统一使用「点检执行单」名称',
        1, N'arbore');
END
GO

PRINT '==== v2.1.2 patch applied ====';
GO
