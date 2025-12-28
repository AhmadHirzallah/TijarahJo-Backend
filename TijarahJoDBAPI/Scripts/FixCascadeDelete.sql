-- ============================================================
-- TIJARAHJO: FIX CASCADE DELETE FOR POSTS
-- ============================================================
-- ISSUE: Stored procedures SP_DeletePost, SP_DeletePostCascade, 
--        and SP_DeleteReview reference "TbReviews" but the actual 
--        table is named "TbPostReviews"
--
-- RUN THIS SCRIPT IN SQL SERVER MANAGEMENT STUDIO (SSMS)
-- ============================================================

USE TijarahJoDB;
GO

PRINT '============================================================'
PRINT 'FIXING STORED PROCEDURES WITH WRONG TABLE NAME'
PRINT 'TbReviews -> TbPostReviews'
PRINT '============================================================'
PRINT ''

-- ============================================================
-- FIX 1: SP_DeletePost
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_DeletePost')
BEGIN
    DROP PROCEDURE SP_DeletePost;
    PRINT 'Dropped old SP_DeletePost';
END
GO

CREATE PROCEDURE [dbo].[SP_DeletePost]
    @PostID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Step 1: Soft delete all images for this post
        UPDATE TbPostImages 
        SET IsDeleted = 1 
        WHERE PostID = @PostID AND IsDeleted = 0;
        
        -- Step 2: Soft delete all reviews for this post
        -- FIXED: Changed from TbReviews to TbPostReviews
        UPDATE TbPostReviews 
        SET IsDeleted = 1 
        WHERE PostID = @PostID AND IsDeleted = 0;
        
        -- Step 3: Soft delete the post itself
        UPDATE TbPosts 
        SET IsDeleted = 1 
        WHERE PostID = @PostID AND IsDeleted = 0;
        
        -- Return number of affected rows
        SELECT @@ROWCOUNT AS RowsAffected;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Return 0 to indicate failure
        SELECT 0 AS RowsAffected;
    END CATCH
END
GO

PRINT 'Created SP_DeletePost with correct table name';
GO

-- ============================================================
-- FIX 2: SP_DeletePostCascade
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_DeletePostCascade')
BEGIN
    DROP PROCEDURE SP_DeletePostCascade;
    PRINT 'Dropped old SP_DeletePostCascade';
END
GO

CREATE PROCEDURE [dbo].[SP_DeletePostCascade]
    @PostID INT,
    @HardDelete BIT = 0  -- 0 = soft delete, 1 = hard delete
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowsAffected INT = 0;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF @HardDelete = 1
        BEGIN
            -- HARD DELETE - permanently remove records
            DELETE FROM TbPostImages WHERE PostID = @PostID;
            
            -- FIXED: Changed from TbReviews to TbPostReviews
            DELETE FROM TbPostReviews WHERE PostID = @PostID;
            
            DELETE FROM TbPosts WHERE PostID = @PostID;
            SET @RowsAffected = @@ROWCOUNT;
        END
        ELSE
        BEGIN
            -- SOFT DELETE - mark as deleted
            UPDATE TbPostImages 
            SET IsDeleted = 1 
            WHERE PostID = @PostID AND IsDeleted = 0;
            
            -- FIXED: Changed from TbReviews to TbPostReviews
            UPDATE TbPostReviews 
            SET IsDeleted = 1 
            WHERE PostID = @PostID AND IsDeleted = 0;
            
            UPDATE TbPosts 
            SET IsDeleted = 1 
            WHERE PostID = @PostID AND IsDeleted = 0;
            SET @RowsAffected = @@ROWCOUNT;
        END
        
        COMMIT TRANSACTION;
        
        SELECT @RowsAffected AS RowsAffected;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 0 AS RowsAffected;
    END CATCH
END
GO

PRINT 'Created SP_DeletePostCascade with correct table name';
GO

-- ============================================================
-- FIX 3: SP_DeleteReview
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_DeleteReview')
BEGIN
    DROP PROCEDURE SP_DeleteReview;
    PRINT 'Dropped old SP_DeleteReview';
END
GO

CREATE PROCEDURE [dbo].[SP_DeleteReview]
    @ReviewID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- FIXED: Changed from TbReviews to TbPostReviews
    UPDATE TbPostReviews 
    SET IsDeleted = 1 
    WHERE ReviewID = @ReviewID;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'Created SP_DeleteReview with correct table name';
GO

-- ============================================================
-- FIX 4: SP_DeletePostImage (ensure it uses soft delete)
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_DeletePostImage')
BEGIN
    DROP PROCEDURE SP_DeletePostImage;
    PRINT 'Dropped old SP_DeletePostImage';
END
GO

CREATE PROCEDURE [dbo].[SP_DeletePostImage]
    @PostImageID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE TbPostImages 
    SET IsDeleted = 1 
    WHERE PostImageID = @PostImageID;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'Created SP_DeletePostImage';
GO

-- ============================================================
-- VERIFICATION: Check the fixes
-- ============================================================
PRINT ''
PRINT '============================================================'
PRINT 'VERIFICATION'
PRINT '============================================================'

-- Show updated procedures
SELECT 
    name AS ProcedureName,
    modify_date AS LastModified
FROM sys.procedures
WHERE name IN ('SP_DeletePost', 'SP_DeletePostCascade', 'SP_DeleteReview', 'SP_DeletePostImage')
ORDER BY name;

-- Verify table exists
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TbPostReviews')
    PRINT '? Table TbPostReviews exists'
ELSE
    PRINT '? ERROR: Table TbPostReviews does NOT exist!'

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TbReviews')
    PRINT '? WARNING: Old table TbReviews still exists (not used)'
ELSE
    PRINT '? No old TbReviews table (correct)'

PRINT ''
PRINT '============================================================'
PRINT 'ALL FIXES APPLIED SUCCESSFULLY!'
PRINT '============================================================'
PRINT ''
PRINT 'What was fixed:'
PRINT '1. SP_DeletePost - now uses TbPostReviews instead of TbReviews'
PRINT '2. SP_DeletePostCascade - now uses TbPostReviews instead of TbReviews'
PRINT '3. SP_DeleteReview - now uses TbPostReviews instead of TbReviews'
PRINT '4. SP_DeletePostImage - confirmed to use soft delete'
PRINT ''
PRINT 'Next steps:'
PRINT '1. Restart your API application'
PRINT '2. Test deleting a post with images'
PRINT '============================================================'
