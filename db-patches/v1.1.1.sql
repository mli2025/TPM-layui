/* =====================================================================
 * TPM v2 增量补丁：v1.1.0 -> v1.1.1
 *   新增「员工主数据」菜单（系统管理 → 员工主数据）
 *   不修改任何表结构，只插菜单 + 调整 Sort + 绑定 admin 权限 + 写版本记录
 *   在 TPM 数据库下执行（幂等：可重复跑）
 * ===================================================================== */

USE [TPM];
GO

/* 1) 调整系统管理子菜单的 Sort：把菜单/全局设置/版本记录依次后移，给员工主数据留出 Sort=4 */
UPDATE [Sys_Module] SET [Sort] = 5 WHERE [Code] = 'sys-module';
UPDATE [Sys_Module] SET [Sort] = 6 WHERE [Code] = 'sys-setting';
UPDATE [Sys_Module] SET [Sort] = 7 WHERE [Code] = 'sys-version';
GO

/* 2) 插入「员工主数据」菜单（若已存在则更新 URL/Sort） */
DECLARE @sysId bigint = (SELECT [Id] FROM [Sys_Module] WHERE [Code] = 'system');
IF @sysId IS NULL
BEGIN
    RAISERROR(N'未找到 system 主菜单（Code=''system''），请先确认 v1.0.0 schema 已部署', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code] = 'sys-employee')
BEGIN
    UPDATE [Sys_Module]
       SET [Name]     = N'员工主数据',
           [Url]      = '/Basic_Employee/Index',
           [ParentId] = @sysId,
           [Sort]     = 4,
           [Status]   = 1
     WHERE [Code] = 'sys-employee';
END
ELSE
BEGIN
    INSERT INTO [Sys_Module] ([Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
    VALUES (N'员工主数据', 'sys-employee', '/Basic_Employee/Index', @sysId, 4, 1, NULL);
END
GO

/* 3) 给 admin 角色补绑该菜单（已绑定则不重复） */
INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT r.[Id], m.[Id]
  FROM [Sys_Role] r
  CROSS JOIN [Sys_Module] m
 WHERE r.[Name] = N'admin'
   AND m.[Code] = 'sys-employee'
   AND NOT EXISTS (
       SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.RoleId = r.[Id] AND rm.ModuleId = m.[Id]);
GO

/* 4) 版本记录 v1.1.1（先把所有当前版本清掉，再插入/更新） */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO

IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v1.1.1')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]       = N'员工主数据：系统管理新增基础资料维护',
           [Content]     =
              N'## 新增功能' + CHAR(10) +
              N'- 系统管理 → 员工主数据（/Basic_Employee/Index）：工号 / 姓名 / 部门 / 状态 CRUD' + CHAR(10) +
              N'- 列表支持按部门、状态过滤，工号 + 姓名模糊搜索，分页、批量删除、Excel 导出' + CHAR(10) +
              N'- 工号在全表唯一校验，保存重复将提示「工号 XXX 已存在」' + CHAR(10) +
              N'- 系统管理菜单顺序调整：用户 / 角色 / 部门 / 员工主数据 / 菜单 / 全局设置 / 版本记录' + CHAR(10) +
              N'## 关联' + CHAR(10) +
              N'- 维修单 / 保养单派工时的「员工列表」、保养人姓名显示均来自此表，' + CHAR(10) +
              N'  请尽快在「员工主数据」里补齐人员后再使用派工功能',
           [IsCurrent]   = 1,
           [Author]      = N'arbore'
     WHERE [Version] = 'v1.1.1';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version], [ReleaseDate], [Title], [Content], [IsCurrent], [Author])
    VALUES
     ('v1.1.1', getdate(),
      N'员工主数据：系统管理新增基础资料维护',
      N'## 新增功能' + CHAR(10) +
      N'- 系统管理 → 员工主数据（/Basic_Employee/Index）：工号 / 姓名 / 部门 / 状态 CRUD' + CHAR(10) +
      N'- 列表支持按部门、状态过滤，工号 + 姓名模糊搜索，分页、批量删除、Excel 导出' + CHAR(10) +
      N'- 工号在全表唯一校验，保存重复将提示「工号 XXX 已存在」' + CHAR(10) +
      N'- 系统管理菜单顺序调整：用户 / 角色 / 部门 / 员工主数据 / 菜单 / 全局设置 / 版本记录' + CHAR(10) +
      N'## 关联' + CHAR(10) +
      N'- 维修单 / 保养单派工时的「员工列表」、保养人姓名显示均来自此表，' + CHAR(10) +
      N'  请尽快在「员工主数据」里补齐人员后再使用派工功能',
      1, N'arbore');
END
GO

PRINT '==== Patch v1.1.1 applied ====';
SELECT [Code], [Name], [Url], [Sort], [Status]
  FROM [Sys_Module]
 WHERE [ParentId] = (SELECT [Id] FROM [Sys_Module] WHERE [Code] = 'system')
 ORDER BY [Sort];

SELECT [Version], [ReleaseDate], [Title], [IsCurrent]
  FROM [Sys_Version] ORDER BY [ReleaseDate] DESC;
GO
