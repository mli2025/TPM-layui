/* =============================================================================
   arbore TPM · Patch v2.1.0 -> v2.1.1
   Target: SQL Server 2017+   Database: [TPM]
   内容：
     1) 新增「仓库」主数据表 Basic_Warehouse + 菜单（备品备件 spare 下）+ 角色绑定
     2) 将「工作流」(原 n8n) 集成配置项的显示标题统一改为「工作流」字样
   幂等：均带 NOT EXISTS / OBJECT_ID 守卫，可重复执行。
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* =============================================================================
   批次1 · 仓库主数据表
   ============================================================================= */
IF OBJECT_ID(N'[Basic_Warehouse]', N'U') IS NULL
BEGIN
    CREATE TABLE [Basic_Warehouse] (
        [Id]         bigint        IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Code]       nvarchar(100) NULL,
        [Name]       nvarchar(200) NULL,
        [Location]   nvarchar(200) NULL,
        [Manager]    nvarchar(50)  NULL,
        [Remark]     nvarchar(300) NULL,
        [Status]     int           NOT NULL DEFAULT(1),
        [CreateDate] datetime      NOT NULL DEFAULT(GETDATE())
    );
    CREATE INDEX IX_Basic_Warehouse_Code ON [Basic_Warehouse]([Code]);
END
GO

/* 菜单挂在 备品备件 spare 下 */
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'仓库管理','spare-warehouse','/Basic_Warehouse/Index',[Id],1,1,'set'
  FROM [Sys_Module] WHERE [Code]='spare'
   AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='spare-warehouse');
GO
INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT (SELECT TOP 1 [Id] FROM [Sys_Role] WHERE [Name] IN (N'admin',N'系统管理员') ORDER BY [Id]), m.[Id]
  FROM [Sys_Module] m
 WHERE m.[Code] = 'spare-warehouse'
   AND NOT EXISTS (SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.RoleId = (SELECT TOP 1 [Id] FROM [Sys_Role] WHERE [Name] IN (N'admin',N'系统管理员') ORDER BY [Id])
          AND rm.ModuleId = m.[Id]);
GO
PRINT '==== Batch1: Basic_Warehouse table & menu ready ====';
GO

/* =============================================================================
   批次2 · 「工作流」(原 n8n) 集成配置项标题字样迁移（仅改显示标题，保留 Key 标识不变）
   ============================================================================= */
UPDATE [Sys_Setting] SET [Title]=N'工作流 OCR/智能解析 Webhook' WHERE [Key]='n8nOcrWebhook' AND [Title] LIKE N'n8n%';
UPDATE [Sys_Setting] SET [Title]=N'工作流 通知分发 Webhook'    WHERE [Key]='n8nNotifyWebhook' AND [Title] LIKE N'n8n%';
UPDATE [Sys_Setting] SET [Title]=N'工作流 AI 问答嵌入路径'      WHERE [Key]='n8nAiAgentUrl' AND [Title] LIKE N'n8n%';
UPDATE [Sys_Setting] SET [Title]=N'工作流 API Key'              WHERE [Key]='n8nApiKey' AND [Title] LIKE N'n8n%';
GO
PRINT '==== Batch2: workflow(n8n) setting titles renamed ====';
GO

PRINT '==== v2.1.1 patch applied ====';
GO
