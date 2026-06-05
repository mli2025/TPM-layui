/* =============================================================================
   arbore TPM · Patch v2.1.5 -> v2.1.6
   Target: SQL Server 2017+   Database: [TPM]
   内容（延期申请改造，第4项）：
     1) Maint_DelayApply 新增 OldEndDate / NewEndDate（原计划/申请新计划改为日期段：开始~结束）
     2) Facility_BillMain 新增 ChangedBeginDate / ChangedEndDate（延期审批通过后写入变更后日期段，原计划日期保留）
   幂等：均带列存在性守卫，可重复执行。
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* ---- 1. Maint_DelayApply：原/新计划日期段（结束日期） ---- */
IF COL_LENGTH('Maint_DelayApply','OldEndDate') IS NULL
    ALTER TABLE [Maint_DelayApply] ADD [OldEndDate] datetime NULL;
GO
IF COL_LENGTH('Maint_DelayApply','NewEndDate') IS NULL
    ALTER TABLE [Maint_DelayApply] ADD [NewEndDate] datetime NULL;
GO

/* ---- 2. Facility_BillMain：延期审批通过后写入的「变更后日期段」 ---- */
IF COL_LENGTH('Facility_BillMain','ChangedBeginDate') IS NULL
    ALTER TABLE [Facility_BillMain] ADD [ChangedBeginDate] datetime NULL;
GO
IF COL_LENGTH('Facility_BillMain','ChangedEndDate') IS NULL
    ALTER TABLE [Facility_BillMain] ADD [ChangedEndDate] datetime NULL;
GO

/* ---- 版本记录 ---- */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.1.6')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]   = N'保养/点检/移动端/备件批量修复：延期申请改造 + 维修记录列 + 状态色板等',
           [Content] = N'- 延期申请：业务列改为保养单号，新增设备编码/名称/派工人员/保养类型列，类型中文；仅显示新建/已派工工单；选工单自动带出原计划日期段；申请日期改为日期段；审批通过写回工单「变更后开始/结束日期」(新增字段)' + CHAR(10) +
                       N'- 点检执行单：查看已完成详情显示最小/最大值与合格范围；按上下限自愈重判，区间内不再误判异常；已执行单不可删除' + CHAR(10) +
                       N'- 移动端保养：待办仅显示派工给本人的工单；接单后才可编辑、全部项有值才可提交；附件支持预览/删除/保存并修复双击重复选文件' + CHAR(10) +
                       N'- 设备台账：维修记录改显示报修原因/维修描述/原因分析/预防措施；保养与点检记录过滤排序、默认执行日期倒序；保养/点检上次日期回写' + CHAR(10) +
                       N'- 维保资质：显示设备编码/名称、查看按钮、附件上传、过滤排序、模板下载导入；保养项目去过滤下拉并增加导入；点检计划列表过滤排序+启用状态；维修工单状态颜色可配置；备品备件出入库选料修复',
           [IsCurrent] = 1, [Author] = N'arbore'
     WHERE [Version] = 'v2.1.6';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.1.6', getdate(),
        N'保养/点检/移动端/备件批量修复：延期申请改造 + 维修记录列 + 状态色板等',
        N'- 延期申请：业务列改为保养单号，新增设备编码/名称/派工人员/保养类型列，类型中文；仅显示新建/已派工工单；选工单自动带出原计划日期段；申请日期改为日期段；审批通过写回工单「变更后开始/结束日期」(新增字段)' + CHAR(10) +
        N'- 点检执行单：查看已完成详情显示最小/最大值与合格范围；按上下限自愈重判，区间内不再误判异常；已执行单不可删除' + CHAR(10) +
        N'- 移动端保养：待办仅显示派工给本人的工单；接单后才可编辑、全部项有值才可提交；附件支持预览/删除/保存并修复双击重复选文件' + CHAR(10) +
        N'- 设备台账：维修记录改显示报修原因/维修描述/原因分析/预防措施；保养与点检记录过滤排序、默认执行日期倒序；保养/点检上次日期回写' + CHAR(10) +
        N'- 维保资质：显示设备编码/名称、查看按钮、附件上传、过滤排序、模板下载导入；保养项目去过滤下拉并增加导入；点检计划列表过滤排序+启用状态；维修工单状态颜色可配置；备品备件出入库选料修复',
        1, N'arbore');
END
GO

PRINT '==== v2.1.6 patch applied ====';
SELECT [Version], [ReleaseDate], [Title], [IsCurrent] FROM [Sys_Version] ORDER BY [ReleaseDate] DESC;
GO
