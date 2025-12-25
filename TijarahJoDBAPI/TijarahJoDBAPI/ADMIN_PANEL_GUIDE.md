# TijarahJo Admin Panel - Frontend Development Guide

## ?? Overview

This document provides complete guidance for building the TijarahJo Admin Dashboard. The API is built with .NET 8 and uses JWT authentication with role-based access control.

**Base URL:** `https://localhost:7064/api` (Development)

---

## ?? Authentication

### Login Flow

1. User submits credentials to `/api/users/login`
2. API returns JWT token + user info
3. Store token in localStorage/sessionStorage
4. Include token in all subsequent requests as: `Authorization: Bearer <token>`

### Token Details
- **Expiration:** Token expires based on server configuration
- **Header:** `Token-Expired: true` is returned when token is expired
- **Roles:** Admin, User, Moderator

---

## ?? Admin Dashboard Structure

### Recommended Layout

```
???????????????????????????????????????????????????????????????????
?  HEADER: Logo | Search Bar | Notifications | Profile Dropdown  ?
???????????????????????????????????????????????????????????????????
?              ?                                                  ?
?   SIDEBAR    ?              MAIN CONTENT AREA                   ?
?              ?                                                  ?
?  Dashboard   ?   ???????????????????????????????????????????    ?
?  Users       ?   ?  Statistics Cards (Total Users, Posts,  ?    ?
?  Posts       ?   ?  Reviews, Active Users Today)           ?    ?
?  Categories  ?   ???????????????????????????????????????????    ?
?  Reviews     ?                                                  ?
?  Roles       ?   ???????????????????????????????????????????    ?
?  Settings    ?   ?  Data Table / Content Area              ?    ?
?              ?   ?                                         ?    ?
?              ?   ?                                         ?    ?
?              ?   ???????????????????????????????????????????    ?
?              ?                                                  ?
???????????????????????????????????????????????????????????????????
```

---

## ?? Dashboard Home Page

### Statistics to Display
- Total Users Count
- Total Posts Count
- Total Categories Count
- Total Reviews Count
- New Users This Week
- New Posts This Week
- Active/Blocked Users ratio

### Quick Actions
- View Recent Users
- View Recent Posts
- View Pending Reviews

---

## ?? Users Management

### Features Required
- ? List all users with pagination
- ? Search users by name/email
- ? View user details
- ? Create new user (with role assignment)
- ? Update user information
- ? Delete/Block user (soft delete)
- ? Change user role
- ? Manage user phone numbers
- ? View user images

### Endpoints

#### Get All Users (Admin Only)
```http
GET /api/users
Authorization: Bearer <token>
```

**Response:**
```json
[
  {
    "userID": 1,
    "username": "ahmad_admin",
    "email": "ahmad@example.com",
    "firstName": "Ahmad",
    "lastName": "Hirzallah",
    "primaryPhone": "+962791234567",
    "joinDate": "2024-01-15T10:30:00Z",
    "status": 1,
    "roleID": 1,
    "isDeleted": false,
    "fullName": "Ahmad Hirzallah"
  }
]
```

#### Get User by ID
```http
GET /api/users/{id}
Authorization: Bearer <token>
```

#### Create User (Admin)
```http
POST /api/users
Authorization: Bearer <token>
Content-Type: application/json

{
  "username": "newuser",
  "password": "SecurePass123",
  "email": "newuser@example.com",
  "firstName": "New",
  "lastName": "User",
  "phoneNumber": "+962791234567",
  "status": 1,
  "roleID": 2
}
```

**Role IDs:**
- `1` = Admin
- `2` = User (Regular)
- `3` = Moderator

#### Update User
```http
PUT /api/users/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "username": "updateduser",
  "password": "NewPassword123",
  "email": "updated@example.com",
  "firstName": "Updated",
  "lastName": "User",
  "status": 1,
  "roleID": 2,
  "isDeleted": false
}
```

#### Delete User (Soft Delete)
```http
DELETE /api/users/{id}
Authorization: Bearer <token>
```

#### Block/Unblock User
To block a user, update their `status` to `0` or `isDeleted` to `true`:
```http
PUT /api/users/{id}
{
  "isDeleted": true
}
```

---

## ?? User Phone Numbers

Each user can have multiple phone numbers. One can be marked as primary.

### Get User's Phone Numbers
```http
GET /api/users/{userId}/phones
Authorization: Bearer <token>
```

**Response:**
```json
{
  "userID": 1,
  "phoneNumbers": [
    {
      "phoneID": 1,
      "userID": 1,
      "phoneNumber": "+962791234567",
      "isPrimary": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "isDeleted": false
    },
    {
      "phoneID": 2,
      "userID": 1,
      "phoneNumber": "+962799876543",
      "isPrimary": false,
      "createdAt": "2024-02-01T15:00:00Z",
      "isDeleted": false
    }
  ],
  "totalCount": 2,
  "primaryPhone": "+962791234567"
}
```

### Add Phone Number
```http
POST /api/users/{userId}/phones
Authorization: Bearer <token>
Content-Type: application/json

{
  "phoneNumber": "+962791234567",
  "isPrimary": true
}
```

### Update Phone Number
```http
PUT /api/users/{userId}/phones/{phoneId}
Authorization: Bearer <token>
Content-Type: application/json

{
  "phoneNumber": "+962791234567",
  "isPrimary": true,
  "isDeleted": false
}
```

### Delete Phone Number
```http
DELETE /api/users/{userId}/phones/{phoneId}
Authorization: Bearer <token>
```

---

## ?? Posts Management

### Features Required
- ? List all posts with filters
- ? Search posts by title
- ? Filter by category
- ? Filter by user
- ? View post details with images
- ? Create/Edit/Delete posts
- ? Manage post images
- ? View post reviews

### Endpoints

#### Get All Posts
```http
GET /api/posts
```

**Response:**
```json
[
  {
    "postID": 1,
    "userID": 1,
    "categoryID": 2,
    "postTitle": "iPhone 15 Pro Max",
    "postDescription": "Brand new iPhone...",
    "price": 1200.00,
    "status": 1,
    "createdAt": "2024-01-20T12:00:00Z",
    "isDeleted": false
  }
]
```

#### Get Post with Full Details
```http
GET /api/posts/{id}/details
```

**Response includes:**
- Post information
- User information (seller)
- Category information
- All post images
- All reviews with reviewer info

#### Create Post
```http
POST /api/posts
Authorization: Bearer <token>
Content-Type: application/json

{
  "userID": 1,
  "categoryID": 2,
  "postTitle": "New Product",
  "postDescription": "Description here",
  "price": 500.00,
  "status": 1
}
```

#### Update Post
```http
PUT /api/posts/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "categoryID": 2,
  "postTitle": "Updated Title",
  "postDescription": "Updated description",
  "price": 550.00,
  "status": 1,
  "isDeleted": false
}
```

#### Delete Post
```http
DELETE /api/posts/{id}
Authorization: Bearer <token>
```

---

## ??? Post Images

### Get Post Images
```http
GET /api/posts/{postId}/images
```

### Upload Image (File)
```http
POST /api/posts/{postId}/images/upload
Authorization: Bearer <token>
Content-Type: multipart/form-data

file: <image file>
```

### Upload Image (Base64)
```http
POST /api/posts/{postId}/images/upload-base64
Authorization: Bearer <token>
Content-Type: application/json

{
  "imageData": "data:image/png;base64,iVBORw0KGgo..."
}
```

### Delete Image
```http
DELETE /api/posts/{postId}/images/{imageId}
Authorization: Bearer <token>
```

---

## ?? Categories Management

### Features Required
- ? List all categories
- ? Create new category
- ? Edit category
- ? Delete category
- ? View posts in category

### Endpoints

#### Get All Categories
```http
GET /api/categories
```

**Response:**
```json
[
  {
    "categoryID": 1,
    "categoryName": "Electronics",
    "createdAt": "2024-01-01T00:00:00Z",
    "isDeleted": false
  }
]
```

#### Create Category
```http
POST /api/categories
Authorization: Bearer <token>
Content-Type: application/json

{
  "categoryName": "New Category"
}
```

#### Update Category
```http
PUT /api/categories/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "categoryName": "Updated Name",
  "isDeleted": false
}
```

#### Delete Category
```http
DELETE /api/categories/{id}
Authorization: Bearer <token>
```

---

## ? Reviews Management

### Features Required
- ? List all reviews
- ? Filter reviews by post
- ? Filter reviews by user
- ? View review details
- ? Delete inappropriate reviews
- ? View ratings statistics

### Endpoints

#### Get All Reviews for a Post
```http
GET /api/posts/{postId}/reviews
```

**Response:**
```json
[
  {
    "reviewID": 1,
    "postID": 5,
    "userID": 3,
    "rating": 5,
    "reviewText": "Great product!",
    "createdAt": "2024-02-01T10:00:00Z",
    "isDeleted": false
  }
]
```

#### Create Review
```http
POST /api/posts/{postId}/reviews
Authorization: Bearer <token>
Content-Type: application/json

{
  "rating": 5,
  "reviewText": "Excellent!"
}
```

#### Delete Review
```http
DELETE /api/posts/{postId}/reviews/{reviewId}
Authorization: Bearer <token>
```

---

## ?? Roles Management

### Get All Roles
```http
GET /api/roles
Authorization: Bearer <token>
```

**Response:**
```json
[
  {
    "roleID": 1,
    "roleName": "Admin",
    "createdAt": "2024-01-01T00:00:00Z",
    "isDeleted": false
  },
  {
    "roleID": 2,
    "roleName": "User",
    "createdAt": "2024-01-01T00:00:00Z",
    "isDeleted": false
  },
  {
    "roleID": 3,
    "roleName": "Moderator",
    "createdAt": "2024-01-01T00:00:00Z",
    "isDeleted": false
  }
]
```

---

## ?? UI Components Recommendations

### Data Tables
- Use pagination (10, 25, 50 items per page)
- Add search/filter functionality
- Add sorting by columns
- Include action buttons (Edit, Delete, View)
- Show status badges (Active/Blocked/Deleted)

### Forms
- Use validation matching API requirements
- Show loading states during submission
- Display success/error messages
- Confirm before delete operations

### Status Badges
```
Active (Green)   - isDeleted: false, status: 1
Blocked (Red)    - isDeleted: true
Pending (Yellow) - status: 0
```

### Role Badges
```
Admin (Purple)     - roleID: 1
User (Blue)        - roleID: 2
Moderator (Orange) - roleID: 3
```

---

## ?? Error Handling

### Common Error Responses

**400 Bad Request:**
```json
{
  "title": "Invalid Data",
  "detail": "Description of the error",
  "status": 400
}
```

**401 Unauthorized:**
```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "You are not authorized to access this resource."
}
```

**403 Forbidden:**
```json
{
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to access this resource."
}
```

**404 Not Found:**
```json
{
  "title": "Not Found",
  "detail": "Resource not found",
  "status": 404
}
```

---

## ?? Responsive Design

### Breakpoints Recommendation
- Mobile: < 768px
- Tablet: 768px - 1024px
- Desktop: > 1024px

### Mobile Sidebar
- Collapsible sidebar with hamburger menu
- Bottom navigation for main actions

---

## ?? Recommended Tech Stack

### Frontend Framework
- **React.js** or **Vue.js** or **Angular**
- **Next.js** (for React with SSR)

### UI Library
- **Ant Design** (antd)
- **Material-UI** (MUI)
- **Tailwind CSS** + **Headless UI**
- **shadcn/ui**

### State Management
- React Query / TanStack Query (for API calls)
- Zustand or Redux (for global state)

### HTTP Client
- Axios
- Fetch API

---

## ?? Implementation Checklist

### Phase 1: Setup
- [ ] Project setup with chosen framework
- [ ] Authentication flow (login/logout)
- [ ] Protected routes
- [ ] Layout with sidebar

### Phase 2: Core Features
- [ ] Dashboard with statistics
- [ ] Users CRUD
- [ ] Posts CRUD
- [ ] Categories CRUD

### Phase 3: Advanced Features
- [ ] Reviews management
- [ ] Image upload/management
- [ ] Phone numbers management
- [ ] Search & filters
- [ ] Pagination

### Phase 4: Polish
- [ ] Error handling
- [ ] Loading states
- [ ] Responsive design
- [ ] Notifications/Toasts

---

## ?? Quick Start Example (React + Axios)

```javascript
// api.js
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7064/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle token expiration
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;

// Usage:
// Login
const login = async (credentials) => {
  const response = await api.post('/users/login', credentials);
  localStorage.setItem('token', response.data.token);
  return response.data;
};

// Get all users
const getUsers = async () => {
  const response = await api.get('/users');
  return response.data;
};

// Create user
const createUser = async (userData) => {
  const response = await api.post('/users', userData);
  return response.data;
};
```

---

## ?? Contact & Support

For API issues or questions, contact the backend team.

**API Documentation:** Available at `/swagger` when running in development mode.
