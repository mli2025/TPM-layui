/* =============================================================================
   Patch v2.0.1 —— WP-A 平台底座（第一批页面交付）
   交付页面：审计日志查询、用户组管理、通知中心
   作用：把对应菜单从 Status=0 占位置为 Status=1 显示。
   依赖：必须先执行 v2.0.0.sql（创建表 + 种菜单）。
   幂等：可重复执行。
   ============================================================================= */

/* 启用已交付页面的菜单 */
UPDATE [Sys_Module] SET [Status] = 1
 WHERE [Code] IN ('sys-audit', 'sys-usergroup', 'sys-notify')
   AND [Status] <> 1;
GO

PRINT '==== Menus enabled: sys-audit / sys-usergroup / sys-notify ====';
GO

/* =============================================================================
   版本记录 v2.0.1
   ============================================================================= */
UPDATE [Sys_Version] SET [IsCurrent] = 0 WHERE [IsCurrent] = 1;
GO
IF EXISTS (SELECT 1 FROM [Sys_Version] WHERE [Version] = 'v2.0.1')
BEGIN
    UPDATE [Sys_Version]
       SET [ReleaseDate] = getdate(),
           [Title]       = N'WP-A 平台底座：审计日志 / 用户组 / 通知中心',
           [Content]     =
              N'## 本次交付（功能页面）' + CHAR(10) +
              N'- 审计日志查询页：账号/模块/动作/结果/时间范围筛选，详情查看（只读）' + CHAR(10) +
              N'- 用户组管理：组 CRUD + 成员绑定 + 菜单授权（权限并集叠加）' + CHAR(10) +
              N'- 通知中心：本人站内消息列表、已读/全部已读、查看详情' + CHAR(10) +
              N'## 菜单' + CHAR(10) +
              N'- sys-audit / sys-usergroup / sys-notify 由占位 Status=0 置为显示',
           [IsCurrent]   = 1,
           [Author]      = N'arbore'
     WHERE [Version] = 'v2.0.1';
END
ELSE
BEGIN
    INSERT INTO [Sys_Version] ([Version],[ReleaseDate],[Title],[Content],[IsCurrent],[Author])
    VALUES ('v2.0.1', getdate(),
        N'WP-A 平台底座：审计日志 / 用户组 / 通知中心',
        N'## 本次交付（功能页面）' + CHAR(10) +
        N'- 审计日志查询页：账号/模块/动作/结果/时间范围筛选，详情查看（只读）' + CHAR(10) +
        N'- 用户组管理：组 CRUD + 成员绑定 + 菜单授权（权限并集叠加）' + CHAR(10) +
        N'- 通知中心：本人站内消息列表、已读/全部已读、查看详情' + CHAR(10) +
        N'## 菜单' + CHAR(10) +
        N'- sys-audit / sys-usergroup / sys-notify 由占位 Status=0 置为显示',
        1, N'arbore');
END
GO

PRINT '==== Patch v2.0.1 applied ====';
GO
