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

PRINT 'Patch v2.1.6 applied.';
GO
