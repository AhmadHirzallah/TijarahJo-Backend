-- =============================================
-- Review Stored Procedures for TijarahJoDB
-- Reviews are nested resources under Posts
-- =============================================

USE [TijarahJoDB];
GO

-- =============================================
-- Table: TbPostReviews (if not exists)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TbPostReviews' AND xtype='U')
BEGIN
    CREATE TABLE TbPostReviews (
        ReviewID INT IDENTITY(1,1) PRIMARY KEY,
        PostID INT NOT NULL FOREIGN KEY REFERENCES TbPosts(PostID),
        UserID INT NOT NULL FOREIGN KEY REFERENCES TbUsers(UserID),
        Rating TINYINT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
        ReviewText NVARCHAR(1000) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0,
        
        -- Ensure one review per user per post
        CONSTRAINT UQ_PostReviews_User_Post UNIQUE (PostID, UserID)
    );

    -- Indexes for common queries
    CREATE INDEX IX_TbPostReviews_PostID ON TbPostReviews(PostID) INCLUDE (Rating, IsDeleted);
    CREATE INDEX IX_TbPostReviews_UserID ON TbPostReviews(UserID);
    CREATE INDEX IX_TbPostReviews_Rating ON TbPostReviews(Rating) WHERE IsDeleted = 0;
END;
GO

-- =============================================
-- SP: Get Review by ID
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetReviewByID]
    @ReviewID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ReviewID, 
        PostID, 
        UserID, 
        Rating, 
        ReviewText, 
        CreatedAt, 
        IsDeleted
    FROM TbPostReviews
    WHERE ReviewID = @ReviewID;
END;
GO

-- =============================================
-- SP: Add Review
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_AddReview]
    @PostID INT,
    @UserID INT,
    @Rating TINYINT,
    @ReviewText NVARCHAR(1000),
    @CreatedAt DATETIME,
    @IsDeleted BIT,
    @NewReviewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if user already reviewed this post
    IF EXISTS (SELECT 1 FROM TbPostReviews WHERE PostID = @PostID AND UserID = @UserID AND IsDeleted = 0)
    BEGIN
        SET @NewReviewID = -1; -- Indicate duplicate
        RETURN;
    END

    INSERT INTO TbPostReviews (PostID, UserID, Rating, ReviewText, CreatedAt, IsDeleted)
    VALUES (@PostID, @UserID, @Rating, @ReviewText, @CreatedAt, @IsDeleted);

    SET @NewReviewID = SCOPE_IDENTITY();
END;
GO

-- =============================================
-- SP: Update Review
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateReview]
    @ReviewID INT,
    @PostID INT,
    @UserID INT,
    @Rating TINYINT,
    @ReviewText NVARCHAR(1000),
    @CreatedAt DATETIME,
    @IsDeleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TbPostReviews
    SET PostID = @PostID,
        UserID = @UserID,
        Rating = @Rating,
        ReviewText = @ReviewText,
        CreatedAt = @CreatedAt,
        IsDeleted = @IsDeleted
    WHERE ReviewID = @ReviewID;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- =============================================
-- SP: Delete Review (Soft Delete)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_DeleteReview]
    @ReviewID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TbPostReviews
    SET IsDeleted = 1
    WHERE ReviewID = @ReviewID;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- =============================================
-- SP: Does Review Exist
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_DoesReviewExist]
    @ReviewID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM TbPostReviews WHERE ReviewID = @ReviewID AND IsDeleted = 0)
        SELECT CAST(1 AS BIT) AS Found;
    ELSE
        SELECT CAST(0 AS BIT) AS Found;
END;
GO

-- =============================================
-- SP: Get All Reviews (Admin use)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetAllTbPostReviews]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ReviewID, 
        PostID, 
        UserID, 
        Rating, 
        ReviewText, 
        CreatedAt, 
        IsDeleted
    FROM TbPostReviews
    WHERE IsDeleted = 0
    ORDER BY CreatedAt DESC;
END;
GO

-- =============================================
-- SP: Get Reviews by Post ID (Primary use case)
-- This is the main SP since reviews are nested under posts
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetReviewsByPostID]
    @PostID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ReviewID, 
        PostID, 
        UserID, 
        Rating, 
        ReviewText, 
        CreatedAt, 
        IsDeleted
    FROM TbPostReviews
    WHERE PostID = @PostID AND IsDeleted = 0
    ORDER BY CreatedAt DESC;
END;
GO

-- =============================================
-- SP: Get Review Statistics for a Post
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetPostReviewStats]
    @PostID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        @PostID AS PostID,
        COUNT(*) AS TotalReviews,
        AVG(CAST(Rating AS DECIMAL(3,2))) AS AverageRating,
        SUM(CASE WHEN Rating = 5 THEN 1 ELSE 0 END) AS FiveStarCount,
        SUM(CASE WHEN Rating = 4 THEN 1 ELSE 0 END) AS FourStarCount,
        SUM(CASE WHEN Rating = 3 THEN 1 ELSE 0 END) AS ThreeStarCount,
        SUM(CASE WHEN Rating = 2 THEN 1 ELSE 0 END) AS TwoStarCount,
        SUM(CASE WHEN Rating = 1 THEN 1 ELSE 0 END) AS OneStarCount
    FROM TbPostReviews
    WHERE PostID = @PostID AND IsDeleted = 0;
END;
GO

-- =============================================
-- SP: Get Post Details with Reviews and Images
-- Returns 3 result sets (Post, Reviews, Images)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetPostDetails_All]
(
    @PostID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Result Set 1: Post + Owner + Category + Role (ONE row)
    SELECT
        p.PostID,
        p.UserID AS OwnerUserID,
        u.Username AS OwnerUsername,
        u.Email AS OwnerEmail,
        u.FirstName AS OwnerFirstName,
        u.LastName AS OwnerLastName,
        CONCAT(u.FirstName, N' ', ISNULL(u.LastName, N'')) AS OwnerFullName,
        u.RoleID,
        ro.RoleName,

        p.CategoryID,
        c.CategoryName,

        p.PostTitle,
        p.PostDescription,
        p.Price,
        p.Status,
        p.CreatedAt,
        p.IsDeleted
    FROM dbo.TbPosts p
    JOIN dbo.TbUsers u ON u.UserID = p.UserID
    JOIN dbo.TbRoles ro ON ro.RoleID = u.RoleID
    JOIN dbo.TbItemCategories c ON c.CategoryID = p.CategoryID
    WHERE p.PostID = @PostID AND p.IsDeleted = 0;

    -- Result Set 2: Reviews with reviewer info (MANY rows)
    SELECT
        rv.ReviewID,
        rv.PostID,
        rv.UserID AS ReviewerUserID,
        reviewer.Username AS ReviewerUsername,
        reviewer.Email AS ReviewerEmail,
        reviewer.FirstName AS ReviewerFirstName,
        reviewer.LastName AS ReviewerLastName,
        CONCAT(reviewer.FirstName, N' ', ISNULL(reviewer.LastName, N'')) AS ReviewerFullName,
        rv.Rating,
        rv.ReviewText,
        rv.CreatedAt
    FROM dbo.TbPostReviews rv
    JOIN dbo.TbUsers reviewer ON reviewer.UserID = rv.UserID
    WHERE rv.PostID = @PostID AND rv.IsDeleted = 0
    ORDER BY rv.CreatedAt DESC, rv.ReviewID DESC;

    -- Result Set 3: Images (MANY rows)
    SELECT
        i.PostImageID,
        i.PostID,
        i.PostImageURL,
        i.UploadedAt
    FROM dbo.TbPostImages i
    WHERE i.PostID = @PostID AND i.IsDeleted = 0
    ORDER BY i.UploadedAt ASC, i.PostImageID ASC;
END;
GO

-- =============================================
-- View: VW_PostsWithReviewStats
-- Useful for listing posts with their review summary
-- =============================================
CREATE OR ALTER VIEW [dbo].[VW_PostsWithReviewStats]
AS
SELECT
    p.PostID,
    p.UserID,
    p.CategoryID,
    p.PostTitle,
    p.PostDescription,
    p.Price,
    p.Status,
    p.CreatedAt,
    p.IsDeleted,
    ISNULL(rs.ReviewCount, 0) AS ReviewCount,
    rs.AverageRating
FROM dbo.TbPosts p
LEFT JOIN (
    SELECT 
        PostID,
        COUNT(*) AS ReviewCount,
        AVG(CAST(Rating AS DECIMAL(3,2))) AS AverageRating
    FROM dbo.TbPostReviews
    WHERE IsDeleted = 0
    GROUP BY PostID
) rs ON rs.PostID = p.PostID;
GO
