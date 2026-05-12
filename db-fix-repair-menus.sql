SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* repair management parent module */
IF NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Id] = 901004)
BEGIN
    INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
    VALUES (901004, N'设备维修', N'repair', NULL, 0, 4, 1, N'tools');
END
GO

/* repair child modules */
;WITH children([Id], [Name], [Code], [Url], [ParentId], [Sort]) AS (
    SELECT 901401, N'维修工单列表', N'rp-bill', N'/Facility_RepairBillMain/Index', 901004, 1
)
INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT c.[Id], c.[Name], c.[Code], c.[Url], c.[ParentId], c.[Sort], 1, NULL
FROM children c
WHERE NOT EXISTS (SELECT 1 FROM [Sys_Module] m WHERE m.[Id] = c.[Id]);
GO

/* bind new modules to role 900001 (admin) */
INSERT INTO [Sys_RoleModule] ([Id], [RoleId], [ModuleId])
SELECT 905000 + ROW_NUMBER() OVER (ORDER BY m.[Id]), 900001, m.[Id]
FROM [Sys_Module] m
WHERE m.[Id] IN (901004, 901401)
  AND NOT EXISTS (
      SELECT 1
      FROM [Sys_RoleModule] rm
      WHERE rm.[RoleId] = 900001 AND rm.[ModuleId] = m.[Id]
  );
GO

SELECT [Id], [Name], [Code], [Url], [ParentId], [Sort], [Status]
FROM [Sys_Module]
WHERE [Id] IN (901004, 901401)
ORDER BY [Id];
GO
