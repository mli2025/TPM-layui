/* =============================================================================
   arbore TPM · Patch v2.0.x -> v2.1.0  (URS backlog 功能落地配套 SQL)
   Target: SQL Server 2017+   Database: [TPM]
   说明：
     - v2.0.0 已建好全部新模块表，本补丁只补「种子数据 / 设置项 / 缺失列 / 菜单激活」
     - 幂等：均带 NOT EXISTS / COL_LENGTH 守卫，可重复执行
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* =============================================================================
   批次1 · WP-A 快赢：安全设置项 + 8 种角色模板
   ============================================================================= */

/* 会话超时（分钟，URS 407 默认建议 15；此处给 60 便于测试，可在“全局设置”改） */
INSERT INTO [Sys_Setting] ([Group],[Key],[Value],[ValueType],[Title],[Descr],[Sort],[Editable])
SELECT 'security','Security.SessionIdleMinutes','60','int',N'会话空闲超时(分钟)',N'空闲超过该时长自动注销，URS 407',1,1
 WHERE NOT EXISTS (SELECT 1 FROM [Sys_Setting] WHERE [Key]='Security.SessionIdleMinutes');

/* 连续登录失败锁定阈值（URS 406） */
INSERT INTO [Sys_Setting] ([Group],[Key],[Value],[ValueType],[Title],[Descr],[Sort],[Editable])
SELECT 'security','Security.LoginFailThreshold','5','int',N'登录失败锁定阈值',N'连续失败达到该次数自动锁定账户，仅管理员可解锁，URS 406',2,1
 WHERE NOT EXISTS (SELECT 1 FROM [Sys_Setting] WHERE [Key]='Security.LoginFailThreshold');
GO

/* 预置 8 种角色模板（URS 401）。admin 已存在则跳过；其余按名称补齐 */
INSERT INTO [Sys_Role] ([Name],[Status])
SELECT v.[Name], 1
  FROM (VALUES
        (N'系统管理员'),
        (N'设备负责人'),
        (N'部门设备管理员'),
        (N'计量管理员'),
        (N'计量员'),
        (N'审核员'),
        (N'批准人'),
        (N'查询员')
      ) AS v([Name])
 WHERE NOT EXISTS (SELECT 1 FROM [Sys_Role] r WHERE r.[Name] = v.[Name]);
GO
PRINT '==== Batch1: security settings + role templates ready ====';
GO
