-- =============================================
-- TijarahJoDB - Complete Setup Script
-- Run this in SQL Server Management Studio
-- 
-- This script is SAFE to run multiple times
-- It checks before inserting/modifying
-- =============================================

USE [TijarahJoDB];
GO

PRINT '========================================';
PRINT 'Starting TijarahJoDB Setup Script';
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Seed Default Roles (if not exist)
-- =============================================
PRINT 'STEP 1: Checking/Seeding Roles...';

-- Check current roles
IF NOT EXISTS (SELECT 1 FROM TbRoles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO TbRoles (RoleName, CreatedAt, IsDeleted)
    VALUES ('Admin', GETUTCDATE(), 0);
    PRINT '  ? Admin role created';
END
ELSE
    PRINT '  ? Admin role already exists';

IF NOT EXISTS (SELECT 1 FROM TbRoles WHERE RoleName = 'Moderator')
BEGIN
    INSERT INTO TbRoles (RoleName, CreatedAt, IsDeleted)
    VALUES ('Moderator', GETUTCDATE(), 0);
    PRINT '  ? Moderator role created';
END
ELSE
    PRINT '  ? Moderator role already exists';

IF NOT EXISTS (SELECT 1 FROM TbRoles WHERE RoleName = 'User')
BEGIN
    INSERT INTO TbRoles (RoleName, CreatedAt, IsDeleted)
    VALUES ('User', GETUTCDATE(), 0);
    PRINT '  ? User role created';
END
ELSE
    PRINT '  ? User role already exists';

PRINT '';

-- =============================================
-- STEP 2: Get Admin RoleID for later use
-- =============================================
DECLARE @AdminRoleID INT;
SELECT @AdminRoleID = RoleID FROM TbRoles WHERE RoleName = 'Admin';
PRINT 'Admin RoleID is: ' + CAST(@AdminRoleID AS VARCHAR(10));
PRINT '';

-- =============================================
-- STEP 3: Create Admin User (if no admin exists)
-- NOTE: Password will be plain text - first login will hash it
-- =============================================
PRINT 'STEP 2: Checking/Creating Admin User...';

IF NOT EXISTS (SELECT 1 FROM TbUsers u INNER JOIN TbRoles r ON u.RoleID = r.RoleID WHERE r.RoleName = 'Admin')
BEGIN
    INSERT INTO TbUsers (Username, HashedPassword, Email, FirstName, LastName, JoinDate, Status, RoleID, IsDeleted)
    VALUES (
        'admin',
        'admin123',  -- Plain text for initial setup - will be hashed on first login
        'admin@tijarahjo.com',
        'System',
        'Administrator',
        GETUTCDATE(),
        1,           -- Status: Active
        @AdminRoleID,
        0            -- IsDeleted: false
    );
    PRINT '  ? Admin user created (username: admin, password: admin123)';
    PRINT '  ? NOTE: Password will be securely hashed on first login';
END
ELSE
    PRINT '  ? Admin user already exists';

PRINT '';

-- =============================================
-- STEP 4: User Stored Procedures
-- =============================================
PRINT 'STEP 3: Creating User Stored Procedures...';
GO

-- SP: Get User by Login (Username or Email) - For secure authentication
CREATE OR ALTER PROCEDURE [dbo].[SP_GetUserByLogin]
    @Login NVARCHAR(510)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        UserID,
        Username,
        HashedPassword,
        Email,
        FirstName,
        LastName,
        JoinDate,
        Status,
        RoleID,
        IsDeleted
    FROM TbUsers
    WHERE (Username = @Login OR Email = @Login)
      AND IsDeleted = 0;
END;
GO

PRINT '  ? SP_GetUserByLogin created';

-- =============================================
-- STEP 5: User Images Stored Procedures
-- =============================================
PRINT 'STEP 4: Creating User Images Stored Procedures...';
GO

-- SP: Get User Image by ID
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

-- SP: Add User Image
CREATE OR ALTER PROCEDURE [dbo].[SP_AddUserImage]
    @UserID INT,
    @ImageURL NVARCHAR(2000),
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

-- SP: Update User Image
CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateUserImage]
    @UserImageID INT,
    @UserID INT,
    @ImageURL NVARCHAR(2000),
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

-- SP: Delete User Image (Soft Delete)
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

-- SP: Does User Image Exist
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

-- SP: Get All User Images (Admin use)
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

-- SP: Get Images by User ID (Primary use for nested resource)
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

PRINT '  ? User Images stored procedures created';

-- =============================================
-- STEP 6: Post Images Stored Procedures
-- =============================================
PRINT 'STEP 5: Creating Post Images Stored Procedures...';
GO

-- SP: Get Images by Post ID
CREATE OR ALTER PROCEDURE [dbo].[SP_GetPostImagesByPostID]
    @PostID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        PostImageID, 
        PostID, 
        PostImageURL, 
        UploadedAt, 
        IsDeleted
    FROM TbPostImages
    WHERE PostID = @PostID AND IsDeleted = 0
    ORDER BY UploadedAt ASC, PostImageID ASC;
END;
GO

-- SP: Get Primary (First) Post Image
CREATE OR ALTER PROCEDURE [dbo].[SP_GetPostPrimaryImage]
    @PostID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        PostImageID, 
        PostID, 
        PostImageURL, 
        UploadedAt, 
        IsDeleted
    FROM TbPostImages
    WHERE PostID = @PostID AND IsDeleted = 0
    ORDER BY UploadedAt ASC, PostImageID ASC;
END;
GO

-- SP: Delete Post Image (Soft Delete)
CREATE OR ALTER PROCEDURE [dbo].[SP_DeletePostImage]
    @PostImageID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TbPostImages
    SET IsDeleted = 1
    WHERE PostImageID = @PostImageID;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- SP: Update Post Image
CREATE OR ALTER PROCEDURE [dbo].[SP_UpdatePostImage]
    @PostImageID INT,
    @PostID INT,
    @PostImageURL NVARCHAR(1000),
    @UploadedAt DATETIME2,
    @IsDeleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TbPostImages
    SET PostID = @PostID,
        PostImageURL = @PostImageURL,
        UploadedAt = @UploadedAt,
        IsDeleted = @IsDeleted
    WHERE PostImageID = @PostImageID;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

PRINT '  ? Post Images stored procedures created';

-- =============================================
-- STEP 7: Verification
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICATION';
PRINT '========================================';
PRINT '';

-- Show Roles
PRINT 'Current Roles:';
SELECT RoleID, RoleName, IsDeleted FROM TbRoles ORDER BY RoleID;

-- Show Admin Users
PRINT '';
PRINT 'Admin Users:';
SELECT 
    u.UserID,
    u.Username,
    u.Email,
    u.FirstName + ' ' + ISNULL(u.LastName, '') AS FullName,
    r.RoleName,
    u.Status,
    u.IsDeleted
FROM TbUsers u
INNER JOIN TbRoles r ON u.RoleID = r.RoleID
WHERE r.RoleName = 'Admin';

-- Show all stored procedures
PRINT '';
PRINT 'Security & Image Stored Procedures:';
SELECT name AS StoredProcedure
FROM sys.procedures 
WHERE name LIKE 'SP_%Image%' OR name LIKE 'SP_%Login%' OR name LIKE 'SP_%User%'
ORDER BY name;

PRINT '';
PRINT '========================================';
PRINT 'Setup Complete!';
PRINT '========================================';
PRINT '';
PRINT 'You can now login with:';
PRINT '  Username: admin';
PRINT '  Password: admin123';
PRINT '';
PRINT 'SECURITY NOTE:';
PRINT '  - Passwords are hashed using PBKDF2-HMAC-SHA256';
PRINT '  - Legacy plain text passwords will be auto-migrated on login';
PRINT '  - 100,000 iterations for brute-force protection';
PRINT '';
GO
