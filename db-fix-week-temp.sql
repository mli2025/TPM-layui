SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* device-side: add WeekTempId column to Facility_ResourceDetail */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'WeekTempId'
      AND Object_ID = Object_ID(N'[dbo].[Facility_ResourceDetail]')
)
BEGIN
    ALTER TABLE [dbo].[Facility_ResourceDetail] ADD [WeekTempId] BIGINT NULL;
    PRINT '[Facility_ResourceDetail] WeekTempId added';
END
ELSE
    PRINT '[Facility_ResourceDetail] WeekTempId already exists, skipped';
GO

/* normalize template-side MaintenanceType values to YEAR/QUARTER/MONTH/WEEK */
UPDATE [dbo].[Facility_TheTemplateMain] SET [MaintenanceType] = 'YEAR'    WHERE [MaintenanceType] IN (N'年');
UPDATE [dbo].[Facility_TheTemplateMain] SET [MaintenanceType] = 'QUARTER' WHERE [MaintenanceType] IN (N'季', N'季度');
UPDATE [dbo].[Facility_TheTemplateMain] SET [MaintenanceType] = 'MONTH'   WHERE [MaintenanceType] IN (N'月');
UPDATE [dbo].[Facility_TheTemplateMain] SET [MaintenanceType] = 'WEEK'    WHERE [MaintenanceType] IN (N'周');
PRINT '[Facility_TheTemplateMain] MaintenanceType normalized';
GO
