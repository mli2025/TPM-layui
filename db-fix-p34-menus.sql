SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============ 系统管理（P4.8） ============ */
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Id] = 901008)
BEGIN
    INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
    VALUES (901008, N'系统管理', N'system', NULL, 0, 99, 1, N'settings');
END
GO

;WITH children([Id], [Name], [Code], [Url], [ParentId], [Sort]) AS (
    SELECT 901801, N'用户管理', N'sys-user',   N'/Sys_User/Index',   901008, 1 UNION ALL
    SELECT 901802, N'角色管理', N'sys-role',   N'/Sys_Role/Index',   901008, 2 UNION ALL
    SELECT 901803, N'部门管理', N'sys-dept',   N'/Sys_Dept/Index',   901008, 3 UNION ALL
    SELECT 901804, N'菜单管理', N'sys-module', N'/Sys_Module/Index', 901008, 4
)
INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT c.[Id], c.[Name], c.[Code], c.[Url], c.[ParentId], c.[Sort], 1, NULL
FROM children c
WHERE NOT EXISTS (SELECT 1 FROM [Sys_Module] m WHERE m.[Id] = c.[Id]);
GO

/* ============ 备品备件（P4.3 完整版） ============ */
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Id] = 901003)
BEGIN
    INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
    VALUES (901003, N'备品备件', N'spare', NULL, 0, 3, 1, N'package');
END
GO

;WITH children([Id], [Name], [Code], [Url], [ParentId], [Sort]) AS (
    SELECT 901301, N'备件主数据', N'spare-basic',   N'/Basic_Spare/Index',         901003, 1 UNION ALL
    SELECT 901302, N'库存查询',   N'spare-stock',   N'/Spare_NowQuan/Index',       901003, 2 UNION ALL
    SELECT 901303, N'入库单',     N'spare-in',      N'/Spare_InvoiceMain/In',      901003, 3 UNION ALL
    SELECT 901304, N'出库单',     N'spare-out',     N'/Spare_InvoiceMain/Out',     901003, 4 UNION ALL
    SELECT 901305, N'全部单据',   N'spare-bills',   N'/Spare_InvoiceMain/Index',   901003, 5
)
INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT c.[Id], c.[Name], c.[Code], c.[Url], c.[ParentId], c.[Sort], 1, NULL
FROM children c
WHERE NOT EXISTS (SELECT 1 FROM [Sys_Module] m WHERE m.[Id] = c.[Id]);
GO

/* ============ 移动端入口（P3） ============ */
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Id] = 901009)
BEGIN
    INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
    VALUES (901009, N'移动端入口', N'mobile', N'/m', 0, 50, 1, N'smartphone');
END
GO

/* ============ 绑定到 admin 角色 ============ */
INSERT INTO [Sys_RoleModule] ([Id], [RoleId], [ModuleId])
SELECT 906000 + ROW_NUMBER() OVER (ORDER BY m.[Id]), 900001, m.[Id]
FROM [Sys_Module] m
WHERE m.[Id] IN (
    901008, 901801, 901802, 901803, 901804,
    901003, 901301, 901302, 901303, 901304, 901305,
    901009
)
  AND NOT EXISTS (
      SELECT 1 FROM [Sys_RoleModule] rm WHERE rm.[RoleId] = 900001 AND rm.[ModuleId] = m.[Id]
  );
GO

SELECT [Id], [Name], [Code], [Url], [ParentId], [Sort], [Status]
FROM [Sys_Module]
WHERE [Id] IN (
    901008, 901801, 901802, 901803, 901804,
    901003, 901301, 901302, 901303, 901304, 901305,
    901009
)
ORDER BY [ParentId], [Sort], [Id];
GO
