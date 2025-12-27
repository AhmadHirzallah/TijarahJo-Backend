# TijarahJo - My Posts/Listings Frontend Integration Guide

## ?? Overview

This guide documents the API endpoints for managing user posts/listings in the TijarahJo marketplace. It covers how to fetch, display, and manage the current user's posts from the React frontend.

**Base URL:** `https://localhost:7064/api` (Development)

---

## ?? Authentication Required

All "My Posts" endpoints require authentication. Include the JWT token in the request header:

```
Authorization: Bearer <your_jwt_token>
```

---

## ?? API Endpoints

### 1. Get Current User's Posts (Primary Endpoint)

**Endpoint:** `GET /api/posts/my`

**Authentication:** Required ?

**Description:** Returns all posts created by the currently authenticated user with full image data.

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Page number (1-based) |
| `rowsPerPage` | int | 50 | Items per page (max: 100) |
| `includeDeleted` | bool | false | Include soft-deleted posts |

#### Example Request

```http
GET /api/posts/my?pageNumber=1&rowsPerPage=20
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response (200 OK)

```json
{
  "items": [
    {
      "postID": 1,
      "userID": 5,
      "categoryID": 3,
      "categoryName": "Electronics",
      "postTitle": "iPhone 15 Pro Max - Like New",
      "postDescription": "Barely used iPhone 15 Pro Max, 256GB, Space Black. Comes with original box and accessories.",
      "price": 1200.00,
      "status": 2,
      "statusName": "Active",
      "createdAt": "2024-01-20T12:00:00Z",
      "isDeleted": false,
      "primaryImageUrl": "/uploads/posts/1/image_abc123.jpg",
      "images": [
        {
          "postImageID": 1,
          "postID": 1,
          "postImageURL": "/uploads/posts/1/image_abc123.jpg",
          "uploadedAt": "2024-01-20T12:00:00Z"
        },
        {
          "postImageID": 2,
          "postID": 1,
          "postImageURL": "/uploads/posts/1/image_def456.jpg",
          "uploadedAt": "2024-01-20T12:05:00Z"
        }
      ],
      "imageCount": 2
    },
    {
      "postID": 2,
      "userID": 5,
      "categoryID": 7,
      "categoryName": "Furniture",
      "postTitle": "Modern Office Desk",
      "postDescription": "Ergonomic standing desk, adjustable height.",
      "price": 350.00,
      "status": 0,
      "statusName": "Draft",
      "createdAt": "2024-01-18T09:30:00Z",
      "isDeleted": false,
      "primaryImageUrl": null,
      "images": [],
      "imageCount": 0
    }
  ],
  "pageNumber": 1,
  "rowsPerPage": 20,
  "totalCount": 2,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

### 2. Get Posts by User ID (Public)

**Endpoint:** `GET /api/posts/user/{userId}`

**Authentication:** Optional (Required to see deleted posts)

**Description:** Returns posts for any user. Useful for viewing another user's listings.

#### Example Request

```http
GET /api/posts/user/5?pageNumber=1&rowsPerPage=10
```

---

### 3. Delete a Post

**Endpoint:** `DELETE /api/posts/{postId}`

**Authentication:** Required ? (Must be owner or admin)

#### Example Request

```http
DELETE /api/posts/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response (204 No Content)

Success - No body returned.

#### Error Responses

| Status | Description |
|--------|-------------|
| 401 | Unauthorized - Token missing or invalid |
| 403 | Forbidden - Not the post owner |
| 404 | Post not found |

---

## ?? Post Status Values

| Status Code | Status Name | Description |
|-------------|-------------|-------------|
| 0 | Draft | Post saved but not published |
| 1 | Pending | Awaiting review/approval |
| 2 | Active | Live and visible to buyers |
| 3 | Sold | Item has been sold |
| 4 | Expired | Listing has expired |
| 5 | Rejected | Post was rejected by admin |

---

## ?? TypeScript Interfaces

### API Types (`types/api.ts`)

```typescript
// Post Status Enum
export enum PostStatus {
  Draft = 0,
  Pending = 1,
  Active = 2,
  Sold = 3,
  Expired = 4,
  Rejected = 5
}

// Post Image
export interface PostImage {
  postImageID: number;
  postID: number;
  postImageURL: string;
  uploadedAt: string;
}

// User Post Response (for My Posts page)
export interface UserPost {
  postID: number;
  userID: number;
  categoryID: number;
  categoryName: string | null;
  postTitle: string;
  postDescription: string | null;
  price: number;
  status: PostStatus;
  statusName: string;
  createdAt: string;
  isDeleted: boolean;
  primaryImageUrl: string | null;
  images: PostImage[];
  imageCount: number;
}

// Paginated Response for User Posts
export interface UserPostsResponse {
  items: UserPost[];
  pageNumber: number;
  rowsPerPage: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
```

---

## ?? Service Layer Implementation

### Post Service (`services/postService.ts`)

```typescript
import api from './api';
import { UserPostsResponse, UserPost } from '../types/api';

interface GetMyPostsParams {
  pageNumber?: number;
  rowsPerPage?: number;
  includeDeleted?: boolean;
}

export const postService = {
  /**
   * Get current authenticated user's posts
   */
  getMyPosts: async (params?: GetMyPostsParams): Promise<UserPostsResponse> => {
    const response = await api.get('/posts/my', { params });
    return response.data;
  },

  /**
   * Get posts by a specific user ID
   */
  getPostsByUser: async (
    userId: number, 
    params?: GetMyPostsParams
  ): Promise<UserPostsResponse> => {
    const response = await api.get(`/posts/user/${userId}`, { params });
    return response.data;
  },

  /**
   * Delete a post by ID
   */
  deletePost: async (postId: number): Promise<void> => {
    await api.delete(`/posts/${postId}`);
  },

  /**
   * Get single post details
   */
  getPostDetails: async (postId: number): Promise<any> => {
    const response = await api.get(`/posts/${postId}/details`);
    return response.data;
  }
};
```

---

## ?? React Component Implementation

### MyPosts Page (`pages/MyPosts.tsx`)

```tsx
import React, { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { postService } from '../../services/postService';
import { UserPost, PostStatus } from '../../types/api';
import Loader from '../../components/UI/Loader';
import { Trash2, Edit3, ExternalLink, Package } from 'lucide-react';
import { Link } from 'react-router';
import { BACKEND_URL } from '../../services/api';

const MyPosts: React.FC = () => {
  const { user } = useAuth();
  const [posts, setPosts] = useState<UserPost[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    fetchMyPosts();
  }, [currentPage]);

  const fetchMyPosts = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await postService.getMyPosts({
        pageNumber: currentPage,
        rowsPerPage: 20
      });
      
      setPosts(response.items);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
    } catch (err: any) {
      console.error('Failed to fetch posts:', err);
      setError(err.response?.data?.detail || 'Failed to load your listings');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Convert relative image URL to absolute URL
   */
  const getAbsoluteImageUrl = (url: string | null | undefined): string | null => {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    const normalizedPath = url.startsWith('/') ? url : `/${url}`;
    return `${BACKEND_URL}${normalizedPath}`;
  };

  /**
   * Handle post deletion
   */
  const handleDelete = async (id: number) => {
    if (!window.confirm("Are you sure you want to delete this listing?")) {
      return;
    }

    try {
      await postService.deletePost(id);
      // Remove from local state
      setPosts(posts.filter(p => p.postID !== id));
      setTotalCount(prev => prev - 1);
    } catch (err: any) {
      alert(err.response?.data?.detail || "Failed to delete listing");
    }
  };

  /**
   * Get status badge styling
   */
  const getStatusBadge = (status: PostStatus) => {
    const styles: Record<PostStatus, string> = {
      [PostStatus.Draft]: 'bg-gray-100 text-gray-600',
      [PostStatus.Pending]: 'bg-yellow-100 text-yellow-700',
      [PostStatus.Active]: 'bg-green-100 text-green-700',
      [PostStatus.Sold]: 'bg-blue-100 text-blue-700',
      [PostStatus.Expired]: 'bg-red-100 text-red-600',
      [PostStatus.Rejected]: 'bg-red-100 text-red-700',
    };
    return styles[status] || 'bg-gray-100 text-gray-500';
  };

  if (loading) return <Loader fullScreen />;

  if (error) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-12 text-center">
        <p className="text-red-500">{error}</p>
        <button 
          onClick={fetchMyPosts}
          className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg"
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-12">
      {/* Header */}
      <div className="flex items-center justify-between mb-10">
        <div>
          <h1 className="text-3xl font-black text-gray-900">My Listings</h1>
          <p className="text-gray-500 font-medium">
            {totalCount} {totalCount === 1 ? 'listing' : 'listings'} total
          </p>
        </div>
        <Link 
          to="/create-post" 
          className="bg-blue-600 text-white px-6 py-3 rounded-2xl font-bold shadow-lg shadow-blue-100 hover:bg-blue-700 transition-all"
        >
          + New Listing
        </Link>
      </div>

      {/* Table */}
      <div className="bg-white rounded-[2.5rem] shadow-xl overflow-hidden border">
        <div className="overflow-x-auto">
          <table className="w-full text-left min-w-[800px]">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-8 py-5 text-[10px] font-bold text-gray-400 uppercase tracking-widest">
                  Item
                </th>
                <th className="px-8 py-5 text-[10px] font-bold text-gray-400 uppercase tracking-widest">
                  Status
                </th>
                <th className="px-8 py-5 text-[10px] font-bold text-gray-400 uppercase tracking-widest">
                  Price
                </th>
                <th className="px-8 py-5 text-[10px] font-bold text-gray-400 uppercase tracking-widest text-right">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {posts.map(post => {
                // Get image URL - prefer primaryImageUrl, fallback to first image
                const imageUrl = getAbsoluteImageUrl(
                  post.primaryImageUrl || 
                  (post.images.length > 0 ? post.images[0].postImageURL : null)
                );

                return (
                  <tr key={post.postID} className="hover:bg-gray-50 transition-colors">
                    {/* Item Info */}
                    <td className="px-8 py-6">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-xl bg-slate-100 overflow-hidden shrink-0 flex items-center justify-center text-slate-300">
                          {imageUrl ? (
                            <img 
                              src={imageUrl} 
                              className="w-full h-full object-cover" 
                              alt={post.postTitle}
                              onError={(e) => {
                                (e.target as HTMLImageElement).style.display = 'none';
                              }}
                            />
                          ) : (
                            <Package size={20} className="opacity-40" />
                          )}
                        </div>
                        <div>
                          <p className="font-bold text-gray-900">{post.postTitle}</p>
                          <p className="text-xs text-gray-400">
                            {post.categoryName || 'Uncategorized'}
                          </p>
                        </div>
                      </div>
                    </td>

                    {/* Status */}
                    <td className="px-8 py-6">
                      <span className={`px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider ${getStatusBadge(post.status)}`}>
                        {post.statusName || PostStatus[post.status]}
                      </span>
                    </td>

                    {/* Price */}
                    <td className="px-8 py-6 font-bold text-blue-600">
                      {post.price.toLocaleString()} JD
                    </td>

                    {/* Actions */}
                    <td className="px-8 py-6 text-right space-x-2">
                      <Link 
                        to={`/post/${post.postID}`} 
                        className="p-2 text-gray-400 hover:text-blue-600 inline-block"
                        title="View"
                      >
                        <ExternalLink size={18} />
                      </Link>
                      <Link 
                        to={`/edit-post/${post.postID}`} 
                        className="p-2 text-gray-400 hover:text-orange-600 inline-block"
                        title="Edit"
                      >
                        <Edit3 size={18} />
                      </Link>
                      <button 
                        onClick={() => handleDelete(post.postID)} 
                        className="p-2 text-gray-400 hover:text-red-600"
                        title="Delete"
                      >
                        <Trash2 size={18} />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {/* Empty State */}
        {posts.length === 0 && (
          <div className="py-20 text-center text-gray-400">
            <Package size={48} className="mx-auto mb-4 opacity-20" />
            <p className="font-bold">No listings yet</p>
            <p className="text-sm mt-2">Create your first listing to start selling!</p>
            <Link 
              to="/create-post"
              className="inline-block mt-4 px-6 py-2 bg-blue-600 text-white rounded-lg"
            >
              Create Listing
            </Link>
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="px-8 py-4 border-t flex items-center justify-between">
            <p className="text-sm text-gray-500">
              Page {currentPage} of {totalPages}
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={currentPage === 1}
                className="px-4 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Previous
              </button>
              <button
                onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
                className="px-4 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default MyPosts;
```

---

## ?? Migration from Old Implementation

If you were previously using a different endpoint, here's how to migrate:

### Before (Old)
```typescript
// Old approach - using paginated with userID filter
postService.getPosts({ userID: user.userID }).then(res => {
  setPosts(res.items);
});
```

### After (New)
```typescript
// New approach - dedicated my posts endpoint
postService.getMyPosts().then(res => {
  setPosts(res.items);
});
```

### Key Differences

| Feature | Old Approach | New Approach |
|---------|--------------|--------------|
| Endpoint | `/posts/paginated?userID=X` | `/posts/my` |
| Images | Only `primaryImageUrl` | Full `images` array + `primaryImageUrl` |
| Status Name | Had to compute | Included as `statusName` |
| Image Count | Had to count | Included as `imageCount` |

---

## ??? Image Handling

### Converting Relative URLs to Absolute

```typescript
const BACKEND_URL = 'https://localhost:7064'; // or your production URL

const getAbsoluteImageUrl = (url: string | null): string | null => {
  if (!url) return null;
  
  // Already absolute URL
  if (url.startsWith('http://') || url.startsWith('https://')) {
    return url;
  }
  
  // Relative URL - prepend backend URL
  const normalizedPath = url.startsWith('/') ? url : `/${url}`;
  return `${BACKEND_URL}${normalizedPath}`;
};
```

### Getting the Best Image

```typescript
const getBestImageUrl = (post: UserPost): string | null => {
  // Priority: primaryImageUrl > first image in array > null
  return getAbsoluteImageUrl(
    post.primaryImageUrl || 
    (post.images.length > 0 ? post.images[0].postImageURL : null)
  );
};
```

---

## ? Error Handling

### Common Error Responses

| Status Code | Meaning | How to Handle |
|-------------|---------|---------------|
| 401 | Unauthorized | Redirect to login |
| 403 | Forbidden | Show "access denied" message |
| 404 | Not Found | Show "post not found" message |
| 500 | Server Error | Show generic error, allow retry |

### Example Error Handler

```typescript
const handleApiError = (error: any) => {
  if (error.response) {
    switch (error.response.status) {
      case 401:
        // Token expired or invalid
        logout();
        navigate('/login');
        break;
      case 403:
        toast.error("You don't have permission to do this");
        break;
      case 404:
        toast.error("The item was not found");
        break;
      default:
        toast.error(error.response.data?.detail || "Something went wrong");
    }
  } else {
    toast.error("Network error. Please check your connection.");
  }
};
```

---

## ?? Testing Checklist

- [ ] User can view their posts when logged in
- [ ] Empty state shows when no posts exist
- [ ] Post images display correctly
- [ ] Status badges show correct colors
- [ ] Delete button removes post from list
- [ ] Pagination works correctly
- [ ] Unauthorized users are redirected to login
- [ ] Error states display properly

---

## ?? Related Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/posts/my` | GET | Get current user's posts |
| `/posts/user/{id}` | GET | Get posts by user ID |
| `/posts/{id}` | GET | Get single post |
| `/posts/{id}/details` | GET | Get post with full details |
| `/posts` | POST | Create new post |
| `/posts/{id}` | PUT | Update post |
| `/posts/{id}` | DELETE | Delete post |
| `/posts/{id}/images/upload` | POST | Upload post image |

---

## ?? Support

For API issues or questions:
- Check Swagger documentation at `/swagger`
- Review the API response in browser DevTools
- Check the backend console for detailed error logs

---

*Last Updated: January 2025*
*API Version: 1.0*
