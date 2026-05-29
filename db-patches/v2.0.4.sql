/* =============================================================================
   Patch v2.0.4 —— 路线 C 试点：设备台账（Tabulator）新版页面菜单
   作用：在「设备台账」下新增「设备台账(新版·试点)」菜单，指向 /Facility_ResourceDetail/Grid，
        用于与原 layui 列表页对比 Excel 式表头筛选 / 排序 / 服务端分页体验。
   依赖：必须先执行 db-schema-v2.sql（含 ledger / res-detail 菜单）。幂等：可重复执行。
   ============================================================================= */

INSERT INTO [Sys_Module] ([Name],[Code],[Url],[ParentId],[Sort],[Status],[Icon])
SELECT N'设备台账(新版·试点)','res-detail-grid','/Facility_ResourceDetail/Grid',[Id],2,1,NULL
  FROM [Sys_Module] WHERE [Code]='ledger'
 AND NOT EXISTS (SELECT 1 FROM [Sys_Module] WHERE [Code]='res-detail-grid');
GO

/* 绑定 admin 角色 */
INSERT INTO [Sys_RoleModule] ([RoleId], [ModuleId])
SELECT (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin'), m.[Id]
  FROM [Sys_Module] m
 WHERE m.[Code] = 'res-detail-grid'
   AND NOT EXISTS (SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.RoleId = (SELECT [Id] FROM [Sys_Role] WHERE [Name]=N'admin')
          AND rm.ModuleId = m.[Id]);
GO

PRINT '==== Menu added: res-detail-grid (Tabulator pilot) ====';
GO

/* 版本记录 v2.0.4 */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.0.4')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]   = N'路线C试点：设备台账 Tabulator 新版（Excel 式筛选/排序/服务端分页）',
           [Content] = N'- 新增 /Facility_ResourceDetail/Grid 试点页：Tailwind 外观 + Tabulator 表格' + CHAR(10) +
                       N'- 表头输入框/下拉即时筛选、点列头排序、服务端分页（2 万级数据可用）、行点击只读详情' + CHAR(10) +
                       N'- 原 layui 台账页 /Facility_ResourceDetail/Index 保留不变，供对比',
           [IsCurrent] = 1, [Author] = N'arbore'
     WHERE [Version] = 'v2.0.4';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.0.4', getdate(),
        N'路线C试点：设备台账 Tabulator 新版（Excel 式筛选/排序/服务端分页）',
        N'- 新增 /Facility_ResourceDetail/Grid 试点页：Tailwind 外观 + Tabulator 表格' + CHAR(10) +
        N'- 表头输入框/下拉即时筛选、点列头排序、服务端分页（2 万级数据可用）、行点击只读详情' + CHAR(10) +
        N'- 原 layui 台账页 /Facility_ResourceDetail/Index 保留不变，供对比',
        1, N'arbore');
END
GO

PRINT '==== Patch v2.0.4 applied ====';
GO
