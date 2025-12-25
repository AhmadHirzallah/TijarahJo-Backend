-- =============================================
-- Post Image Stored Procedures for TijarahJoDB
-- Post Images are nested resources under Posts
-- 
-- NOTE: These are included in Database-Setup-Complete.sql
-- Run that file for full setup.
-- =============================================

USE [TijarahJoDB];
GO

-- SP: Get Images by Post ID (Primary use case)
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
