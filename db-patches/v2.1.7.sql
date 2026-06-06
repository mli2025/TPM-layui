/* =============================================================================
   arbore TPM · Patch v2.1.6 -> v2.1.7
   Target: SQL Server 2017+   Database: [TPM]
   内容（点检列表体验 + 全局表格刷新 + 标签右键菜单修复）：
     1) 点检执行单列表：新增刷新按钮，改用 SoulGrid 前端模式，支持表头筛选与点列头排序、导出
     2) 点检计划列表：新增刷新按钮
     3) 所有表格新增/编辑保存后自动刷新列表
        （修复 SoulGrid 前端模式列表误用 table.reload 导致保存后不刷新的问题）
     4) 标签页右键快捷菜单：点击/右键菜单外部区域（含 iframe 之上）自动关闭，支持 Esc 关闭
   说明：本补丁仅写版本记录，无表结构变更；幂等，可重复执行。
   ============================================================================= */
SET NOCOUNT ON;
GO
USE [TPM];
GO

/* ---- 版本记录 ---- */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.1.7')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]   = N'点检列表增加刷新/筛选/排序；表格保存后自动刷新；标签右键菜单可点外部关闭',
           [Content] = N'- 点检执行单列表：新增刷新按钮，支持表头筛选、点列头排序与导出' + CHAR(10) +
                       N'- 点检计划列表：新增刷新按钮' + CHAR(10) +
                       N'- 所有表格新增/编辑保存后自动刷新列表（修复前端模式列表保存后不刷新的问题）' + CHAR(10) +
                       N'- 标签页右键快捷菜单：点击/右键菜单外部区域（含 iframe 之上）自动关闭，支持 Esc 关闭',
           [IsCurrent] = 1, [Author] = N'arbore'
     WHERE [Version] = 'v2.1.7';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.1.7', getdate(),
        N'点检列表增加刷新/筛选/排序；表格保存后自动刷新；标签右键菜单可点外部关闭',
        N'- 点检执行单列表：新增刷新按钮，支持表头筛选、点列头排序与导出' + CHAR(10) +
        N'- 点检计划列表：新增刷新按钮' + CHAR(10) +
        N'- 所有表格新增/编辑保存后自动刷新列表（修复前端模式列表保存后不刷新的问题）' + CHAR(10) +
        N'- 标签页右键快捷菜单：点击/右键菜单外部区域（含 iframe 之上）自动关闭，支持 Esc 关闭',
        1, N'arbore');
END
GO

PRINT '==== v2.1.7 patch applied ====';
GO
