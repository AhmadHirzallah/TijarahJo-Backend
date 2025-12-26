-- =============================================
-- User Image Stored Procedures for TijarahJoDB
-- User Images are nested resources under Users
-- =============================================

USE [TijarahJoDB];
GO

-- =============================================
-- Table: TbUserImages (if not exists)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TbUserImages' AND xtype='U')
BEGIN
    CREATE TABLE dbo.TbUserImages
    (
        UserImageID   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TbUserImages PRIMARY KEY,
        UserID        INT NOT NULL,
        ImageURL      NVARCHAR(1000) NOT NULL,
        UploadedAt    DATETIME2(7) NOT NULL CONSTRAINT DF_TbUserImages_UploadedAt DEFAULT (SYSUTCDATETIME()),
        IsDeleted     BIT NOT NULL CONSTRAINT DF_TbUserImages_IsDeleted DEFAULT (0)
    );

    ALTER TABLE dbo.TbUserImages
    ADD CONSTRAINT FK_TbUserImages_TbUsers
    FOREIGN KEY (UserID) REFERENCES dbo.TbUsers(UserID);

    -- Index for fast fetch of user images
    CREATE INDEX IX_TbUserImages_UserID
    ON dbo.TbUserImages(UserID, IsDeleted, UploadedAt DESC)
    INCLUDE (ImageURL);
END;
GO

-- =============================================
-- SP: Get User Image by ID
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetUserImageByID]
    @UserImageID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        UserImageID, 
        UserID, 
        ImageURL, 
        UploadedAt, 
        IsDeleted
    FROM TbUserImages
    WHERE UserImageID = @UserImageID;
END;
GO

-- =============================================
-- SP: Add User Image
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_AddUserImage]
    @UserID INT,
    @ImageURL NVARCHAR(1000),
    @UploadedAt DATETIME2,
    @IsDeleted BIT,
    @NewUserImageID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO TbUserImages (UserID, ImageURL, UploadedAt, IsDeleted)
    VALUES (@UserID, @ImageURL, @UploadedAt, @IsDeleted);

    SET @NewUserImageID = SCOPE_IDENTITY();
END;
GO

-- =============================================
-- SP: Update User Image
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateUserImage]
    @UserImageID INT,
    @UserID INT,
    @ImageURL NVARCHAR(1000),
    @UploadedAt DATETIME2,
    @IsDeleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TbUserImages
    SET UserID = @UserID,
        ImageURL = @ImageURL,
        UploadedAt = @UploadedAt,
        IsDeleted = @IsDeleted
    WHERE UserImageID = @UserImageID;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- =============================================
-- SP: Delete User Image (Soft Delete)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_DeleteUserImage]
    @UserImageID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TbUserImages
    SET IsDeleted = 1
    WHERE UserImageID = @UserImageID;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- =============================================
-- SP: Does User Image Exist
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_DoesUserImageExist]
    @UserImageID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM TbUserImages WHERE UserImageID = @UserImageID AND IsDeleted = 0)
        SELECT CAST(1 AS BIT) AS Found;
    ELSE
        SELECT CAST(0 AS BIT) AS Found;
END;
GO

-- =============================================
-- SP: Get All User Images (Admin use)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetAllTbUserImages]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        UserImageID, 
        UserID, 
        ImageURL, 
        UploadedAt, 
        IsDeleted
    FROM TbUserImages
    WHERE IsDeleted = 0
    ORDER BY UploadedAt DESC;
END;
GO

-- =============================================
-- SP: Get Images by User ID (Primary use case)
-- This is the main SP since images are nested under users
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetUserImagesByUserID]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        UserImageID, 
        UserID, 
        ImageURL, 
        UploadedAt, 
        IsDeleted
    FROM TbUserImages
    WHERE UserID = @UserID AND IsDeleted = 0
    ORDER BY UploadedAt DESC;
END;
GO

-- =============================================
-- SP: Get Primary (Latest) User Image
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetUserPrimaryImage]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        UserImageID, 
        UserID, 
        ImageURL, 
        UploadedAt, 
        IsDeleted
    FROM TbUserImages
    WHERE UserID = @UserID AND IsDeleted = 0
    ORDER BY UploadedAt DESC;
END;
GO
