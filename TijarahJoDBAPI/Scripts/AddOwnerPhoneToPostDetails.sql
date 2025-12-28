-- ============================================================
-- TIJARAHJO: ADD OWNER PHONE TO POST DETAILS
-- ============================================================
-- This script updates SP_GetPostDetails_All to include
-- the owner's primary phone number for WhatsApp contact
-- ============================================================

USE TijarahJoDB;
GO

-- Update SP_GetPostDetails_All to include owner's phone
CREATE OR ALTER PROCEDURE [dbo].[SP_GetPostDetails_All]
(
    @PostID INT,
    @IncludeDeleted BIT = 0   -- 0 = only active posts, 1 = include deleted posts
)
AS
BEGIN
    SET NOCOUNT ON;

    /* ---------------------------
       Result Set 1: Post + Owner + Category + Role + Owner Phone
       --------------------------- */
    SELECT
        p.PostID,
        p.UserID AS OwnerUserID,
        u.Username AS OwnerUsername,
        u.Email AS OwnerEmail,
        u.FirstName AS OwnerFirstName,
        u.LastName AS OwnerLastName,
        CONCAT(u.FirstName, N' ', ISNULL(u.LastName, N'')) AS OwnerFullName,
        
        -- Owner's primary phone for WhatsApp contact
        (SELECT TOP 1 PhoneNumber 
         FROM TbUserPhoneNumbers 
         WHERE UserID = u.UserID AND IsPrimary = 1 AND IsDeleted = 0) AS OwnerPrimaryPhone,
        
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
    INNER JOIN dbo.TbUsers u ON u.UserID = p.UserID
    INNER JOIN dbo.TbRoles ro ON ro.RoleID = u.RoleID
    INNER JOIN dbo.TbItemCategories c ON c.CategoryID = p.CategoryID
    WHERE p.PostID = @PostID
      AND (@IncludeDeleted = 1 OR p.IsDeleted = 0);

    -- If post doesn't exist or doesn't match filter, return empty sets
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.TbPosts p
        WHERE p.PostID = @PostID
          AND (@IncludeDeleted = 1 OR p.IsDeleted = 0)
    )
    BEGIN
        RETURN;
    END

    /* ---------------------------
       Result Set 2: Reviews
       --------------------------- */
    SELECT 
        r.ReviewID,
        r.PostID,
        r.Rating,
        r.ReviewText,
        r.CreatedAt,
        u.UserID AS ReviewerUserID,
        u.Username AS ReviewerUsername,
        u.Email AS ReviewerEmail,
        u.FirstName AS ReviewerFirstName,
        u.LastName AS ReviewerLastName,
        CONCAT(u.FirstName, N' ', ISNULL(u.LastName, N'')) AS ReviewerFullName
    FROM dbo.TbPostReviews r
    INNER JOIN dbo.TbUsers u ON r.UserID = u.UserID
    WHERE r.PostID = @PostID
      AND r.IsDeleted = 0
    ORDER BY r.CreatedAt DESC, r.ReviewID DESC;

    /* ---------------------------
       Result Set 3: Images
       --------------------------- */
    SELECT
        i.PostImageID,
        i.PostID,
        i.PostImageURL,
        i.UploadedAt
    FROM dbo.TbPostImages i
    WHERE i.PostID = @PostID
      AND i.IsDeleted = 0
    ORDER BY i.UploadedAt ASC, i.PostImageID ASC;
END;
GO

PRINT 'SP_GetPostDetails_All updated with OwnerPrimaryPhone field';
GO

-- Verify the change
PRINT '';
PRINT 'Testing SP_GetPostDetails_All...';
EXEC SP_GetPostDetails_All @PostID = 1;
GO
