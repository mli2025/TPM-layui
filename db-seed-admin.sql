/* ================================================================
   arbore TPM · admin 初始化种子数据（幂等）
   目标库：wantong_mes_20250211
   作用：让 waes 账号登录后左栏立刻看到 8 个模块、25+ 个子菜单
   ================================================================

   前置假设
   --------
   1. Sys_User 表里 Account='waes' 这条记录的 Id 是  425623414465769472
      （来自你给我看的 SSMS 截图）。如果是别的值，把下面所有 425623414465769472
      替换成你的真实 Id。
   2. Sys_Role / Sys_UserRole / Sys_Module / Sys_RoleModule 4 张表已存在
      （hamaton 原生库一般都有；没有的话先跑 db-schema.sql）。
   3. 本脚本所有插入都用 IF NOT EXISTS 包裹，可以重复执行。
   4. 如果你的 hamaton 真表对 Sys_Module 等还有 NOT NULL 的额外字段
      （CreateTime/CreateUserId 之类），按报错信息往 INSERT 列表里补默认值即可。

   Id 段位约定（避免与现有数据冲突）
   ------------------------------------
   admin 角色 Id:        900001
   waes-admin 关联 Id:   900100
   主模块 Id:            901001 - 901099
   子模块 Id:            901100 - 901999
   admin-模块 关联 Id:   902000 - 902999
   ================================================================ */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO


/* ========== 1. admin 角色 ========== */
IF NOT EXISTS (SELECT 1 FROM [Sys_Role] WHERE [Id] = 900001)
BEGIN
    INSERT INTO [Sys_Role] ([Id], [Name], [Status])
    VALUES (900001, 'admin', 1);
    PRINT '[Sys_Role] admin role inserted (Id=900001)';
END
ELSE
    PRINT '[Sys_Role] admin role already exists, skipped';
GO


/* ========== 2. waes 用户 → admin 角色 ========== */
IF NOT EXISTS (
    SELECT 1 FROM [Sys_UserRole]
    WHERE [UserId] = 425623414465769472 AND [RoleId] = 900001
)
BEGIN
    INSERT INTO [Sys_UserRole] ([Id], [UserId], [RoleId])
    VALUES (900100, 425623414465769472, 900001);
    PRINT '[Sys_UserRole] waes -> admin linked (Id=900100)';
END
ELSE
    PRINT '[Sys_UserRole] waes -> admin already linked, skipped';
GO


/* ========== 3. 8 个主模块（父，ParentId=0） ========== */
;WITH parents([Id], [Name], [Code], [Sort], [Icon]) AS (
    SELECT 901001, N'设备台账',  'ledger',      1, 'package' UNION ALL
    SELECT 901002, N'设备保养',  'maintenance', 2, 'wrench' UNION ALL
    SELECT 901003, N'设备维修',  'repair',      3, 'hammer' UNION ALL
    SELECT 901004, N'设备点检',  'inspection',  4, 'clipboard-check' UNION ALL
    SELECT 901005, N'状态履历',  'status',      5, 'history' UNION ALL
    SELECT 901006, N'OEE',       'oee',         6, 'gauge' UNION ALL
    SELECT 901007, N'备品备件',  'spare',       7, 'boxes' UNION ALL
    SELECT 901008, N'模具管理',  'mold',        8, 'layers'
)
INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT p.[Id], p.[Name], p.[Code], NULL, 0, p.[Sort], 1, p.[Icon]
FROM parents p
WHERE NOT EXISTS (SELECT 1 FROM [Sys_Module] m WHERE m.[Id] = p.[Id]);
PRINT '[Sys_Module] parent modules inserted/skipped';
GO


/* ========== 4. 子模块（业务入口，指向 Controller 的 Index） ========== */
;WITH children([Id], [Name], [Code], [Url], [ParentId], [Sort]) AS (
    -- 1. 设备台账
    SELECT 901101, N'设备资源台账', 'res-detail',       '/Facility_ResourceDetail/Index',    901001, 1 UNION ALL
    SELECT 901102, N'设备资源',     'equipment-res',    '/Basic_EquipmentResources/Index',   901001, 2 UNION ALL

    -- 2. 设备保养
    SELECT 901201, N'保养工单',     'bill-main',        '/Facility_BillMain/Index',          901002, 1 UNION ALL
    SELECT 901202, N'保养模板',     'tpl-main',         '/Facility_TheTemplateMain/Index',   901002, 2 UNION ALL
    SELECT 901203, N'外委保养',     'outsourcing-mt',   '/Facility_OutsourcingMaintenance/Index', 901002, 3 UNION ALL

    -- 3. 设备维修
    SELECT 901301, N'维修单',       'repair-main',      '/Facility_RepairBillMain/Index',    901003, 1 UNION ALL
    SELECT 901302, N'外委维修',     'outsourcing-rep',  '/Facility_OutsourcingRepair/Index', 901003, 2 UNION ALL
    SELECT 901303, N'维修履历',     'repair-history',   '/Facility_RepairHistory/Index',     901003, 3 UNION ALL

    -- 4. 设备点检
    SELECT 901401, N'点检数据',     'inspect-data',     '/Facility_DATA/Index',              901004, 1 UNION ALL
    SELECT 901402, N'点检历史',     'inspect-hist',     '/Facility_DATA_History/Index',      901004, 2 UNION ALL
    SELECT 901403, N'点检项',       'inspect-item',     '/Facility_Item/Index',              901004, 3 UNION ALL
    SELECT 901404, N'部门点检',     'inspect-dept',     '/DianJianDept/Index',               901004, 4 UNION ALL

    -- 5. 状态履历
    SELECT 901501, N'状态历史',     'status-hist',      '/Facility_Status_History/Index',    901005, 1 UNION ALL
    SELECT 901502, N'出厂检验',     'out-qc',           '/Facility_OutQC/Index',             901005, 2 UNION ALL
    SELECT 901503, N'设备采集',     'gather',           '/Facility_ResourceDetailGather/Index', 901005, 3 UNION ALL

    -- 6. OEE
    SELECT 901601, N'OEE 速率',     'oee-rate',         '/OEE_Rate/Index',                   901006, 1 UNION ALL
    SELECT 901602, N'报废',         'oee-scrap',        '/OEE_Scrap/Index',                  901006, 2 UNION ALL
    SELECT 901603, N'停机',         'oee-stop',         '/OEE_StopTimes/Index',              901006, 3 UNION ALL
    SELECT 901604, N'总工时',       'oee-total',        '/OEE_TotalTimes/Index',             901006, 4 UNION ALL
    SELECT 901605, N'OEE 报表',     'oee-rpt',          '/Rpt_OEE/Index',                    901006, 5 UNION ALL

    -- 7. 备品备件
    SELECT 901701, N'备件资料',     'spare-basic',      '/Basic_Spare/Index',                901007, 1 UNION ALL
    SELECT 901702, N'出入库单',     'spare-invoice',    '/Spare_InvoiceMain/Index',          901007, 2 UNION ALL
    SELECT 901703, N'现存量',       'spare-quan',       '/Spare_NowQuan/Index',              901007, 3 UNION ALL

    -- 8. 模具管理
    SELECT 901801, N'模具资料',     'mold-basic',       '/Basic_Mold/Index',                 901008, 1 UNION ALL
    SELECT 901802, N'模具单据',     'mold-bill',        '/Mold_BillMain/Index',              901008, 2 UNION ALL
    SELECT 901803, N'上下模',       'mold-onoff',       '/Mold_OnOff/Index',                 901008, 3 UNION ALL
    SELECT 901804, N'模具维修',     'mold-repair',      '/Mold_RepairBill/Index',            901008, 4
)
INSERT INTO [Sys_Module] ([Id], [Name], [Code], [Url], [ParentId], [Sort], [Status], [Icon])
SELECT c.[Id], c.[Name], c.[Code], c.[Url], c.[ParentId], c.[Sort], 1, NULL
FROM children c
WHERE NOT EXISTS (SELECT 1 FROM [Sys_Module] m WHERE m.[Id] = c.[Id]);
PRINT '[Sys_Module] child modules inserted/skipped';
GO


/* ========== 5. admin 角色挂全部本次新建模块 ========== */
INSERT INTO [Sys_RoleModule] ([Id], [RoleId], [ModuleId])
SELECT 902000 + ROW_NUMBER() OVER (ORDER BY m.[Id]),
       900001,
       m.[Id]
FROM [Sys_Module] m
WHERE m.[Id] BETWEEN 901001 AND 901999
  AND NOT EXISTS (
        SELECT 1 FROM [Sys_RoleModule] rm
        WHERE rm.[RoleId] = 900001 AND rm.[ModuleId] = m.[Id]
      );
PRINT '[Sys_RoleModule] admin -> all modules linked';
GO


/* ========== 6. 校验 ========== */
SELECT
    (SELECT COUNT(1) FROM [Sys_Role] WHERE [Id] = 900001) AS adminRole,
    (SELECT COUNT(1) FROM [Sys_UserRole] WHERE [UserId] = 425623414465769472 AND [RoleId] = 900001) AS waesAdminLink,
    (SELECT COUNT(1) FROM [Sys_Module] WHERE [Id] BETWEEN 901001 AND 901999) AS modules,
    (SELECT COUNT(1) FROM [Sys_RoleModule] WHERE [RoleId] = 900001) AS roleModules;
GO

PRINT '==== Seed completed ====';
PRINT 'Re-login as waes to see the left sidebar populated with 8 modules.';
GO
