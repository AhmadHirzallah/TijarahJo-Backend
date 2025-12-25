namespace TijarahJoDBAPI.DTOs.Responses;

#region Dashboard

/// <summary>
/// Admin dashboard statistics
/// </summary>
public class AdminDashboardResponse
{
    public DateTime GeneratedAt { get; set; }
    public int GeneratedByAdminId { get; set; }

    // User Statistics
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int DeletedUsers { get; set; }

    // Post Statistics
    public int TotalPosts { get; set; }
    public int ActivePosts { get; set; }
    public int DeletedPosts { get; set; }
    public int DraftPosts { get; set; }
    public int PendingReviewPosts { get; set; }
    public int PublishedPosts { get; set; }

    // Other Statistics
    public int TotalCategories { get; set; }
    public int TotalRoles { get; set; }
}

#endregion

#region User Management

/// <summary>
/// Admin view of a user
/// </summary>
public class AdminUserResponse
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public DateTime JoinDate { get; set; }
    public int Status { get; set; }
    public string StatusName => Status switch
    {
        0 => "Active",
        1 => "Inactive",
        2 => "Banned",
        3 => "Suspended",
        _ => "Unknown"
    };
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public string FullName => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";
}

/// <summary>
/// Detailed admin view of a user with activity stats
/// </summary>
public class AdminUserDetailResponse : AdminUserResponse
{
    public int TotalPosts { get; set; }
    public int TotalImages { get; set; }
    public int TotalReviews { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// List of users with summary counts
/// </summary>
public class AdminUsersListResponse
{
    public List<AdminUserResponse> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int DeletedCount { get; set; }
}

#endregion

#region Post Management

/// <summary>
/// Admin view of a post
/// </summary>
public class AdminPostResponse
{
    public int PostID { get; set; }
    public int UserID { get; set; }
    public int CategoryID { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public string? PostDescription { get; set; }
    public decimal? Price { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// List of posts with summary
/// </summary>
public class AdminPostsListResponse
{
    public List<AdminPostResponse> Posts { get; set; } = new();
    public int TotalCount { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
}

#endregion
