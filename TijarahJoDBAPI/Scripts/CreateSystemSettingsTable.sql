-- ============================================================
-- TIJARAHJO: SYSTEM SETTINGS TABLE
-- ============================================================
-- This script creates the TbSystemSettings table for storing
-- application configuration like support contact info
-- ============================================================

USE TijarahJoDB;
GO

-- ============================================================
-- 1. CREATE SETTINGS TABLE
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TbSystemSettings')
BEGIN
    CREATE TABLE [dbo].[TbSystemSettings]
    (
        [SettingID]     INT IDENTITY(1,1) NOT NULL,
        [SettingKey]    NVARCHAR(100) NOT NULL,
        [SettingValue]  NVARCHAR(500) NULL,
        [SettingGroup]  NVARCHAR(50) NOT NULL DEFAULT 'General',
        [Description]   NVARCHAR(255) NULL,
        [DataType]      NVARCHAR(20) NOT NULL DEFAULT 'String', -- String, Int, Bool, Json
        [IsPublic]      BIT NOT NULL DEFAULT 0,  -- 1 = Can be read without auth
        [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedByUserID] INT NULL,
        
        CONSTRAINT [PK_TbSystemSettings] PRIMARY KEY CLUSTERED ([SettingID] ASC),
        CONSTRAINT [UQ_TbSystemSettings_Key] UNIQUE NONCLUSTERED ([SettingKey] ASC),
        CONSTRAINT [FK_TbSystemSettings_UpdatedBy] FOREIGN KEY ([UpdatedByUserID]) 
            REFERENCES [dbo].[TbUsers]([UserID]) ON DELETE SET NULL
    );
    
    PRINT 'Table TbSystemSettings created successfully.';
END
ELSE
BEGIN
    PRINT 'Table TbSystemSettings already exists.';
END
GO

-- ============================================================
-- 2. INSERT DEFAULT SETTINGS
-- ============================================================

-- Support Email Setting
IF NOT EXISTS (SELECT 1 FROM TbSystemSettings WHERE SettingKey = 'SupportEmail')
BEGIN
    INSERT INTO TbSystemSettings (SettingKey, SettingValue, SettingGroup, Description, DataType, IsPublic)
    VALUES ('SupportEmail', 'support@tijarahjo.com', 'Support', 'Support email address displayed to users', 'String', 1);
    PRINT 'Default SupportEmail setting inserted.';
END

-- Support WhatsApp Number Setting
IF NOT EXISTS (SELECT 1 FROM TbSystemSettings WHERE SettingKey = 'SupportWhatsApp')
BEGIN
    INSERT INTO TbSystemSettings (SettingKey, SettingValue, SettingGroup, Description, DataType, IsPublic)
    VALUES ('SupportWhatsApp', '962791234567', 'Support', 'Support WhatsApp number (Jordan format: 9627XXXXXXXX)', 'String', 1);
    PRINT 'Default SupportWhatsApp setting inserted.';
END

-- Site Name Setting
IF NOT EXISTS (SELECT 1 FROM TbSystemSettings WHERE SettingKey = 'SiteName')
BEGIN
    INSERT INTO TbSystemSettings (SettingKey, SettingValue, SettingGroup, Description, DataType, IsPublic)
    VALUES ('SiteName', 'TijarahJo', 'General', 'Platform name displayed in UI', 'String', 1);
    PRINT 'Default SiteName setting inserted.';
END

-- Maintenance Mode Setting
IF NOT EXISTS (SELECT 1 FROM TbSystemSettings WHERE SettingKey = 'MaintenanceMode')
BEGIN
    INSERT INTO TbSystemSettings (SettingKey, SettingValue, SettingGroup, Description, DataType, IsPublic)
    VALUES ('MaintenanceMode', 'false', 'General', 'Enable/disable maintenance mode', 'Bool', 1);
    PRINT 'Default MaintenanceMode setting inserted.';
END

-- Max Upload Size (MB)
IF NOT EXISTS (SELECT 1 FROM TbSystemSettings WHERE SettingKey = 'MaxUploadSizeMB')
BEGIN
    INSERT INTO TbSystemSettings (SettingKey, SettingValue, SettingGroup, Description, DataType, IsPublic)
    VALUES ('MaxUploadSizeMB', '5', 'Upload', 'Maximum file upload size in MB', 'Int', 1);
    PRINT 'Default MaxUploadSizeMB setting inserted.';
END

GO

-- ============================================================
-- 3. STORED PROCEDURES
-- ============================================================

-- Get All Settings
CREATE OR ALTER PROCEDURE [dbo].[SP_GetAllSettings]
    @PublicOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SettingID,
        SettingKey,
        SettingValue,
        SettingGroup,
        Description,
        DataType,
        IsPublic,
        CreatedAt,
        UpdatedAt,
        UpdatedByUserID
    FROM TbSystemSettings
    WHERE (@PublicOnly = 0 OR IsPublic = 1)
    ORDER BY SettingGroup, SettingKey;
END;
GO

-- Get Settings by Group
CREATE OR ALTER PROCEDURE [dbo].[SP_GetSettingsByGroup]
    @SettingGroup NVARCHAR(50),
    @PublicOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SettingID,
        SettingKey,
        SettingValue,
        SettingGroup,
        Description,
        DataType,
        IsPublic,
        CreatedAt,
        UpdatedAt,
        UpdatedByUserID
    FROM TbSystemSettings
    WHERE SettingGroup = @SettingGroup
      AND (@PublicOnly = 0 OR IsPublic = 1)
    ORDER BY SettingKey;
END;
GO

-- Get Setting by Key
CREATE OR ALTER PROCEDURE [dbo].[SP_GetSettingByKey]
    @SettingKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SettingID,
        SettingKey,
        SettingValue,
        SettingGroup,
        Description,
        DataType,
        IsPublic,
        CreatedAt,
        UpdatedAt,
        UpdatedByUserID
    FROM TbSystemSettings
    WHERE SettingKey = @SettingKey;
END;
GO

-- Update Setting by Key
CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateSettingByKey]
    @SettingKey NVARCHAR(100),
    @SettingValue NVARCHAR(500),
    @UpdatedByUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE TbSystemSettings
    SET SettingValue = @SettingValue,
        UpdatedAt = GETUTCDATE(),
        UpdatedByUserID = @UpdatedByUserID
    WHERE SettingKey = @SettingKey;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- Update Multiple Settings (Batch)
CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateSettings]
    @SettingsJson NVARCHAR(MAX),  -- JSON array: [{"key":"SupportEmail","value":"new@email.com"},...]
    @UpdatedByUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Parse JSON and update each setting
        UPDATE s
        SET s.SettingValue = j.value,
            s.UpdatedAt = GETUTCDATE(),
            s.UpdatedByUserID = @UpdatedByUserID
        FROM TbSystemSettings s
        INNER JOIN OPENJSON(@SettingsJson) 
            WITH (
                [key] NVARCHAR(100) '$.key',
                [value] NVARCHAR(500) '$.value'
            ) j ON s.SettingKey = j.[key];
        
        COMMIT TRANSACTION;
        
        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Get Support Contact Info (Optimized for public use)
CREATE OR ALTER PROCEDURE [dbo].[SP_GetSupportContactInfo]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SettingKey,
        SettingValue
    FROM TbSystemSettings
    WHERE SettingKey IN ('SupportEmail', 'SupportWhatsApp')
      AND IsPublic = 1;
END;
GO

-- Check if Setting Exists
CREATE OR ALTER PROCEDURE [dbo].[SP_DoesSettingExist]
    @SettingKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CAST(CASE WHEN EXISTS(
        SELECT 1 FROM TbSystemSettings WHERE SettingKey = @SettingKey
    ) THEN 1 ELSE 0 END AS BIT) AS SettingExists;
END;
GO

-- Create New Setting (Admin only)
CREATE OR ALTER PROCEDURE [dbo].[SP_CreateSetting]
    @SettingKey NVARCHAR(100),
    @SettingValue NVARCHAR(500),
    @SettingGroup NVARCHAR(50) = 'General',
    @Description NVARCHAR(255) = NULL,
    @DataType NVARCHAR(20) = 'String',
    @IsPublic BIT = 0,
    @CreatedByUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if key already exists
    IF EXISTS (SELECT 1 FROM TbSystemSettings WHERE SettingKey = @SettingKey)
    BEGIN
        RAISERROR('Setting key already exists', 16, 1);
        RETURN;
    END
    
    INSERT INTO TbSystemSettings (SettingKey, SettingValue, SettingGroup, Description, DataType, IsPublic, UpdatedByUserID)
    VALUES (@SettingKey, @SettingValue, @SettingGroup, @Description, @DataType, @IsPublic, @CreatedByUserID);
    
    SELECT SCOPE_IDENTITY() AS NewSettingID;
END;
GO

-- Delete Setting (Admin only - use with caution)
CREATE OR ALTER PROCEDURE [dbo].[SP_DeleteSetting]
    @SettingKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM TbSystemSettings WHERE SettingKey = @SettingKey;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- ============================================================
-- 4. CREATE INDEX FOR PERFORMANCE
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TbSystemSettings_Group')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TbSystemSettings_Group] 
    ON [dbo].[TbSystemSettings] ([SettingGroup], [IsPublic])
    INCLUDE ([SettingKey], [SettingValue]);
    
    PRINT 'Index IX_TbSystemSettings_Group created.';
END
GO

-- ============================================================
-- 5. VERIFY SETUP
-- ============================================================

PRINT '';
PRINT '============================================================';
PRINT 'SETUP COMPLETE - Verifying...';
PRINT '============================================================';

SELECT 'Settings Count' AS Info, COUNT(*) AS Value FROM TbSystemSettings
UNION ALL
SELECT 'Support Group Count', COUNT(*) FROM TbSystemSettings WHERE SettingGroup = 'Support'
UNION ALL
SELECT 'Public Settings Count', COUNT(*) FROM TbSystemSettings WHERE IsPublic = 1;

PRINT '';
PRINT 'Current Settings:';
SELECT SettingKey, SettingValue, SettingGroup, IsPublic FROM TbSystemSettings ORDER BY SettingGroup, SettingKey;

GO
