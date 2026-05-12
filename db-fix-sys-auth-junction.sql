/* ================================================================
   Fix: 对象名 'dbo.Sys_UserRole' 无效（208）
   适用：库中已有 Sys_User / Sys_Module 等，但缺少用户-角色-模块关联表

   在 SSMS 中切到业务库后整段执行。幂等：已存在表则跳过。
   ================================================================ */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[Sys_UserRole]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_UserRole]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_UserRole] (
        [Id] BIGINT NOT NULL,
        [UserId] BIGINT NOT NULL,
        [RoleId] BIGINT NOT NULL,
        CONSTRAINT [PK_Sys_UserRole] PRIMARY KEY ([Id])
    );
    PRINT 'Created [dbo].[Sys_UserRole]';
END
ELSE
    PRINT 'Skipped [dbo].[Sys_UserRole] (already exists)';
GO

IF OBJECT_ID(N'[dbo].[Sys_RoleModule]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_RoleModule]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_RoleModule] (
        [Id] BIGINT NOT NULL,
        [RoleId] BIGINT NOT NULL,
        [ModuleId] BIGINT NOT NULL,
        CONSTRAINT [PK_Sys_RoleModule] PRIMARY KEY ([Id])
    );
    PRINT 'Created [dbo].[Sys_RoleModule]';
END
ELSE
    PRINT 'Skipped [dbo].[Sys_RoleModule] (already exists)';
GO

IF OBJECT_ID(N'[dbo].[Sys_ModuleButtons]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_ModuleButtons]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_ModuleButtons] (
        [Id] BIGINT NOT NULL,
        [ModuleId] BIGINT NOT NULL,
        [DomId] NVARCHAR(MAX) NULL,
        [Name] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Sys_ModuleButtons] PRIMARY KEY ([Id])
    );
    PRINT 'Created [dbo].[Sys_ModuleButtons]';
END
ELSE
    PRINT 'Skipped [dbo].[Sys_ModuleButtons] (already exists)';
GO

IF OBJECT_ID(N'[dbo].[Sys_Role]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Sys_Role]', N'V') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sys_Role] (
        [Id] BIGINT NOT NULL,
        [Name] NVARCHAR(MAX) NULL,
        [Status] INT NOT NULL,
        CONSTRAINT [PK_Sys_Role] PRIMARY KEY ([Id])
    );
    PRINT 'Created [dbo].[Sys_Role]';
END
ELSE
    PRINT 'Skipped [dbo].[Sys_Role] (already exists)';
GO

PRINT '==== db-fix-sys-auth-junction.sql finished ====';
PRINT 'Next: run db-seed-admin.sql if you need menu data, then re-login.';
GO
