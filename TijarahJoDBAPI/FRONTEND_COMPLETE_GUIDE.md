# TijarahJo - Complete Frontend Integration Guide

## ?? AI Prompt for Frontend Development

> **Copy this prompt and give it to your AI assistant along with this document:**
>
> "I am building a frontend for TijarahJo e-commerce platform. The backend is a .NET 8 REST API with JWT authentication. I need to implement:
> 1. User authentication (login/register/logout)
> 2. Admin dashboard with full CRUD for users, posts, categories, reviews
> 3. User profile management with password change, phone numbers, and images
> 4. Role-based access control (Admin, User, Moderator)
> 
> Please help me implement these features following the API documentation provided. Use React/Next.js with TypeScript, Axios for API calls, and a modern UI library like Ant Design or Material-UI. Implement proper error handling, loading states, and form validation."

---

## ?? AUTHENTICATION SYSTEM

### How Authentication Works

1. User logs in with username/email + password
2. Server returns JWT token + user info
3. Store token in `localStorage` or `sessionStorage`
4. Include token in ALL authenticated requests
5. Handle token expiration (redirect to login)

### Login Endpoint

```http
POST /api/users/login
Content-Type: application/json
```

**Request:**
```json
{
  "login": "admin@tijarahjo.com",
  "password": "Admin123!"
}
```

**Success Response (200):**
```json
{
  "user": {
    "userID": 1,
    "username": "admin",
    "email": "admin@tijarahjo.com",
    "firstName": "Admin",
    "lastName": "User",
    "primaryPhone": "+962791234567",
    "joinDate": "2024-01-01T00:00:00Z",
    "status": 0,
    "roleID": 1,
    "isDeleted": false,
    "fullName": "Admin User"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-26T10:30:00Z",
  "role": "Admin"
}
```

**Error Response (401):**
```json
{
  "title": "Invalid credentials",
  "status": 401
}
```

### After Login - Store Token

```javascript
// After successful login
const loginResponse = await api.post('/users/login', credentials);

// Store token
localStorage.setItem('token', loginResponse.data.token);
localStorage.setItem('user', JSON.stringify(loginResponse.data.user));
localStorage.setItem('role', loginResponse.data.role);
localStorage.setItem('tokenExpiry', loginResponse.data.expiresAt);

// Redirect based on role
if (loginResponse.data.role === 'Admin') {
  window.location.href = '/admin/dashboard';
} else {
  window.location.href = '/profile';
}
```

### Logout

```javascript
const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  localStorage.removeItem('role');
  localStorage.removeItem('tokenExpiry');
  window.location.href = '/login';
};
```

---

## ?? PASSWORD CHANGE - COMPLETE GUIDE

### Password Change Flow

```
???????????????????????????????????????????????????????????????
?                  CHANGE PASSWORD FORM                       ?
???????????????????????????????????????????????????????????????
?                                                             ?
?  Current Password:  [••••••••••••]                          ?
?                                                             ?
?  New Password:      [••••••••••••]                          ?
?                     ? At least 6 characters                 ?
?                                                             ?
?  Confirm Password:  [••••••••••••]                          ?
?                     ? Passwords match                       ?
?                                                             ?
?              [Cancel]  [Change Password]                    ?
?                                                             ?
???????????????????????????????????????????????????????????????
```

### API Endpoint

```http
PUT /api/users/{userId}/password
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "currentPassword": "OldPassword123",
  "newPassword": "NewSecurePassword456"
}
```

**Validation Rules:**
| Field | Required | Rules |
|-------|----------|-------|
| currentPassword | ? Yes | Must match existing password |
| newPassword | ? Yes | Minimum 6 characters |

### Success Response
```
204 No Content
```

### Error Responses

**400 - Wrong Current Password:**
```json
{
  "title": "Invalid current password",
  "status": 400
}
```

**400 - Validation Error:**
```json
{
  "errors": {
    "NewPassword": ["Password must be at least 6 characters."]
  }
}
```

**401 - Not Authenticated:**
```json
{
  "title": "Unauthorized",
  "status": 401
}
```

**403 - Not Allowed (trying to change other user's password):**
```json
{
  "title": "Forbidden",
  "status": 403
}
```

### Complete React Component Example

```jsx
// ChangePasswordForm.jsx
import { useState } from 'react';
import api from '../services/api';

function ChangePasswordForm({ userId, onSuccess, onCancel }) {
  const [formData, setFormData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [showPasswords, setShowPasswords] = useState({
    current: false,
    new: false,
    confirm: false
  });

  // Validation
  const validate = () => {
    const newErrors = {};

    if (!formData.currentPassword) {
      newErrors.currentPassword = 'Current password is required';
    }

    if (!formData.newPassword) {
      newErrors.newPassword = 'New password is required';
    } else if (formData.newPassword.length < 6) {
      newErrors.newPassword = 'Password must be at least 6 characters';
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your new password';
    } else if (formData.newPassword !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    if (formData.currentPassword === formData.newPassword) {
      newErrors.newPassword = 'New password must be different from current password';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validate()) return;

    setLoading(true);
    setErrors({});

    try {
      await api.put(`/users/${userId}/password`, {
        currentPassword: formData.currentPassword,
        newPassword: formData.newPassword
      });

      // Success!
      alert('Password changed successfully!');
      setFormData({ currentPassword: '', newPassword: '', confirmPassword: '' });
      onSuccess?.();
      
    } catch (error) {
      if (error.response?.status === 400) {
        const errorData = error.response.data;
        
        if (errorData.title === 'Invalid current password') {
          setErrors({ currentPassword: 'Current password is incorrect' });
        } else if (errorData.errors) {
          // Validation errors from server
          setErrors(errorData.errors);
        } else {
          setErrors({ general: 'Failed to change password' });
        }
      } else if (error.response?.status === 401) {
        setErrors({ general: 'Session expired. Please login again.' });
        // Redirect to login
        window.location.href = '/login';
      } else if (error.response?.status === 403) {
        setErrors({ general: 'You are not allowed to change this password' });
      } else {
        setErrors({ general: 'An error occurred. Please try again.' });
      }
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: null }));
    }
  };

  return (
    <form onSubmit={handleSubmit} className="password-form">
      <h3>Change Password</h3>
      
      {errors.general && (
        <div className="error-banner">{errors.general}</div>
      )}

      {/* Current Password */}
      <div className="form-group">
        <label>Current Password *</label>
        <div className="password-input">
          <input
            type={showPasswords.current ? 'text' : 'password'}
            name="currentPassword"
            value={formData.currentPassword}
            onChange={handleChange}
            placeholder="Enter current password"
            className={errors.currentPassword ? 'error' : ''}
          />
          <button 
            type="button" 
            onClick={() => setShowPasswords(p => ({...p, current: !p.current}))}
          >
            {showPasswords.current ? '??' : '???'}
          </button>
        </div>
        {errors.currentPassword && (
          <span className="error-text">{errors.currentPassword}</span>
        )}
      </div>

      {/* New Password */}
      <div className="form-group">
        <label>New Password *</label>
        <div className="password-input">
          <input
            type={showPasswords.new ? 'text' : 'password'}
            name="newPassword"
            value={formData.newPassword}
            onChange={handleChange}
            placeholder="Enter new password (min 6 characters)"
            className={errors.newPassword ? 'error' : ''}
          />
          <button 
            type="button" 
            onClick={() => setShowPasswords(p => ({...p, new: !p.new}))}
          >
            {showPasswords.new ? '??' : '???'}
          </button>
        </div>
        {errors.newPassword && (
          <span className="error-text">{errors.newPassword}</span>
        )}
        {formData.newPassword && formData.newPassword.length >= 6 && (
          <span className="success-text">? Password length OK</span>
        )}
      </div>

      {/* Confirm Password */}
      <div className="form-group">
        <label>Confirm New Password *</label>
        <div className="password-input">
          <input
            type={showPasswords.confirm ? 'text' : 'password'}
            name="confirmPassword"
            value={formData.confirmPassword}
            onChange={handleChange}
            placeholder="Confirm new password"
            className={errors.confirmPassword ? 'error' : ''}
          />
          <button 
            type="button" 
            onClick={() => setShowPasswords(p => ({...p, confirm: !p.confirm}))}
          >
            {showPasswords.confirm ? '??' : '???'}
          </button>
        </div>
        {errors.confirmPassword && (
          <span className="error-text">{errors.confirmPassword}</span>
        )}
        {formData.confirmPassword && formData.newPassword === formData.confirmPassword && (
          <span className="success-text">? Passwords match</span>
        )}
      </div>

      {/* Buttons */}
      <div className="form-actions">
        <button type="button" onClick={onCancel} disabled={loading}>
          Cancel
        </button>
        <button type="submit" disabled={loading} className="primary">
          {loading ? 'Changing...' : 'Change Password'}
        </button>
      </div>
    </form>
  );
}

export default ChangePasswordForm;
```

---

## ?? ADMIN DASHBOARD - COMPLETE GUIDE

### Role-Based Access Control

| Role | RoleID | Can Access |
|------|--------|------------|
| **Admin** | 1 | Everything - Full CRUD on all resources |
| **User** | 2 | Own profile, own posts, own reviews |
| **Moderator** | 3 | Can moderate posts and reviews |

### Check User Role in Frontend

```javascript
// utils/auth.js
export const getCurrentUser = () => {
  const user = localStorage.getItem('user');
  return user ? JSON.parse(user) : null;
};

export const getUserRole = () => {
  return localStorage.getItem('role') || 'User';
};

export const isAdmin = () => {
  return getUserRole() === 'Admin';
};

export const isModerator = () => {
  return ['Admin', 'Moderator'].includes(getUserRole());
};

export const isAuthenticated = () => {
  const token = localStorage.getItem('token');
  const expiry = localStorage.getItem('tokenExpiry');
  
  if (!token || !expiry) return false;
  
  // Check if token is expired
  return new Date(expiry) > new Date();
};

// Protected Route Component
export const AdminRoute = ({ children }) => {
  if (!isAuthenticated()) {
    return <Navigate to="/login" />;
  }
  
  if (!isAdmin()) {
    return <Navigate to="/unauthorized" />;
  }
  
  return children;
};
```

---

## ?? ADMIN DASHBOARD ENDPOINTS

### 1. USERS MANAGEMENT (Admin Only)

#### Get All Users
```http
GET /api/users
Authorization: Bearer <admin_token>
```

**Response:**
```json
[
  {
    "userID": 1,
    "username": "admin",
    "email": "admin@tijarahjo.com",
    "firstName": "Admin",
    "lastName": "User",
    "primaryPhone": "+962791234567",
    "joinDate": "2024-01-01T00:00:00Z",
    "status": 0,
    "roleID": 1,
    "isDeleted": false,
    "fullName": "Admin User"
  },
  {
    "userID": 2,
    "username": "user1",
    "email": "user1@example.com",
    "firstName": "Regular",
    "lastName": "User",
    "primaryPhone": null,
    "joinDate": "2024-01-15T00:00:00Z",
    "status": 0,
    "roleID": 2,
    "isDeleted": false,
    "fullName": "Regular User"
  }
]
```

#### Create User (Admin)
```http
POST /api/users
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request:**
```json
{
  "username": "newuser",
  "password": "Password123",
  "email": "newuser@example.com",
  "firstName": "New",
  "lastName": "User",
  "phoneNumber": "+962791234567",
  "status": 0,
  "roleID": 2
}
```

#### Update User (Admin)
```http
PUT /api/users/{userId}
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request:**
```json
{
  "username": "updateduser",
  "password": "NewPassword123",
  "email": "updated@example.com",
  "firstName": "Updated",
  "lastName": "User",
  "status": 0,
  "roleID": 2,
  "isDeleted": false
}
```

**?? IMPORTANT:** When updating a user, you MUST provide the password field. If you don't want to change the password, you need to either:
1. Ask user to enter their password
2. Or create a separate endpoint for profile updates (use `PUT /api/users/me` for profile-only updates)

#### Delete User (Soft Delete)
```http
DELETE /api/users/{userId}
Authorization: Bearer <admin_token>
```

**Response:** `204 No Content`

#### Block/Unblock User
To block a user, update with `isDeleted: true`:
```http
PUT /api/users/{userId}
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "isDeleted": true,
  // ... other required fields
}
```

---

### 2. POSTS MANAGEMENT

#### Get All Posts
```http
GET /api/posts
```

#### Get Paginated Posts
```http
GET /api/posts/paged?pageNumber=1&rowsPerPage=10&categoryId=1&includeDeleted=false
```

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Page number (1-based) |
| rowsPerPage | int | 10 | Items per page (max 200) |
| categoryId | int? | null | Filter by category |
| includeDeleted | bool | false | Include deleted posts |

#### Get Post Details
```http
GET /api/posts/{postId}/details
```

**Response includes:**
- Post information
- Seller (user) information
- Category information
- All post images
- All reviews

#### Create Post
```http
POST /api/posts
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "categoryID": 1,
  "postTitle": "iPhone 15 Pro Max",
  "postDescription": "Brand new iPhone 15 Pro Max, 256GB, Black",
  "price": 1200.00,
  "status": 1
}
```

#### Update Post
```http
PUT /api/posts/{postId}
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "categoryID": 1,
  "postTitle": "iPhone 15 Pro Max - Updated",
  "postDescription": "Updated description",
  "price": 1100.00,
  "status": 1,
  "isDeleted": false
}
```

#### Delete Post
```http
DELETE /api/posts/{postId}
Authorization: Bearer <token>
```

#### Upload Post Image
```http
POST /api/posts/{postId}/images/upload
Authorization: Bearer <token>
Content-Type: multipart/form-data

file: <image_file>
```

---

### 3. CATEGORIES MANAGEMENT

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
  },
  {
    "categoryID": 2,
    "categoryName": "Vehicles",
    "createdAt": "2024-01-01T00:00:00Z",
    "isDeleted": false
  }
]
```

#### Create Category (Admin)
```http
POST /api/categories
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request:**
```json
{
  "categoryName": "New Category"
}
```

#### Update Category (Admin)
```http
PUT /api/categories/{categoryId}
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request:**
```json
{
  "categoryName": "Updated Category Name",
  "isDeleted": false
}
```

#### Delete Category (Admin)
```http
DELETE /api/categories/{categoryId}
Authorization: Bearer <admin_token>
```

---

### 4. REVIEWS MANAGEMENT

#### Get Post Reviews
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
```

**Request:**
```json
{
  "rating": 5,
  "reviewText": "Excellent product, highly recommended!"
}
```

#### Delete Review (Owner or Admin)
```http
DELETE /api/posts/{postId}/reviews/{reviewId}
Authorization: Bearer <token>
```

---

### 5. ROLES MANAGEMENT (Admin Only)

#### Get All Roles
```http
GET /api/roles
Authorization: Bearer <admin_token>
```

**Response:**
```json
[
  { "roleID": 1, "roleName": "Admin", "createdAt": "2024-01-01T00:00:00Z", "isDeleted": false },
  { "roleID": 2, "roleName": "User", "createdAt": "2024-01-01T00:00:00Z", "isDeleted": false },
  { "roleID": 3, "roleName": "Moderator", "createdAt": "2024-01-01T00:00:00Z", "isDeleted": false }
]
```

---

## ??? COMPLETE API SERVICE

```javascript
// services/api.js
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7064/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' }
});

// Request interceptor - add token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid
      localStorage.clear();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;

// ============ AUTH API ============
export const authApi = {
  login: (credentials) => api.post('/users/login', credentials),
  register: (userData) => api.post('/users/register', userData),
};

// ============ USERS API ============
export const usersApi = {
  // Admin endpoints
  getAll: () => api.get('/users'),
  getById: (id) => api.get(`/users/${id}`),
  create: (data) => api.post('/users', data),
  update: (id, data) => api.put(`/users/${id}`, data),
  delete: (id) => api.delete(`/users/${id}`),
  
  // Profile endpoints
  getMyProfile: () => api.get('/users/me'),
  updateMyProfile: (data) => api.put('/users/me', data),
  changePassword: (userId, data) => api.put(`/users/${userId}/password`, data),
  
  // Phone numbers
  getPhones: (userId) => api.get(`/users/${userId}/phones`),
  addPhone: (userId, data) => api.post(`/users/${userId}/phones`, data),
  updatePhone: (userId, phoneId, data) => api.put(`/users/${userId}/phones/${phoneId}`, data),
  deletePhone: (userId, phoneId) => api.delete(`/users/${userId}/phones/${phoneId}`),
  
  // Images
  getImages: (userId) => api.get(`/users/${userId}/images`),
  uploadImage: (userId, file) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post(`/users/${userId}/images/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  },
  deleteImage: (userId, imageId) => api.delete(`/users/${userId}/images/${imageId}`),
};

// ============ POSTS API ============
export const postsApi = {
  getAll: () => api.get('/posts'),
  getPaged: (params) => api.get('/posts/paged', { params }),
  getById: (id) => api.get(`/posts/${id}`),
  getDetails: (id) => api.get(`/posts/${id}/details`),
  create: (data) => api.post('/posts', data),
  update: (id, data) => api.put(`/posts/${id}`, data),
  delete: (id) => api.delete(`/posts/${id}`),
  
  // Images
  getImages: (postId) => api.get(`/posts/${postId}/images`),
  uploadImage: (postId, file) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post(`/posts/${postId}/images/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  },
  deleteImage: (postId, imageId) => api.delete(`/posts/${postId}/images/${imageId}`),
  
  // Reviews
  getReviews: (postId) => api.get(`/posts/${postId}/reviews`),
  addReview: (postId, data) => api.post(`/posts/${postId}/reviews`, data),
  deleteReview: (postId, reviewId) => api.delete(`/posts/${postId}/reviews/${reviewId}`),
};

// ============ CATEGORIES API ============
export const categoriesApi = {
  getAll: () => api.get('/categories'),
  getById: (id) => api.get(`/categories/${id}`),
  create: (data) => api.post('/categories', data),
  update: (id, data) => api.put(`/categories/${id}`, data),
  delete: (id) => api.delete(`/categories/${id}`),
};

// ============ ROLES API ============
export const rolesApi = {
  getAll: () => api.get('/roles'),
  getById: (id) => api.get(`/roles/${id}`),
  create: (data) => api.post('/roles', data),
  update: (id, data) => api.put(`/roles/${id}`, data),
  delete: (id) => api.delete(`/roles/${id}`),
};
```

---

## ?? ADMIN DASHBOARD STRUCTURE

```
???????????????????????????????????????????????????????????????????????
?  ?? TijarahJo Admin    [?? Notifications]  [?? Admin ?]             ?
???????????????????????????????????????????????????????????????????????
?             ?                                                       ?
?  ?? Dashboard?   DASHBOARD                                          ?
?             ?   ??????????? ??????????? ??????????? ???????????    ?
?  ?? Users   ?   ? Users   ? ? Posts   ? ?Categories? ? Reviews ?    ?
?             ?   ?   125   ? ?   450   ? ?    12   ? ?   890   ?    ?
?  ?? Posts   ?   ??????????? ??????????? ??????????? ???????????    ?
?             ?                                                       ?
?  ?? Categories  Recent Activity                                     ?
?             ?   ?????????????????????????????????????????           ?
?  ? Reviews ?   • New user registered: Ahmad                        ?
?             ?   • New post: iPhone 15 Pro                           ?
?  ?? Roles   ?   • Review added on Post #45                          ?
?             ?                                                       ?
?  ?? Settings?                                                       ?
?             ?                                                       ?
???????????????????????????????????????????????????????????????????????
```

### Users Management Page

```
???????????????????????????????????????????????????????????????????????
?  Users Management                            [+ Add New User]       ?
???????????????????????????????????????????????????????????????????????
?  Search: [__________________] [??]    Filter: [All Roles ?]         ?
???????????????????????????????????????????????????????????????????????
?  ID ? Username ? Email           ? Role      ? Status  ? Actions   ?
??????????????????????????????????????????????????????????????????????
?  1  ? admin    ? admin@tj.com    ? ?? Admin  ? ? Active? ?? ???    ?
?  2  ? user1    ? user1@tj.com    ? ?? User   ? ? Active? ?? ???    ?
?  3  ? mod1     ? mod1@tj.com     ? ?? Mod    ? ? Active? ?? ???    ?
?  4  ? blocked  ? blocked@tj.com  ? ?? User   ? ?? Blocked? ?? ??    ?
???????????????????????????????????????????????????????????????????????
?  Showing 1-10 of 125       [? Prev] [1] [2] [3] ... [13] [Next ?]  ?
???????????????????????????????????????????????????????????????????????
```

---

## ?? COMMON ISSUES & SOLUTIONS

### Issue 1: "Cannot change password - says incorrect"

**Cause:** The `currentPassword` doesn't match the stored password.

**Solution:**
```javascript
// Make sure you're sending the correct current password
const changePassword = async () => {
  try {
    await api.put(`/users/${userId}/password`, {
      currentPassword: currentPasswordInput.value, // What user entered
      newPassword: newPasswordInput.value
    });
    alert('Success!');
  } catch (error) {
    if (error.response?.data?.title === 'Invalid current password') {
      // Show error to user
      setError('The current password you entered is incorrect');
    }
  }
};
```

### Issue 2: "403 Forbidden when trying to access admin endpoints"

**Cause:** User is not logged in as Admin.

**Solution:**
```javascript
// Check role before making admin requests
if (getUserRole() !== 'Admin') {
  alert('You need admin privileges to access this page');
  return;
}
```

### Issue 3: "401 Unauthorized on all requests"

**Cause:** Token is missing or expired.

**Solution:**
```javascript
// Check token before making requests
const token = localStorage.getItem('token');
const expiry = localStorage.getItem('tokenExpiry');

if (!token || new Date(expiry) < new Date()) {
  // Token missing or expired - redirect to login
  localStorage.clear();
  window.location.href = '/login';
}
```

### Issue 4: "Cannot update user - validation errors"

**Cause:** Missing required fields in update request.

**Solution:** The full update endpoint requires ALL fields:
```javascript
// Use the profile update endpoint instead for simple updates
await api.put('/users/me', {
  firstName: 'Ahmad',
  lastName: 'Hirzallah',
  email: 'ahmad@example.com'
});

// OR if using full update, provide all fields
await api.put(`/users/${userId}`, {
  username: user.username,
  password: newPassword, // Required!
  email: user.email,
  firstName: user.firstName,
  lastName: user.lastName,
  status: user.status,
  roleID: user.roleID,
  isDeleted: user.isDeleted
});
```

---

## ?? COMPLETE ENDPOINTS REFERENCE

### Authentication
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/users/login` | Login | ? |
| POST | `/api/users/register` | Register new user | ? |

### Users
| Method | Endpoint | Description | Auth Required | Admin Only |
|--------|----------|-------------|---------------|------------|
| GET | `/api/users` | Get all users | ? | ? |
| GET | `/api/users/{id}` | Get user by ID | ? | ? (own or admin) |
| GET | `/api/users/me` | Get current user | ? | ? |
| POST | `/api/users` | Create user | ? | ? |
| PUT | `/api/users/me` | Update profile | ? | ? |
| PUT | `/api/users/{id}` | Full user update | ? | ? (own or admin) |
| PUT | `/api/users/{id}/password` | Change password | ? | ? (own or admin) |
| DELETE | `/api/users/{id}` | Delete user | ? | ? (own or admin) |

### Phone Numbers
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/users/{userId}/phones` | Get all phones | ? |
| POST | `/api/users/{userId}/phones` | Add phone | ? |
| PUT | `/api/users/{userId}/phones/{phoneId}` | Update phone | ? |
| DELETE | `/api/users/{userId}/phones/{phoneId}` | Delete phone | ? |

### User Images
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/users/{userId}/images` | Get all images | ? |
| POST | `/api/users/{userId}/images/upload` | Upload image | ? |
| DELETE | `/api/users/{userId}/images/{imageId}` | Delete image | ? |

### Posts
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/posts` | Get all posts | ? |
| GET | `/api/posts/paged` | Get paginated posts | ? |
| GET | `/api/posts/{id}` | Get post by ID | ? |
| GET | `/api/posts/{id}/details` | Get post with details | ? |
| POST | `/api/posts` | Create post | ? |
| PUT | `/api/posts/{id}` | Update post | ? |
| DELETE | `/api/posts/{id}` | Delete post | ? |

### Post Images
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/posts/{postId}/images` | Get all images | ? |
| POST | `/api/posts/{postId}/images/upload` | Upload image | ? |
| DELETE | `/api/posts/{postId}/images/{imageId}` | Delete image | ? |

### Reviews
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/posts/{postId}/reviews` | Get all reviews | ? |
| POST | `/api/posts/{postId}/reviews` | Add review | ? |
| DELETE | `/api/posts/{postId}/reviews/{reviewId}` | Delete review | ? |

### Categories
| Method | Endpoint | Description | Auth Required | Admin Only |
|--------|----------|-------------|---------------|------------|
| GET | `/api/categories` | Get all categories | ? | ? |
| GET | `/api/categories/{id}` | Get by ID | ? | ? |
| POST | `/api/categories` | Create | ? | ? |
| PUT | `/api/categories/{id}` | Update | ? | ? |
| DELETE | `/api/categories/{id}` | Delete | ? | ? |

### Roles
| Method | Endpoint | Description | Auth Required | Admin Only |
|--------|----------|-------------|---------------|------------|
| GET | `/api/roles` | Get all roles | ? | ? |
| GET | `/api/roles/{id}` | Get by ID | ? | ? |
| POST | `/api/roles` | Create | ? | ? |
| PUT | `/api/roles/{id}` | Update | ? | ? |
| DELETE | `/api/roles/{id}` | Delete | ? | ? |

---

## ? TESTING CHECKLIST

### Authentication
- [ ] Login with valid credentials
- [ ] Login with invalid credentials (show error)
- [ ] Register new user
- [ ] Logout clears token
- [ ] Protected routes redirect to login when not authenticated

### Admin Dashboard
- [ ] Only accessible by Admin role
- [ ] Shows statistics on dashboard
- [ ] Users list loads and displays correctly
- [ ] Can create new user
- [ ] Can edit user
- [ ] Can delete/block user
- [ ] Posts list loads correctly
- [ ] Can create/edit/delete posts
- [ ] Categories CRUD works
- [ ] Reviews can be viewed and deleted

### User Profile
- [ ] Profile page loads with user data
- [ ] Can update profile (name, email)
- [ ] Can add phone number
- [ ] Can set phone as primary
- [ ] Can delete phone number
- [ ] Can upload profile image
- [ ] Can delete profile image
- [ ] Can change password with correct current password
- [ ] Shows error for wrong current password

---

## ?? QUICK START FOR FRONTEND TEAM

1. **Clone/Setup Project**
2. **Install Dependencies:**
   ```bash
   npm install axios react-router-dom
   ```
3. **Copy the API service code** from this document
4. **Implement login page first**
5. **Test authentication works**
6. **Build admin dashboard pages**
7. **Implement user profile features**
8. **Test all CRUD operations**

---

**Questions?** Check Swagger at `/swagger` or contact backend team.

**Last Updated:** December 2024
