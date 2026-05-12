SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* maintenance parent module */
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Id] = 901002)
BEGIN
    INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
    VALUES (901002, N'设备保养', N'maintenance', NULL, 0, 2, 1, N'wrench');
END
GO

/* maintenance child modules */
;WITH children([Id], [Name], [Code], [Url], [ParentId], [Sort]) AS (
    SELECT 901201, N'设备保养项目列表', N'mt-item',        N'/Facility_Item/Index',                    901002, 1 UNION ALL
    SELECT 901202, N'设备保养模板列表', N'mt-template',    N'/Facility_TheTemplateMain/Index',         901002, 2 UNION ALL
    SELECT 901203, N'外委保养列表',     N'mt-outsourcing', N'/Facility_OutsourcingMaintenance/Index', 901002, 3 UNION ALL
    SELECT 901204, N'保养工单列表',     N'mt-bill',        N'/Facility_BillMain/Index',                901002, 4
)
INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT c.[Id], c.[Name], c.[Code], c.[Url], c.[ParentId], c.[Sort], 1, NULL
FROM children c
WHERE NOT EXISTS (SELECT 1 FROM [Sys_Module] m WHERE m.[Id] = c.[Id]);
GO

/* bind new modules to role 900001 (admin) */
INSERT INTO [Sys_RoleModule] ([Id], [RoleId], [ModuleId])
SELECT 903000 + ROW_NUMBER() OVER (ORDER BY m.[Id]), 900001, m.[Id]
FROM [Sys_Module] m
WHERE m.[Id] IN (901002, 901201, 901202, 901203, 901204)
  AND NOT EXISTS (
      SELECT 1
      FROM [Sys_RoleModule] rm
      WHERE rm.[RoleId] = 900001 AND rm.[ModuleId] = m.[Id]
  );
GO

SELECT [Id], [Name], [Code], [Url], [ParentId], [Sort], [Status]
FROM [Sys_Module]
WHERE [Id] IN (901002, 901201, 901202, 901203, 901204)
ORDER BY [Id];
GO
