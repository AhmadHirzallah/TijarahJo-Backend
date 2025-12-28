# TijarahJo - Complete Posts API Frontend Integration Guide

## ?? Overview

This comprehensive guide documents all API endpoints for managing marketplace posts in TijarahJo. It covers how to create, read, update, and delete posts along with their images and reviews.

**Base URL:** `https://localhost:7064/api` (Development)  
**Production URL:** `https://your-domain.com/api`

---

## ?? IMPORTANT: Database Fix Required

Before the delete functionality works, run this SQL script in SSMS:

```sql
-- Run this in SQL Server Management Studio
-- File: TijarahJoDBAPI\Scripts\FixCascadeDelete.sql

USE TijarahJoDB;
GO

-- Fix SP_DeletePost (was using wrong table name TbReviews instead of TbPostReviews)
DROP PROCEDURE IF EXISTS SP_DeletePost;
GO

CREATE PROCEDURE [dbo].[SP_DeletePost]
    @PostID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        UPDATE TbPostImages SET IsDeleted = 1 WHERE PostID = @PostID AND IsDeleted = 0;
        UPDATE TbPostReviews SET IsDeleted = 1 WHERE PostID = @PostID AND IsDeleted = 0;
        UPDATE TbPosts SET IsDeleted = 1 WHERE PostID = @PostID AND IsDeleted = 0;
        SELECT @@ROWCOUNT AS RowsAffected;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS RowsAffected;
    END CATCH
END
GO

-- Fix SP_DeleteReview
DROP PROCEDURE IF EXISTS SP_DeleteReview;
GO

CREATE PROCEDURE [dbo].[SP_DeleteReview]
    @ReviewID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE TbPostReviews SET IsDeleted = 1 WHERE ReviewID = @ReviewID;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'Stored procedures fixed!'
```

---

## ?? Authentication

Most endpoints require authentication. Include the JWT token in the request header:

```
Authorization: Bearer <your_jwt_token>
```

### Authentication Status by Endpoint

| Endpoint | Auth Required | Notes |
|----------|---------------|-------|
| `GET /posts` | ? No | Public listing |
| `GET /posts/paginated` | ? No | Public with filters |
| `GET /posts/my` | ? Yes | User's own posts |
| `GET /posts/user/{id}` | ? No | Public user posts |
| `GET /posts/{id}` | ? No | Single post |
| `GET /posts/{id}/details` | ? No | Full post details |
| `POST /posts` | ? Yes | Create post |
| `PUT /posts/{id}` | ? Yes | Update (owner/admin) |
| `DELETE /posts/{id}` | ? Yes | Delete (owner/admin) |

---

## ?? API Endpoints

### 1. Get Current User's Posts

**Endpoint:** `GET /api/posts/my`

**Authentication:** Required ?

**Description:** Returns paginated posts for the currently authenticated user with full image data.

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
      "postDescription": "Barely used iPhone 15 Pro Max, 256GB, Space Black.",
      "price": 1200.00,
      "status": 2,
      "createdAt": "2024-01-20T12:00:00Z",
      "isDeleted": false,
      "primaryImageUrl": "/uploads/posts/1/image_abc123.jpg",
      "images": [
        {
          "postImageID": 1,
          "postID": 1,
          "postImageURL": "/uploads/posts/1/image_abc123.jpg",
          "uploadedAt": "2024-01-20T12:00:00Z"
        }
      ]
    }
  ],
  "pageNumber": 1,
  "rowsPerPage": 20,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

### 2. Get Posts by User ID (Public)

**Endpoint:** `GET /api/posts/user/{userId}`

**Authentication:** Optional

#### Example Request

```http
GET /api/posts/user/5?pageNumber=1&rowsPerPage=10
```

---

### 3. Get Paginated Posts (Public Listing)

**Endpoint:** `GET /api/posts/paginated`

**Authentication:** Not required

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Page number |
| `rowsPerPage` | int | 10 | Items per page |
| `categoryID` | int? | null | Filter by category |
| `includeDeleted` | bool | false | Include deleted posts |

#### Example Request

```http
GET /api/posts/paginated?pageNumber=1&rowsPerPage=12&categoryID=3
```

---

### 4. Create a Post

**Endpoint:** `POST /api/posts`

**Authentication:** Required ?

#### Request Body

```json
{
  "postTitle": "iPhone 15 Pro Max",
  "postDescription": "Brand new, never used.",
  "price": 1200.00,
  "categoryID": 3,
  "userID": 5,
  "status": 0
}
```

#### Field Validation

| Field | Required | Type | Validation |
|-------|----------|------|------------|
| `postTitle` | ? Yes | string | 3-200 characters |
| `postDescription` | ? No | string | Max 5000 characters |
| `price` | ? No | decimal | >= 0 |
| `categoryID` | ? Yes | int | Must exist in database |
| `userID` | ? Yes | int | Auto-filled from JWT |
| `status` | ? No | int | 0-5, defaults to 0 |

#### Response (201 Created)

```json
{
  "postID": 42,
  "userID": 5,
  "categoryID": 3,
  "postTitle": "iPhone 15 Pro Max",
  "postDescription": "Brand new, never used.",
  "price": 1200.00,
  "status": 0,
  "createdAt": "2024-01-20T12:00:00Z",
  "isDeleted": false
}
```

---

### 5. Update a Post

**Endpoint:** `PUT /api/posts/{id}`

**Authentication:** Required ? (Owner or Admin)

#### Request Body

```json
{
  "userID": 5,
  "categoryID": 3,
  "postTitle": "iPhone 15 Pro Max - REDUCED PRICE",
  "postDescription": "Brand new, never used. Price reduced!",
  "price": 1100.00,
  "status": 2,
  "isDeleted": false
}
```

---

### 6. Delete a Post ??

**Endpoint:** `DELETE /api/posts/{id}`

**Authentication:** Required ? (Owner or Admin)

**Description:** Soft-deletes a post along with all its images and reviews.

#### What Happens on Delete

1. ? All images for this post are soft-deleted (`IsDeleted = 1`)
2. ? All reviews for this post are soft-deleted (`IsDeleted = 1`)
3. ? The post itself is soft-deleted (`IsDeleted = 1`)

#### Example Request

```http
DELETE /api/posts/42
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response (204 No Content)

Success - No body returned.

#### Error Responses

| Status | Title | When |
|--------|-------|------|
| 400 | Invalid ID | ID < 1 |
| 401 | Unauthorized | No/invalid token |
| 403 | Forbidden | Not owner or admin |
| 404 | Post Not Found | Post doesn't exist |
| 500 | Delete Failed | Database error |

---

## ?? Post Status Values

| Code | Name | Description | Badge Color |
|------|------|-------------|-------------|
| 0 | Active | Live and visible | Green |
| 1 | Blocked | Blocked by admin | Red |
| 2 | Inactive | Hidden by user | Gray |
| 3 | SoldOut | Item has been sold | Blue |

---

## ??? Image Endpoints

### Upload Image (File)

**Endpoint:** `POST /api/posts/{postId}/images/upload`

**Content-Type:** `multipart/form-data`

```javascript
const formData = new FormData();
formData.append('file', imageFile);

await api.post(`/posts/${postId}/images/upload`, formData, {
  headers: { 'Content-Type': 'multipart/form-data' }
});
```

### Upload Image (Base64)

**Endpoint:** `POST /api/posts/{postId}/images/upload-base64`

```json
{
  "imageData": "data:image/jpeg;base64,/9j/4AAQSkZJRg..."
}
```

### Get Post Images

**Endpoint:** `GET /api/posts/{postId}/images`

### Delete Image

**Endpoint:** `DELETE /api/posts/{postId}/images/{imageId}`

---

## ?? TypeScript Interfaces

```typescript
// types/api.ts

export enum PostStatus {
  Active = 0,
  Blocked = 1,
  Inactive = 2,
  SoldOut = 3
}

export interface PostImage {
  postImageID: number;
  postID: number;
  postImageURL: string;
  uploadedAt: string;
}

export interface UserPost {
  postID: number;
  userID: number;
  categoryID: number;
  categoryName: string | null;
  postTitle: string;
  postDescription: string | null;
  price: number;
  status: PostStatus;
  createdAt: string;
  isDeleted: boolean;
  primaryImageUrl: string | null;
  images: PostImage[];
}

export interface UserPostsResponse {
  items: UserPost[];
  pageNumber: number;
  rowsPerPage: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreatePostRequest {
  postTitle: string;
  postDescription?: string;
  price?: number;
  categoryID: number;
  userID: number;
  status?: number;
}

export interface UpdatePostRequest {
  userID: number;
  categoryID: number;
  postTitle: string;
  postDescription?: string;
  price?: number;
  status: number;
  isDeleted: boolean;
}
```

---

## ?? Service Layer Implementation

```typescript
// services/postService.ts

import api, { BACKEND_URL } from './api';
import { 
  UserPostsResponse, 
  CreatePostRequest, 
  UpdatePostRequest 
} from '../types/api';

interface GetPostsParams {
  pageNumber?: number;
  rowsPerPage?: number;
  includeDeleted?: boolean;
  categoryID?: number;
}

export const postService = {
  // Get current user's posts
  getMyPosts: async (params?: GetPostsParams): Promise<UserPostsResponse> => {
    const { data } = await api.get('/posts/my', { params });
    return data;
  },

  // Get posts by user ID
  getPostsByUser: async (userId: number, params?: GetPostsParams): Promise<UserPostsResponse> => {
    const { data } = await api.get(`/posts/user/${userId}`, { params });
    return data;
  },

  // Get paginated posts (public)
  getPaginatedPosts: async (params?: GetPostsParams): Promise<any> => {
    const { data } = await api.get('/posts/paginated', { params });
    return data;
  },

  // Get single post
  getPost: async (postId: number): Promise<any> => {
    const { data } = await api.get(`/posts/${postId}`);
    return data;
  },

  // Get post with full details
  getPostDetails: async (postId: number): Promise<any> => {
    const { data } = await api.get(`/posts/${postId}/details`);
    return data;
  },

  // Create a new post
  createPost: async (payload: CreatePostRequest): Promise<any> => {
    const { data } = await api.post('/posts', payload);
    return data;
  },

  // Update a post
  updatePost: async (postId: number, payload: UpdatePostRequest): Promise<any> => {
    const { data } = await api.put(`/posts/${postId}`, payload);
    return data;
  },

  // Delete a post
  deletePost: async (postId: number): Promise<void> => {
    await api.delete(`/posts/${postId}`);
  },

  // Upload image (file)
  uploadImage: async (postId: number, file: File): Promise<any> => {
    const formData = new FormData();
    formData.append('file', file);
    const { data } = await api.post(`/posts/${postId}/images/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return data;
  },

  // Delete image
  deleteImage: async (postId: number, imageId: number): Promise<void> => {
    await api.delete(`/posts/${postId}/images/${imageId}`);
  }
};

// Helper: Convert relative URL to absolute
export const getAbsoluteImageUrl = (url: string | null): string | null => {
  if (!url) return null;
  if (url.startsWith('http')) return url;
  const path = url.startsWith('/') ? url : `/${url}`;
  return `${BACKEND_URL}${path}`;
};
```

---

## ?? React Component: My Posts Page

```tsx
// pages/MyPosts.tsx

import React, { useEffect, useState } from 'react';
import { postService, getAbsoluteImageUrl } from '../services/postService';
import { UserPost, PostStatus } from '../types/api';
import { Trash2, Edit3, ExternalLink, Package, Plus } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';

const MyPosts: React.FC = () => {
  const navigate = useNavigate();
  const [posts, setPosts] = useState<UserPost[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<number | null>(null);

  // Pagination
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
      
      if (err.response?.status === 401) {
        toast.error('Please login to view your posts');
        navigate('/login');
        return;
      }
      
      setError(err.response?.data?.detail || 'Failed to load your listings');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (postId: number, postTitle: string) => {
    if (!window.confirm(`Are you sure you want to delete "${postTitle}"?\n\nThis will also delete all images and reviews.`)) {
      return;
    }

    setDeleting(postId);
    
    try {
      await postService.deletePost(postId);
      
      // Remove from local state
      setPosts(prev => prev.filter(p => p.postID !== postId));
      setTotalCount(prev => prev - 1);
      
      toast.success('Listing deleted successfully');
    } catch (err: any) {
      console.error('Delete failed:', err);
      
      const errorMessage = err.response?.data?.detail || 'Failed to delete listing';
      toast.error(errorMessage);
    } finally {
      setDeleting(null);
    }
  };

  const getStatusBadge = (status: PostStatus) => {
    const config: Record<PostStatus, { bg: string; text: string; label: string }> = {
      [PostStatus.Active]: { bg: 'bg-green-100', text: 'text-green-700', label: 'Active' },
      [PostStatus.Blocked]: { bg: 'bg-red-100', text: 'text-red-700', label: 'Blocked' },
      [PostStatus.Inactive]: { bg: 'bg-gray-100', text: 'text-gray-600', label: 'Inactive' },
      [PostStatus.SoldOut]: { bg: 'bg-blue-100', text: 'text-blue-700', label: 'Sold' },
    };
    const c = config[status] || config[PostStatus.Active];
    return (
      <span className={`px-3 py-1 rounded-full text-xs font-bold ${c.bg} ${c.text}`}>
        {c.label}
      </span>
    );
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-12 text-center">
        <p className="text-red-500 mb-4">{error}</p>
        <button 
          onClick={fetchMyPosts}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">My Listings</h1>
          <p className="text-gray-500 mt-1">
            {totalCount} {totalCount === 1 ? 'listing' : 'listings'}
          </p>
        </div>
        <Link 
          to="/create-post" 
          className="flex items-center gap-2 bg-blue-600 text-white px-5 py-2.5 rounded-xl font-semibold hover:bg-blue-700 transition"
        >
          <Plus size={20} />
          New Listing
        </Link>
      </div>

      {/* Posts Table */}
      {posts.length === 0 ? (
        <div className="bg-white rounded-2xl shadow-sm border p-12 text-center">
          <Package size={48} className="mx-auto mb-4 text-gray-300" />
          <h3 className="text-lg font-semibold text-gray-700">No listings yet</h3>
          <p className="text-gray-500 mt-2">Create your first listing to start selling!</p>
          <Link 
            to="/create-post"
            className="inline-block mt-4 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Create Listing
          </Link>
        </div>
      ) : (
        <div className="bg-white rounded-2xl shadow-sm border overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-semibold text-gray-500 uppercase">
                  Item
                </th>
                <th className="px-6 py-4 text-left text-xs font-semibold text-gray-500 uppercase">
                  Status
                </th>
                <th className="px-6 py-4 text-left text-xs font-semibold text-gray-500 uppercase">
                  Price
                </th>
                <th className="px-6 py-4 text-right text-xs font-semibold text-gray-500 uppercase">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {posts.map(post => {
                const imageUrl = getAbsoluteImageUrl(
                  post.primaryImageUrl || post.images[0]?.postImageURL
                );

                return (
                  <tr key={post.postID} className="hover:bg-gray-50 transition">
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0">
                          {imageUrl ? (
                            <img 
                              src={imageUrl} 
                              alt={post.postTitle}
                              className="w-full h-full object-cover"
                              onError={(e) => {
                                (e.target as HTMLImageElement).style.display = 'none';
                              }}
                            />
                          ) : (
                            <div className="w-full h-full flex items-center justify-center">
                              <Package size={20} className="text-gray-400" />
                            </div>
                          )}
                        </div>
                        <div>
                          <p className="font-semibold text-gray-900">{post.postTitle}</p>
                          <p className="text-sm text-gray-500">{post.categoryName || 'Uncategorized'}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      {getStatusBadge(post.status)}
                    </td>
                    <td className="px-6 py-4">
                      <span className="font-semibold text-blue-600">
                        {post.price?.toLocaleString() || 0} JD
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-end gap-2">
                        <Link 
                          to={`/post/${post.postID}`}
                          className="p-2 text-gray-400 hover:text-blue-600 transition"
                          title="View"
                        >
                          <ExternalLink size={18} />
                        </Link>
                        <Link 
                          to={`/edit-post/${post.postID}`}
                          className="p-2 text-gray-400 hover:text-orange-500 transition"
                          title="Edit"
                        >
                          <Edit3 size={18} />
                        </Link>
                        <button 
                          onClick={() => handleDelete(post.postID, post.postTitle)}
                          disabled={deleting === post.postID}
                          className="p-2 text-gray-400 hover:text-red-500 transition disabled:opacity-50"
                          title="Delete"
                        >
                          {deleting === post.postID ? (
                            <div className="w-[18px] h-[18px] border-2 border-red-500 border-t-transparent rounded-full animate-spin" />
                          ) : (
                            <Trash2 size={18} />
                          )}
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="px-6 py-4 border-t flex items-center justify-between">
              <p className="text-sm text-gray-500">
                Page {currentPage} of {totalPages}
              </p>
              <div className="flex gap-2">
                <button
                  onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                  className="px-4 py-2 border rounded-lg text-sm disabled:opacity-50 hover:bg-gray-50"
                >
                  Previous
                </button>
                <button
                  onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                  className="px-4 py-2 border rounded-lg text-sm disabled:opacity-50 hover:bg-gray-50"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default MyPosts;
```

---

## ? Error Handling

### Error Response Format

All errors follow this format:

```json
{
  "title": "Error Title",
  "detail": "Human-readable error message",
  "status": 400
}
```

### Common Errors and Handling

```typescript
const handleApiError = (error: any) => {
  const status = error.response?.status;
  const detail = error.response?.data?.detail;

  switch (status) {
    case 400:
      toast.error(detail || 'Invalid request');
      break;
    case 401:
      toast.error('Please login to continue');
      navigate('/login');
      break;
    case 403:
      toast.error("You don't have permission to do this");
      break;
    case 404:
      toast.error('Item not found');
      break;
    case 500:
      toast.error(detail || 'Server error. Please try again.');
      break;
    default:
      toast.error('Something went wrong');
  }
};
```

---

## ?? Testing Checklist

### My Posts Page
- [ ] Posts load correctly for logged-in user
- [ ] Empty state shows when no posts
- [ ] Images display with fallback
- [ ] Status badges show correct colors
- [ ] Pagination works
- [ ] Unauthorized redirects to login

### Delete Post
- [ ] Confirmation dialog appears
- [ ] Loading state during delete
- [ ] Success removes post from list
- [ ] Error shows toast message
- [ ] Post with images deletes correctly
- [ ] Post with reviews deletes correctly

### Create Post
- [ ] Form validation works
- [ ] Category dropdown populated
- [ ] Image upload works
- [ ] Success redirects to my posts
- [ ] Error shows validation messages

---

## ?? Quick Reference

### Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/posts/my` | Current user's posts |
| GET | `/posts/user/{id}` | User's public posts |
| GET | `/posts/paginated` | All posts (public) |
| GET | `/posts/{id}` | Single post |
| GET | `/posts/{id}/details` | Post with full details |
| POST | `/posts` | Create post |
| PUT | `/posts/{id}` | Update post |
| DELETE | `/posts/{id}` | Delete post (cascade) |
| POST | `/posts/{id}/images/upload` | Upload image |
| DELETE | `/posts/{id}/images/{imageId}` | Delete image |

### Response Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 204 | Deleted (no content) |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 500 | Server Error |

---

## ?? Troubleshooting

### "Failed to delete post reviews" (500 Error)

**Cause:** The stored procedure `SP_DeletePost` in the database references `TbReviews` instead of `TbPostReviews`.

**Solution:** Run the SQL fix script at the top of this document in SSMS.

### Images Not Loading

**Cause:** Relative URLs need backend prefix.

**Solution:**
```typescript
const imageUrl = getAbsoluteImageUrl(post.primaryImageUrl);
// Converts "/uploads/..." to "https://localhost:7064/uploads/..."
```

### 401 on API Calls

**Cause:** Token expired or missing.

**Solution:**
- Check localStorage for `auth_token`
- Redirect to login
- Re-authenticate

---

## ?? Database Table Names

| Entity | Correct Table Name |
|--------|-------------------|
| Posts | `TbPosts` |
| Post Images | `TbPostImages` |
| Post Reviews | `TbPostReviews` ?? (NOT TbReviews!) |
| Categories | `TbItemCategories` |
| Users | `TbUsers` |
| Roles | `TbRoles` |

---

*Last Updated: December 2024*  
*API Version: 1.0*
