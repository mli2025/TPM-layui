SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* inspection parent module */
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Id] = 901003)
BEGIN
    INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
    VALUES (901003, N'设备点检', N'inspection', NULL, 0, 3, 1, N'check-square');
END
GO

/* inspection child modules */
;WITH children([Id], [Name], [Code], [Url], [ParentId], [Sort]) AS (
    SELECT 901301, N'设备点检项目列表', N'ck-item',     N'/Facility_CheckItem/Index',     901003, 1 UNION ALL
    SELECT 901302, N'设备点检模板列表', N'ck-template', N'/Facility_CheckTemplate/Index', 901003, 2 UNION ALL
    SELECT 901303, N'设备点检单列表',   N'ck-bill',     N'/Facility_CheckBill/Index',     901003, 3
)
INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT c.[Id], c.[Name], c.[Code], c.[Url], c.[ParentId], c.[Sort], 1, NULL
FROM children c
WHERE NOT EXISTS (SELECT 1 FROM [Sys_Module] m WHERE m.[Id] = c.[Id]);
GO

/* bind new modules to role 900001 (admin) */
INSERT INTO [Sys_RoleModule] ([Id], [RoleId], [ModuleId])
SELECT 903100 + ROW_NUMBER() OVER (ORDER BY m.[Id]), 900001, m.[Id]
FROM [Sys_Module] m
WHERE m.[Id] IN (901003, 901301, 901302, 901303)
  AND NOT EXISTS (
      SELECT 1
      FROM [Sys_RoleModule] rm
      WHERE rm.[RoleId] = 900001 AND rm.[ModuleId] = m.[Id]
  );
GO

SELECT [Id], [Name], [Code], [Url], [ParentId], [Sort], [Status]
FROM [Sys_Module]
WHERE [Id] IN (901003, 901301, 901302, 901303)
ORDER BY [Id];
GO
