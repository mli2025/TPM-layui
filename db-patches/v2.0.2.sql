/* =============================================================================
   Patch v2.0.2 —— WP-F/G/H/J 全新模块 + WP-A 登录日志/账户锁定（页面交付）
   交付页面：
     特种设备(台账/检验计划/检验记录)、安全附件(台账/检定计划)、
     计量器具(档案/校准计划/校准记录/送外检)、
     能源(计量点配置/实时监控/能耗统计)、
     系统管理(登录日志/账户锁定)
   作用：启用对应占位菜单(Status=1) + 新增登录日志/账户锁定菜单并绑定 admin。
   依赖：必须先执行 v2.0.0.sql。幂等：可重复执行。
   ============================================================================= */

/* 1) 启用 4 个新顶级模块及其子菜单 */
UPDATE [Sys_Module] SET [Status] = 1
 WHERE [Code] IN (
    'special','safety','meter','energy',
    'special-equip','special-plan','special-record',
    'safety-acc','safety-plan',
    'meter-archive','meter-calib','meter-record','meter-sendout',
    'energy-dash','energy-point','energy-stat')
   AND [Status] <> 1;
GO

/* 2) 新增 WP-A 登录日志 / 账户锁定菜单（system 下，直接可见） */
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'登录日志','sys-loginlog','/Sys_LoginLog/Index',[Id],12,1,NULL FROM [Sys_Module] WHERE [Code]='system'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='sys-loginlog');
INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'账户锁定','sys-lock','/Sys_AccountLock/Index',[Id],13,1,NULL FROM [Sys_Module] WHERE [Code]='system'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='sys-lock');
GO

/* 3) 新增菜单绑定 admin 角色 */
INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin'), m.[Id]
  FROM [Sys_Module] m
 WHERE m.[Code] IN ('sys-loginlog','sys-lock')
   AND NOT EXISTS (SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.RoleId = (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin')
          AND rm.ModuleId = m.[Id]);
GO

PRINT '==== Menus enabled: special / safety / meter / energy + login-log / account-lock ====';
GO

/* =============================================================================
   版本记录 v2.0.2
   ============================================================================= */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.0.2')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]       = N'全新模块：特种设备/安全附件/计量器具/能源 + 登录日志/账户锁定',
           [Content]     =
              N'## 本次交付（功能页面）' + CHAR(10) +
              N'- 特种设备：台账(代码/类别/注册证/使用证/下次检验日超期红标) + 法定检验计划 + 检验记录' + CHAR(10) +
              N'- 安全附件：台账(整定压力/检定范围/周期) + 检定计划' + CHAR(10) +
              N'- 计量器具：档案 + 校准计划(超期红标) + 校准记录(复核后生效·GMP) + 送外检(主子多选器具)' + CHAR(10) +
              N'- 能源：计量点配置 + 实时监控仪表板(读 n8n 时序最新值) + 能耗多维统计' + CHAR(10) +
              N'- 系统：登录日志查询、账户锁定(管理员解锁)' + CHAR(10) +
              N'## 菜单' + CHAR(10) +
              N'- special/safety/meter/energy 及子菜单由占位置为显示；新增 sys-loginlog/sys-lock',
           [IsCurrent]   = 1,
           [Author]      = N'arbore'
     WHERE [Version] = 'v2.0.2';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.0.2', getdate(),
        N'全新模块：特种设备/安全附件/计量器具/能源 + 登录日志/账户锁定',
        N'## 本次交付（功能页面）' + CHAR(10) +
        N'- 特种设备：台账(代码/类别/注册证/使用证/下次检验日超期红标) + 法定检验计划 + 检验记录' + CHAR(10) +
        N'- 安全附件：台账(整定压力/检定范围/周期) + 检定计划' + CHAR(10) +
        N'- 计量器具：档案 + 校准计划(超期红标) + 校准记录(复核后生效·GMP) + 送外检(主子多选器具)' + CHAR(10) +
        N'- 能源：计量点配置 + 实时监控仪表板(读 n8n 时序最新值) + 能耗多维统计' + CHAR(10) +
        N'- 系统：登录日志查询、账户锁定(管理员解锁)' + CHAR(10) +
        N'## 菜单' + CHAR(10) +
        N'- special/safety/meter/energy 及子菜单由占位置为显示；新增 sys-loginlog/sys-lock',
        1, N'arbore');
END
GO

PRINT '==== Patch v2.0.2 applied ====';
GO
