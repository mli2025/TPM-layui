/* =============================================================================
   Patch v2.0.3 —— 工作流审批引擎 + 补全页面（检定记录/出入库/能源报警规则·记录）
   交付页面：
     工作流(流程模板/流程审批)、
     安全附件(检定记录)、计量器具(出入库)、
     能源(报警规则/报警记录)
   作用：启用 sys-workflow 占位菜单 + 新增上述菜单并绑定 admin。
   依赖：必须先执行 v2.0.0.sql。幂等：可重复执行。
   ============================================================================= */

/* 1) 启用工作流模板占位菜单 */
UPDATE [Sys_Module] SET [Status] = 1 WHERE [Code] = 'sys-workflow' AND [Status] <> 1;
GO

/* 2) 流程审批（实例）菜单，放在工作流模板之后 */
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'流程审批','wf-inst','/Wf_Instance/Index',[Id],11,1,NULL FROM [Sys_Module] WHERE [Code]='system'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='wf-inst');
GO

/* 3) 安全附件 - 检定记录（safety 下） */
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'检定记录','safety-record','/Safety_CheckRecord/Index',[Id],3,1,NULL FROM [Sys_Module] WHERE [Code]='safety'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='safety-record');
GO

/* 4) 计量器具 - 出入库（meter 下） */
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'出入库管理','meter-inout','/Meter_InOut/Index',[Id],5,1,NULL FROM [Sys_Module] WHERE [Code]='meter'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='meter-inout');
GO

/* 5) 能源 - 报警规则 / 报警记录（energy 下） */
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'报警规则','energy-alarm-rule','/Energy_AlarmRule/Index',[Id],4,1,NULL FROM [Sys_Module] WHERE [Code]='energy'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='energy-alarm-rule');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'报警记录','energy-alarm-rec','/Energy_AlarmRecord/Index',[Id],5,1,NULL FROM [Sys_Module] WHERE [Code]='energy'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='energy-alarm-rec');
GO

/* 6) 新增菜单绑定 admin 角色 */
INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin'), m.[Id]
  FROM [Sys_Module] m
 WHERE m.[Code] IN ('sys-workflow','wf-inst','safety-record','meter-inout','energy-alarm-rule','energy-alarm-rec')
   AND NOT EXISTS (SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.RoleId = (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin')
          AND rm.ModuleId = m.[Id]);
GO

PRINT '==== Menus enabled: workflow + safety-record / meter-inout / energy-alarm ====';
GO

/* =============================================================================
   版本记录 v2.0.3
   ============================================================================= */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.0.3')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]       = N'工作流审批引擎 + 补全页面（检定记录/出入库/能源报警）',
           [Content]     =
              N'## 本次交付（功能页面）' + CHAR(10) +
              N'- 工作流引擎：流程模板(线性节点·提交/审核/批准/派发) + 流程审批(发起/同意/驳回/撤回·审批时间线)，供各业务模块复用' + CHAR(10) +
              N'- 安全附件：检定记录(送检/取回/结论/下次检定日)' + CHAR(10) +
              N'- 计量器具：出入库管理(入库/出库·经办人)' + CHAR(10) +
              N'- 能源：报警规则(阈值/级别/通知) + 报警记录(n8n 比对写入·处置)',
           [IsCurrent]   = 1,
           [Author]      = N'arbore'
     WHERE [Version] = 'v2.0.3';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.0.3', getdate(),
        N'工作流审批引擎 + 补全页面（检定记录/出入库/能源报警）',
        N'## 本次交付（功能页面）' + CHAR(10) +
        N'- 工作流引擎：流程模板(线性节点·提交/审核/批准/派发) + 流程审批(发起/同意/驳回/撤回·审批时间线)，供各业务模块复用' + CHAR(10) +
        N'- 安全附件：检定记录(送检/取回/结论/下次检定日)' + CHAR(10) +
        N'- 计量器具：出入库管理(入库/出库·经办人)' + CHAR(10) +
        N'- 能源：报警规则(阈值/级别/通知) + 报警记录(n8n 比对写入·处置)',
        1, N'arbore');
END
GO

PRINT '==== Patch v2.0.3 applied ====';
GO
