-- =============================================
-- View: VW_PostsForListing
-- Database: TijarahJoDB
-- Purpose: Enriched posts view for listing pages
-- =============================================

USE [TijarahJoDB];
GO

CREATE OR ALTER VIEW [dbo].[VW_PostsForListing]
AS
SELECT
    -- Post columns
    p.PostID,
    p.PostTitle,
    p.PostDescription,
    p.Price,
    p.Status,
    p.CreatedAt,
    p.IsDeleted,

    -- User columns
    p.UserID,
    u.Username,
    u.Email,
    u.FirstName,
    u.LastName,

    -- Role columns
    u.RoleID,
    r.RoleName,

    -- Category columns
    p.CategoryID,
    c.CategoryName
FROM dbo.TbPosts p
INNER JOIN dbo.TbUsers u ON p.UserID = u.UserID
INNER JOIN dbo.TbRoles r ON u.RoleID = r.RoleID
INNER JOIN dbo.TbItemCategories c ON p.CategoryID = c.CategoryID;
GO


-- =============================================
-- Stored Procedure: SP_GetTbPostsPaged (Updated)
-- Database: TijarahJoDB
-- Purpose: Paginated posts with category filtering
-- =============================================

USE [TijarahJoDB];
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetTbPostsPaged]
(
    @PageNumber      INT = 1,
    @RowsPerPage     INT = 10,
    @IncludeDeleted  BIT = 0,      -- 0 = IsDeleted = 0 only, 1 = include all
    @CategoryID      INT = NULL    -- NULL = all categories, else filter
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Safety guards
    IF (@PageNumber < 1) SET @PageNumber = 1;
    IF (@RowsPerPage < 1) SET @RowsPerPage = 10;
    IF (@RowsPerPage > 200) SET @RowsPerPage = 200;

    DECLARE @Offset INT = (@PageNumber - 1) * @RowsPerPage;

    -- Use CTE for filtering and counting
    ;WITH Filtered AS
    (
        SELECT *
        FROM dbo.VW_PostsForListing
        WHERE
            (@IncludeDeleted = 1 OR IsDeleted = 0)
            AND (@CategoryID IS NULL OR CategoryID = @CategoryID)
    )
    SELECT
        f.PostID,
        f.UserID,
        f.Username,
        f.Email,
        f.FirstName,
        f.LastName,
        f.RoleID,
        f.RoleName,

        f.CategoryID,
        f.CategoryName,

        f.PostTitle,
        f.PostDescription,
        f.Price,
        f.Status,
        f.CreatedAt,
        f.IsDeleted,

        -- Include total count for pagination UI
        TotalRows = COUNT(*) OVER()
    FROM Filtered f
    ORDER BY f.CreatedAt DESC, f.PostID DESC
    OFFSET @Offset ROWS FETCH NEXT @RowsPerPage ROWS ONLY;
END;
GO
