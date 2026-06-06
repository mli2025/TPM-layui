/* =============================================================================
   arbore TPM · Patch v2.1.5 -> v2.1.6
   Target: SQL Server 2017+   Database: [TPM]
   内容（角色保存修复 + 点检角色约束落地）：
     1) 用户管理：角色复选框加 lay-ignore，修复保存时勾选状态读取错乱
        （此前 layui 美化复选框导致 RoleIds 与实际勾选不一致，角色未能正确写入）
     2) 点检待办：未分配角色的用户不再放行全部待办，改为返回空+提示
        （与保养待办按工号过滤一致的「无授权即无数据」语义，落实「按角色加载」约定）
     3) 点检计划生成：保存计划生成执行单前，先清除「日期范围内 + 同设备 + 未执行」的
        旧待执行单，再按当前计划/模板重建（已执行的保留）
   说明：本补丁仅写版本记录，无表结构变更；幂等，可重复执行。
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* ---- 版本记录 ---- */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.1.6')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]   = N'修复用户角色保存；点检按角色加载落地；点检计划生成前清理范围内未执行单',
           [Content] = N'- 用户管理：角色复选框加 lay-ignore，修复保存时勾选状态读取错乱导致角色未能正确保存的问题' + CHAR(10) +
                       N'- 点检待办：未分配角色的用户不再看到全部待办，改为「按角色加载」，无角色返回空并提示' + CHAR(10) +
                       N'- 点检计划生成：保存计划生成执行单前，先清除「日期范围内 + 同设备 + 未执行」的旧待执行单再按当前模板重建（已完成保留）',
           [IsCurrent] = 1, [Author] = N'arbore'
     WHERE [Version] = 'v2.1.6';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.1.6', getdate(),
        N'修复用户角色保存；点检按角色加载落地；点检计划生成前清理范围内未执行单',
        N'- 用户管理：角色复选框加 lay-ignore，修复保存时勾选状态读取错乱导致角色未能正确保存的问题' + CHAR(10) +
        N'- 点检待办：未分配角色的用户不再看到全部待办，改为「按角色加载」，无角色返回空并提示' + CHAR(10) +
        N'- 点检计划生成：保存计划生成执行单前，先清除「日期范围内 + 同设备 + 未执行」的旧待执行单再按当前模板重建（已完成保留）',
        1, N'arbore');
END
GO

PRINT '==== v2.1.6 patch applied ====';
GO
