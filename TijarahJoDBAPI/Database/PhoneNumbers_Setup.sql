-- ============================================
-- TIJARAHJO - USER PHONE NUMBERS SETUP
-- Simplified version without PhoneType
-- One-to-Many: One User can have Many Phone Numbers
-- ============================================

-- ============================================
-- STEP 1: CREATE THE PHONE NUMBERS TABLE
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TbUserPhoneNumbers')
BEGIN
    CREATE TABLE [dbo].[TbUserPhoneNumbers]
    (
        [PhoneID]       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserID]        INT NOT NULL,
        [PhoneNumber]   NVARCHAR(20) NOT NULL,
        [IsPrimary]     BIT NOT NULL DEFAULT(0),
        [CreatedAt]     DATETIME2 NOT NULL DEFAULT(SYSUTCDATETIME()),
        [IsDeleted]     BIT NOT NULL DEFAULT(0),
        
        -- Foreign Key to TbUsers
        CONSTRAINT [FK_TbUserPhoneNumbers_TbUsers] 
            FOREIGN KEY ([UserID]) REFERENCES [dbo].[TbUsers]([UserID])
            ON DELETE CASCADE
    );
    
    -- Index for faster lookups by UserID
    CREATE NONCLUSTERED INDEX [IX_TbUserPhoneNumbers_UserID] 
        ON [dbo].[TbUserPhoneNumbers]([UserID]);
        
    PRINT 'Table TbUserPhoneNumbers created successfully.';
END
ELSE
BEGIN
    PRINT 'Table TbUserPhoneNumbers already exists.';
    -- Remove PhoneType column if it exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TbUserPhoneNumbers') AND name = 'PhoneType')
    BEGIN
        ALTER TABLE TbUserPhoneNumbers DROP COLUMN PhoneType;
        PRINT 'Removed PhoneType column.';
    END
END
GO

-- ============================================
-- STEP 2: DROP OLD SP_AddUser (conflicts with system)
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_AddUser')
BEGIN
    DROP PROCEDURE [dbo].[SP_AddUser];
    PRINT 'Dropped old SP_AddUser procedure.';
END
GO

-- ============================================
-- STEP 3: CREATE SP_CreateUser (New User Only)
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CreateUser')
    DROP PROCEDURE [dbo].[SP_CreateUser];
GO

CREATE PROCEDURE [dbo].[SP_CreateUser]
    @Username NVARCHAR(200),
    @HashedPassword NVARCHAR(510),
    @Email NVARCHAR(510),
    @FirstName NVARCHAR(200),
    @LastName NVARCHAR(200) = NULL,
    @JoinDate DATETIME2 = NULL,
    @Status INT = 1,
    @RoleID INT,
    @IsDeleted BIT = 0,
    @NewUserID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @JoinDate IS NULL
        SET @JoinDate = SYSUTCDATETIME();
    
    INSERT INTO [dbo].[TbUsers] 
        (Username, HashedPassword, Email, FirstName, LastName, JoinDate, Status, RoleID, IsDeleted)
    VALUES 
        (@Username, @HashedPassword, @Email, @FirstName, @LastName, @JoinDate, @Status, @RoleID, @IsDeleted);
    
    SET @NewUserID = SCOPE_IDENTITY();
END
GO

PRINT 'Created SP_CreateUser procedure.';
GO

-- ============================================
-- STEP 4: CREATE SP_AddUserPhoneNumber
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_AddUserPhoneNumber')
    DROP PROCEDURE [dbo].[SP_AddUserPhoneNumber];
GO

CREATE PROCEDURE [dbo].[SP_AddUserPhoneNumber]
    @UserID INT,
    @PhoneNumber NVARCHAR(20),
    @IsPrimary BIT = 0,
    @NewPhoneID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- If this is set as primary, unset other primary numbers for this user
    IF @IsPrimary = 1
    BEGIN
        UPDATE [dbo].[TbUserPhoneNumbers]
        SET IsPrimary = 0
        WHERE UserID = @UserID AND IsDeleted = 0;
    END
    
    INSERT INTO [dbo].[TbUserPhoneNumbers] 
        (UserID, PhoneNumber, IsPrimary, CreatedAt, IsDeleted)
    VALUES 
        (@UserID, @PhoneNumber, @IsPrimary, SYSUTCDATETIME(), 0);
    
    SET @NewPhoneID = SCOPE_IDENTITY();
END
GO

PRINT 'Created SP_AddUserPhoneNumber procedure.';
GO

-- ============================================
-- STEP 5: CREATE SP_GetUserPhoneNumbers
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserPhoneNumbers')
    DROP PROCEDURE [dbo].[SP_GetUserPhoneNumbers];
GO

CREATE PROCEDURE [dbo].[SP_GetUserPhoneNumbers]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PhoneID,
        UserID,
        PhoneNumber,
        IsPrimary,
        CreatedAt,
        IsDeleted
    FROM [dbo].[TbUserPhoneNumbers]
    WHERE UserID = @UserID AND IsDeleted = 0
    ORDER BY IsPrimary DESC, CreatedAt ASC;
END
GO

PRINT 'Created SP_GetUserPhoneNumbers procedure.';
GO

-- ============================================
-- STEP 6: CREATE SP_GetUserPhoneNumberByID
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserPhoneNumberByID')
    DROP PROCEDURE [dbo].[SP_GetUserPhoneNumberByID];
GO

CREATE PROCEDURE [dbo].[SP_GetUserPhoneNumberByID]
    @PhoneID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PhoneID,
        UserID,
        PhoneNumber,
        IsPrimary,
        CreatedAt,
        IsDeleted
    FROM [dbo].[TbUserPhoneNumbers]
    WHERE PhoneID = @PhoneID;
END
GO

PRINT 'Created SP_GetUserPhoneNumberByID procedure.';
GO

-- ============================================
-- STEP 7: CREATE SP_UpdateUserPhoneNumber
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_UpdateUserPhoneNumber')
    DROP PROCEDURE [dbo].[SP_UpdateUserPhoneNumber];
GO

CREATE PROCEDURE [dbo].[SP_UpdateUserPhoneNumber]
    @PhoneID INT,
    @PhoneNumber NVARCHAR(20),
    @IsPrimary BIT = 0,
    @IsDeleted BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserID INT;
    SELECT @UserID = UserID FROM [dbo].[TbUserPhoneNumbers] WHERE PhoneID = @PhoneID;
    
    -- If setting as primary, unset other primary numbers for this user
    IF @IsPrimary = 1 AND @IsDeleted = 0
    BEGIN
        UPDATE [dbo].[TbUserPhoneNumbers]
        SET IsPrimary = 0
        WHERE UserID = @UserID AND PhoneID != @PhoneID AND IsDeleted = 0;
    END
    
    UPDATE [dbo].[TbUserPhoneNumbers]
    SET 
        PhoneNumber = @PhoneNumber,
        IsPrimary = @IsPrimary,
        IsDeleted = @IsDeleted
    WHERE PhoneID = @PhoneID;
    
    SELECT @@ROWCOUNT;
END
GO

PRINT 'Created SP_UpdateUserPhoneNumber procedure.';
GO

-- ============================================
-- STEP 8: CREATE SP_DeleteUserPhoneNumber
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_DeleteUserPhoneNumber')
    DROP PROCEDURE [dbo].[SP_DeleteUserPhoneNumber];
GO

CREATE PROCEDURE [dbo].[SP_DeleteUserPhoneNumber]
    @PhoneID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[TbUserPhoneNumbers]
    SET IsDeleted = 1, IsPrimary = 0
    WHERE PhoneID = @PhoneID;
    
    SELECT @@ROWCOUNT;
END
GO

PRINT 'Created SP_DeleteUserPhoneNumber procedure.';
GO

-- ============================================
-- STEP 9: CREATE SP_DoesUserPhoneNumberExist
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_DoesUserPhoneNumberExist')
    DROP PROCEDURE [dbo].[SP_DoesUserPhoneNumberExist];
GO

CREATE PROCEDURE [dbo].[SP_DoesUserPhoneNumberExist]
    @PhoneID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [dbo].[TbUserPhoneNumbers] WHERE PhoneID = @PhoneID AND IsDeleted = 0)
        SELECT CAST(1 AS BIT);
    ELSE
        SELECT CAST(0 AS BIT);
END
GO

PRINT 'Created SP_DoesUserPhoneNumberExist procedure.';
GO

-- ============================================
-- STEP 10: UPDATE SP_GetUserByID
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserByID')
    DROP PROCEDURE [dbo].[SP_GetUserByID];
GO

CREATE PROCEDURE [dbo].[SP_GetUserByID]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserID,
        u.Username,
        u.HashedPassword,
        u.Email,
        u.FirstName,
        u.LastName,
        u.JoinDate,
        u.Status,
        u.RoleID,
        u.IsDeleted,
        (SELECT TOP 1 PhoneNumber FROM TbUserPhoneNumbers WHERE UserID = u.UserID AND IsPrimary = 1 AND IsDeleted = 0) AS PrimaryPhone
    FROM [dbo].[TbUsers] u
    WHERE u.UserID = @UserID;
END
GO

PRINT 'Updated SP_GetUserByID procedure.';
GO

-- ============================================
-- STEP 11: UPDATE SP_GetUserByLogin
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserByLogin')
    DROP PROCEDURE [dbo].[SP_GetUserByLogin];
GO

CREATE PROCEDURE [dbo].[SP_GetUserByLogin]
    @Login NVARCHAR(510)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserID,
        u.Username,
        u.HashedPassword,
        u.Email,
        u.FirstName,
        u.LastName,
        u.JoinDate,
        u.Status,
        u.RoleID,
        u.IsDeleted,
        (SELECT TOP 1 PhoneNumber FROM TbUserPhoneNumbers WHERE UserID = u.UserID AND IsPrimary = 1 AND IsDeleted = 0) AS PrimaryPhone
    FROM [dbo].[TbUsers] u
    WHERE (u.Username = @Login OR u.Email = @Login) AND u.IsDeleted = 0;
END
GO

PRINT 'Updated SP_GetUserByLogin procedure.';
GO

-- ============================================
-- STEP 12: UPDATE SP_GetAllTbUsers
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetAllTbUsers')
    DROP PROCEDURE [dbo].[SP_GetAllTbUsers];
GO

CREATE PROCEDURE [dbo].[SP_GetAllTbUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserID,
        u.Username,
        u.HashedPassword,
        u.Email,
        u.FirstName,
        u.LastName,
        u.JoinDate,
        u.Status,
        u.RoleID,
        u.IsDeleted,
        (SELECT TOP 1 PhoneNumber FROM TbUserPhoneNumbers WHERE UserID = u.UserID AND IsPrimary = 1 AND IsDeleted = 0) AS PrimaryPhone
    FROM [dbo].[TbUsers] u
    ORDER BY u.UserID;
END
GO

PRINT 'Updated SP_GetAllTbUsers procedure.';
GO

-- ============================================
-- STEP 13: CREATE SP_CreateUserWithPhone
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CreateUserWithPhone')
    DROP PROCEDURE [dbo].[SP_CreateUserWithPhone];
GO

CREATE PROCEDURE [dbo].[SP_CreateUserWithPhone]
    @Username NVARCHAR(200),
    @HashedPassword NVARCHAR(510),
    @Email NVARCHAR(510),
    @FirstName NVARCHAR(200),
    @LastName NVARCHAR(200) = NULL,
    @PhoneNumber NVARCHAR(20) = NULL,
    @JoinDate DATETIME2 = NULL,
    @Status INT = 1,
    @RoleID INT,
    @IsDeleted BIT = 0,
    @NewUserID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF @JoinDate IS NULL
            SET @JoinDate = SYSUTCDATETIME();
        
        -- Insert User
        INSERT INTO [dbo].[TbUsers] 
            (Username, HashedPassword, Email, FirstName, LastName, JoinDate, Status, RoleID, IsDeleted)
        VALUES 
            (@Username, @HashedPassword, @Email, @FirstName, @LastName, @JoinDate, @Status, @RoleID, @IsDeleted);
        
        SET @NewUserID = SCOPE_IDENTITY();
        
        -- Insert Phone Number if provided (as primary)
        IF @PhoneNumber IS NOT NULL AND LEN(TRIM(@PhoneNumber)) > 0
        BEGIN
            INSERT INTO [dbo].[TbUserPhoneNumbers] 
                (UserID, PhoneNumber, IsPrimary, CreatedAt, IsDeleted)
            VALUES 
                (@NewUserID, @PhoneNumber, 1, SYSUTCDATETIME(), 0);
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Created SP_CreateUserWithPhone procedure.';
GO

-- ============================================
-- VERIFICATION
-- ============================================
PRINT '';
PRINT '============================================';
PRINT 'SETUP COMPLETE!';
PRINT '============================================';
PRINT '';

-- Show the table structure
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.is_nullable,
    c.is_identity
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('TbUserPhoneNumbers')
ORDER BY c.column_id;

-- Show all procedures
SELECT name AS ProcedureName, create_date, modify_date
FROM sys.procedures
WHERE name LIKE '%User%' OR name LIKE '%Phone%'
ORDER BY name;
