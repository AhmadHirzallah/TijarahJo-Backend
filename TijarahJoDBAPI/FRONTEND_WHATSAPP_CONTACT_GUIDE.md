# TijarahJo - Post Owner Contact (WhatsApp) Integration Guide

## ?? Overview

This guide documents how to get the post owner's phone number from a post and integrate WhatsApp contact functionality in your React frontend.

**Base URL:** `https://localhost:7064/api` (Development)  
**Production URL:** `https://your-domain.com/api`

---

## ?? Use Case

When viewing a post/listing, users want to contact the seller via WhatsApp. The API provides:
1. Owner's primary phone number
2. Pre-computed WhatsApp deep link
3. Helper flag to check if phone is available

---

## ?? API Endpoint

### Get Post Details (with Owner Phone)

**Endpoint:** `GET /api/posts/{id}/details`

**Authentication:** Not required (public endpoint)

**Description:** Returns complete post details including owner information with their primary phone number.

---

## ? Success Response (200 OK)

```json
{
  "postID": 42,
  "postTitle": "iPhone 15 Pro Max - Like New",
  "postDescription": "Barely used, 256GB, Space Black color.",
  "price": 1200.00,
  "status": 2,
  "createdAt": "2024-01-20T12:00:00Z",
  "isDeleted": false,
  
  "ownerUserID": 5,
  "ownerUsername": "ahmad_seller",
  "ownerEmail": "ahmad@example.com",
  "ownerFirstName": "Ahmad",
  "ownerLastName": "Ali",
  "ownerFullName": "Ahmad Ali",
  "ownerPrimaryPhone": "+962791234567",
  
  "roleID": 2,
  "roleName": "User",
  
  "categoryID": 3,
  "categoryName": "Electronics",
  
  "reviews": [...],
  "images": [...],
  
  "reviewCount": 5,
  "averageRating": 4.5,
  "imageCount": 3,
  "primaryImageUrl": "/uploads/posts/42/image1.jpg",
  
  "hasOwnerPhone": true,
  "whatsAppLink": "https://wa.me/962791234567"
}
```

### Key Fields for WhatsApp Contact

| Field | Type | Description |
|-------|------|-------------|
| `ownerPrimaryPhone` | string? | Owner's phone number (may be null) |
| `hasOwnerPhone` | boolean | `true` if phone is available |
| `whatsAppLink` | string? | Ready-to-use WhatsApp URL |
| `ownerFullName` | string | Owner's display name |

---

## ?? When Phone is Not Available

If the owner hasn't added a phone number:

```json
{
  "ownerPrimaryPhone": null,
  "hasOwnerPhone": false,
  "whatsAppLink": null
}
```

---

## ?? TypeScript Types

```typescript
// types/post.ts

export interface PostOwnerInfo {
  ownerUserID: number;
  ownerUsername: string | null;
  ownerEmail: string | null;
  ownerFirstName: string | null;
  ownerLastName: string | null;
  ownerFullName: string | null;
  ownerPrimaryPhone: string | null;
}

export interface PostDetailsResponse extends PostOwnerInfo {
  // Post Info
  postID: number;
  postTitle: string;
  postDescription: string | null;
  price: number | null;
  status: number;
  createdAt: string;
  isDeleted: boolean;
  
  // Category
  categoryID: number;
  categoryName: string | null;
  
  // Role
  roleID: number | null;
  roleName: string | null;
  
  // Related Data
  reviews: ReviewDetail[];
  images: PostImage[];
  
  // Computed Properties
  reviewCount: number;
  averageRating: number | null;
  imageCount: number;
  primaryImageUrl: string | null;
  
  // WhatsApp Contact
  hasOwnerPhone: boolean;
  whatsAppLink: string | null;
}

export interface PostImage {
  postImageID: number;
  postID: number;
  postImageURL: string;
  uploadedAt: string;
}

export interface ReviewDetail {
  reviewID: number;
  postID: number;
  rating: number;
  reviewText: string | null;
  createdAt: string;
  reviewerUserID: number;
  reviewerUsername: string | null;
  reviewerFullName: string | null;
}
```

---

## ?? Service Layer

```typescript
// services/postService.ts

import api, { BACKEND_URL } from './api';
import { PostDetailsResponse } from '../types/post';

export const postService = {
  /**
   * Get complete post details including owner phone for WhatsApp
   */
  getPostDetails: async (postId: number): Promise<PostDetailsResponse> => {
    const { data } = await api.get<PostDetailsResponse>(`/posts/${postId}/details`);
    return data;
  },
  
  /**
   * Generate WhatsApp link with pre-filled message
   */
  generateWhatsAppLink: (
    phoneNumber: string, 
    postTitle: string,
    message?: string
  ): string => {
    // Clean phone number (remove +, spaces, dashes)
    const cleanPhone = phoneNumber.replace(/[\s\-\+]/g, '');
    
    // Default message about the listing
    const defaultMessage = message || 
      `Hi! I'm interested in your listing: "${postTitle}" on TijarahJo.`;
    
    // URL encode the message
    const encodedMessage = encodeURIComponent(defaultMessage);
    
    return `https://wa.me/${cleanPhone}?text=${encodedMessage}`;
  }
};

export default postService;
```

---

## ?? React Components

### 1. WhatsApp Contact Button Component

```tsx
// components/WhatsAppButton.tsx

import React from 'react';
import { MessageCircle, Phone } from 'lucide-react';

interface WhatsAppButtonProps {
  phoneNumber: string | null;
  whatsAppLink: string | null;
  ownerName: string | null;
  postTitle: string;
  variant?: 'button' | 'icon' | 'full';
  className?: string;
}

const WhatsAppButton: React.FC<WhatsAppButtonProps> = ({
  phoneNumber,
  whatsAppLink,
  ownerName,
  postTitle,
  variant = 'button',
  className = ''
}) => {
  // If no phone number, show disabled state or alternative
  if (!phoneNumber || !whatsAppLink) {
    return (
      <div className={`flex items-center gap-2 px-4 py-2 bg-gray-100 text-gray-400 rounded-lg cursor-not-allowed ${className}`}>
        <Phone size={20} />
        <span>Phone not available</span>
      </div>
    );
  }

  // Generate WhatsApp link with custom message
  const message = `Hi ${ownerName || 'there'}! I'm interested in your listing: "${postTitle}" on TijarahJo.`;
  const cleanPhone = phoneNumber.replace(/[\s\-\+]/g, '');
  const fullWhatsAppLink = `https://wa.me/${cleanPhone}?text=${encodeURIComponent(message)}`;

  if (variant === 'icon') {
    return (
      <a
        href={fullWhatsAppLink}
        target="_blank"
        rel="noopener noreferrer"
        className={`p-3 bg-green-500 text-white rounded-full hover:bg-green-600 transition shadow-lg ${className}`}
        title="Contact via WhatsApp"
      >
        <MessageCircle size={24} />
      </a>
    );
  }

  if (variant === 'full') {
    return (
      <a
        href={fullWhatsAppLink}
        target="_blank"
        rel="noopener noreferrer"
        className={`flex items-center justify-center gap-3 w-full px-6 py-4 bg-green-500 text-white rounded-xl font-semibold text-lg hover:bg-green-600 transition shadow-lg ${className}`}
      >
        <MessageCircle size={24} />
        <span>Contact via WhatsApp</span>
      </a>
    );
  }

  // Default button variant
  return (
    <a
      href={fullWhatsAppLink}
      target="_blank"
      rel="noopener noreferrer"
      className={`inline-flex items-center gap-2 px-5 py-2.5 bg-green-500 text-white rounded-lg font-medium hover:bg-green-600 transition ${className}`}
    >
      <MessageCircle size={20} />
      <span>WhatsApp</span>
    </a>
  );
};

export default WhatsAppButton;
```

---

### 2. Post Detail Page with WhatsApp Contact

```tsx
// pages/PostDetail.tsx

import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { 
  MapPin, 
  Calendar, 
  User, 
  Star, 
  Phone, 
  Mail,
  MessageCircle,
  Share2
} from 'lucide-react';
import postService from '../services/postService';
import { PostDetailsResponse } from '../types/post';
import WhatsAppButton from '../components/WhatsAppButton';

const PostDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [post, setPost] = useState<PostDetailsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedImage, setSelectedImage] = useState(0);

  useEffect(() => {
    if (id) {
      fetchPostDetails(parseInt(id));
    }
  }, [id]);

  const fetchPostDetails = async (postId: number) => {
    try {
      setLoading(true);
      const data = await postService.getPostDetails(postId);
      setPost(data);
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Failed to load post details');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
      </div>
    );
  }

  if (error || !post) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-800 mb-2">Post Not Found</h2>
          <p className="text-gray-600">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Left Column - Images & Details */}
        <div className="lg:col-span-2 space-y-6">
          {/* Image Gallery */}
          <div className="bg-white rounded-2xl shadow-sm overflow-hidden">
            {post.images.length > 0 ? (
              <>
                <div className="aspect-video bg-gray-100">
                  <img
                    src={post.images[selectedImage]?.postImageURL}
                    alt={post.postTitle}
                    className="w-full h-full object-cover"
                  />
                </div>
                {post.images.length > 1 && (
                  <div className="flex gap-2 p-4 overflow-x-auto">
                    {post.images.map((img, idx) => (
                      <button
                        key={img.postImageID}
                        onClick={() => setSelectedImage(idx)}
                        className={`flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden border-2 ${
                          selectedImage === idx ? 'border-blue-500' : 'border-transparent'
                        }`}
                      >
                        <img
                          src={img.postImageURL}
                          alt={`Thumbnail ${idx + 1}`}
                          className="w-full h-full object-cover"
                        />
                      </button>
                    ))}
                  </div>
                )}
              </>
            ) : (
              <div className="aspect-video bg-gray-100 flex items-center justify-center">
                <span className="text-gray-400">No images</span>
              </div>
            )}
          </div>

          {/* Post Info */}
          <div className="bg-white rounded-2xl shadow-sm p-6">
            <div className="flex items-start justify-between mb-4">
              <div>
                <span className="inline-block px-3 py-1 bg-blue-100 text-blue-700 text-sm font-medium rounded-full mb-2">
                  {post.categoryName}
                </span>
                <h1 className="text-2xl font-bold text-gray-900">{post.postTitle}</h1>
              </div>
              <p className="text-3xl font-bold text-blue-600">
                {post.price?.toLocaleString()} JD
              </p>
            </div>

            <p className="text-gray-600 whitespace-pre-wrap">{post.postDescription}</p>

            <div className="flex items-center gap-4 mt-6 pt-6 border-t text-sm text-gray-500">
              <div className="flex items-center gap-1">
                <Calendar size={16} />
                <span>{new Date(post.createdAt).toLocaleDateString()}</span>
              </div>
              {post.averageRating && (
                <div className="flex items-center gap-1">
                  <Star size={16} className="text-yellow-500 fill-yellow-500" />
                  <span>{post.averageRating} ({post.reviewCount} reviews)</span>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Right Column - Seller Info & Contact */}
        <div className="space-y-6">
          
          {/* Seller Card */}
          <div className="bg-white rounded-2xl shadow-sm p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Seller Information</h3>
            
            {/* Seller Avatar & Name */}
            <div className="flex items-center gap-4 mb-6">
              <div className="w-16 h-16 bg-gradient-to-br from-blue-500 to-purple-500 rounded-full flex items-center justify-center text-white text-xl font-bold">
                {post.ownerFirstName?.charAt(0)}{post.ownerLastName?.charAt(0)}
              </div>
              <div>
                <p className="font-semibold text-gray-900">{post.ownerFullName}</p>
                <p className="text-sm text-gray-500">@{post.ownerUsername}</p>
              </div>
            </div>

            {/* Contact Info */}
            <div className="space-y-3 mb-6">
              {post.ownerEmail && (
                <div className="flex items-center gap-3 text-gray-600">
                  <Mail size={18} className="text-gray-400" />
                  <span className="text-sm">{post.ownerEmail}</span>
                </div>
              )}
              {post.ownerPrimaryPhone && (
                <div className="flex items-center gap-3 text-gray-600">
                  <Phone size={18} className="text-gray-400" />
                  <span className="text-sm">{post.ownerPrimaryPhone}</span>
                </div>
              )}
            </div>

            {/* WhatsApp Contact Button */}
            <WhatsAppButton
              phoneNumber={post.ownerPrimaryPhone}
              whatsAppLink={post.whatsAppLink}
              ownerName={post.ownerFullName}
              postTitle={post.postTitle}
              variant="full"
            />

            {/* No Phone Available Message */}
            {!post.hasOwnerPhone && (
              <div className="mt-4 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                <p className="text-sm text-yellow-700">
                  ?? This seller hasn't added a phone number yet.
                  You can try contacting them via email.
                </p>
              </div>
            )}
          </div>

          {/* Share Button */}
          <button className="w-full flex items-center justify-center gap-2 px-6 py-3 bg-gray-100 text-gray-700 rounded-xl font-medium hover:bg-gray-200 transition">
            <Share2 size={20} />
            <span>Share Listing</span>
          </button>
        </div>
      </div>
    </div>
  );
};

export default PostDetail;
```

---

### 3. Floating WhatsApp Button (Optional)

For a sticky WhatsApp button at the bottom of the page:

```tsx
// components/FloatingWhatsAppButton.tsx

import React from 'react';
import { MessageCircle } from 'lucide-react';

interface FloatingWhatsAppButtonProps {
  phoneNumber: string | null;
  postTitle: string;
  ownerName: string | null;
}

const FloatingWhatsAppButton: React.FC<FloatingWhatsAppButtonProps> = ({
  phoneNumber,
  postTitle,
  ownerName
}) => {
  if (!phoneNumber) return null;

  const message = `Hi ${ownerName || 'there'}! I'm interested in "${postTitle}" on TijarahJo.`;
  const cleanPhone = phoneNumber.replace(/[\s\-\+]/g, '');
  const whatsAppLink = `https://wa.me/${cleanPhone}?text=${encodeURIComponent(message)}`;

  return (
    <a
      href={whatsAppLink}
      target="_blank"
      rel="noopener noreferrer"
      className="fixed bottom-6 right-6 z-50 flex items-center gap-3 px-6 py-4 bg-green-500 text-white rounded-full font-semibold shadow-2xl hover:bg-green-600 hover:scale-105 transition-all"
    >
      <MessageCircle size={24} />
      <span className="hidden sm:inline">Chat on WhatsApp</span>
    </a>
  );
};

export default FloatingWhatsAppButton;
```

---

## ?? WhatsApp URL Format

### Basic Format
```
https://wa.me/{phoneNumber}
```

### With Pre-filled Message
```
https://wa.me/{phoneNumber}?text={urlEncodedMessage}
```

### Phone Number Format
- Remove `+` sign
- Remove spaces and dashes
- Example: `+962 79 123 4567` ? `962791234567`

### Example URLs
```
https://wa.me/962791234567
https://wa.me/962791234567?text=Hi%20I%27m%20interested%20in%20your%20iPhone
```

---

## ?? Complete Flow

```
???????????????????????????????????????????????????????????????
?                       FRONTEND                               ?
???????????????????????????????????????????????????????????????
?                                                              ?
?  1. User views post listing                                  ?
?           ?                                                  ?
?  2. Call GET /api/posts/{id}/details                        ?
?           ?                                                  ?
?  3. Receive response with:                                   ?
?     - ownerPrimaryPhone: "+962791234567"                    ?
?     - hasOwnerPhone: true                                   ?
?     - whatsAppLink: "https://wa.me/962791234567"           ?
?           ?                                                  ?
?  4. Show WhatsApp button if hasOwnerPhone = true            ?
?           ?                                                  ?
?  5. User clicks button ? Opens WhatsApp                     ?
?                                                              ?
???????????????????????????????????????????????????????????????
```

---

## ?? Database Script Required

Before this works, run this SQL script in SSMS to update the stored procedure:

```sql
-- File: TijarahJoDBAPI\Scripts\AddOwnerPhoneToPostDetails.sql

USE TijarahJoDB;
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_GetPostDetails_All]
(
    @PostID INT,
    @IncludeDeleted BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

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
    
    -- ... rest of procedure (reviews, images)
END;
GO
```

---

## ?? Testing Checklist

### API Testing
- [ ] `GET /api/posts/{id}/details` returns `ownerPrimaryPhone`
- [ ] `hasOwnerPhone` is `true` when phone exists
- [ ] `hasOwnerPhone` is `false` when no phone
- [ ] `whatsAppLink` is properly formatted

### Frontend Testing
- [ ] WhatsApp button shows when phone available
- [ ] WhatsApp button hidden/disabled when no phone
- [ ] Clicking button opens WhatsApp
- [ ] Pre-filled message appears in WhatsApp
- [ ] Works on mobile (opens WhatsApp app)
- [ ] Works on desktop (opens WhatsApp Web)

---

## ?? Mobile Considerations

### Opening WhatsApp on Mobile
The `wa.me` links automatically:
- Open WhatsApp app on iOS/Android if installed
- Redirect to App Store/Play Store if not installed
- Open WhatsApp Web on desktop browsers

### Deep Link Behavior
```javascript
// This works automatically with wa.me links
<a href="https://wa.me/962791234567">Contact</a>

// For more control, you can detect mobile
const isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
const whatsappUrl = isMobile 
  ? `whatsapp://send?phone=${phone}` 
  : `https://web.whatsapp.com/send?phone=${phone}`;
```

---

## ?? API Reference

### Endpoints Used

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/posts/{id}/details` | Get post with owner phone |
| GET | `/posts/{id}` | Basic post info (no phone) |
| GET | `/users/{id}/phones` | Get all user phones (Auth) |

### Response Fields

| Field | Type | Always Present | Description |
|-------|------|----------------|-------------|
| `ownerPrimaryPhone` | string? | ? | Phone or null |
| `hasOwnerPhone` | boolean | ? | Helper flag |
| `whatsAppLink` | string? | ? | Ready URL or null |

---

## ?? UI/UX Best Practices

### WhatsApp Button Design
- Use WhatsApp's green color: `#25D366` or `#128C7E`
- Use the official WhatsApp icon or message bubble
- Make it prominent but not intrusive
- Show loading state while fetching

### Fallback When No Phone
- Don't hide the section entirely
- Show a helpful message
- Offer email as alternative
- Suggest seller add phone number

### Mobile-First
- Large touch targets (min 44x44px)
- Full-width buttons on mobile
- Floating action button for easy access

---

*Last Updated: December 2024*  
*API Version: 1.0*
