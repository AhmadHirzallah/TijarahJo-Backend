# TijarahJo - User Profile Management Guide

## ?? Complete Frontend Guide for User Profile Features

This document provides complete details for implementing user profile functionality including:
- Profile viewing and editing
- Phone number management
- Profile image management
- Password changes

---

## ?? Authentication Header

**All authenticated endpoints require:**
```
Authorization: Bearer <your_jwt_token>
```

**Base URL:** `https://localhost:7064/api`

---

## ?? USER PROFILE

### 1. Get Current User Profile

**Use this to load the profile page**

```http
GET /api/users/me
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "userID": 5,
  "username": "ahmad_user",
  "email": "ahmad@example.com",
  "firstName": "Ahmad",
  "lastName": "Hirzallah",
  "primaryPhone": "+962791234567",
  "joinDate": "2024-01-15T10:30:00Z",
  "status": 0,
  "roleID": 2,
  "isDeleted": false,
  "fullName": "Ahmad Hirzallah"
}
```

### 2. Update Profile (Simple - No Password Required)

**Use this for the profile edit form**

```http
PUT /api/users/me
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "firstName": "Ahmad",
  "lastName": "Hirzallah",
  "email": "newemail@example.com",
  "username": "new_username"
}
```

**Response (200 OK):**
```json
{
  "userID": 5,
  "username": "new_username",
  "email": "newemail@example.com",
  "firstName": "Ahmad",
  "lastName": "Hirzallah",
  "primaryPhone": "+962791234567",
  "joinDate": "2024-01-15T10:30:00Z",
  "status": 0,
  "roleID": 2,
  "isDeleted": false,
  "fullName": "Ahmad Hirzallah"
}
```

**Field Validation:**
| Field | Required | Rules |
|-------|----------|-------|
| firstName | ? Yes | Max 100 characters |
| lastName | ? No | Max 100 characters |
| email | ? Yes | Valid email format |
| username | ? No | 3-50 characters (optional, keeps existing if not provided) |

---

## ?? PHONE NUMBERS

Each user can have **multiple phone numbers**. One can be marked as **primary**.

### 1. Get All Phone Numbers

```http
GET /api/users/{userId}/phones
Authorization: Bearer <token>
```

**Example:** `GET /api/users/5/phones`

**Response (200 OK):**
```json
{
  "userID": 5,
  "phoneNumbers": [
    {
      "phoneID": 1,
      "userID": 5,
      "phoneNumber": "+962791234567",
      "isPrimary": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "isDeleted": false
    },
    {
      "phoneID": 2,
      "userID": 5,
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

### 2. Add New Phone Number

```http
POST /api/users/{userId}/phones
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "phoneNumber": "+962791234567",
  "isPrimary": true
}
```

**Response (201 Created):**
```json
{
  "phoneID": 3,
  "userID": 5,
  "phoneNumber": "+962791234567",
  "isPrimary": true,
  "createdAt": "2024-12-25T10:30:00Z",
  "isDeleted": false
}
```

**Notes:**
- If `isPrimary: true`, all other phone numbers for this user become non-primary automatically
- Phone format should be valid (e.g., `+962791234567`)

### 3. Update Phone Number

```http
PUT /api/users/{userId}/phones/{phoneId}
Authorization: Bearer <token>
Content-Type: application/json
```

**Example:** `PUT /api/users/5/phones/3`

**Request Body:**
```json
{
  "phoneNumber": "+962791111111",
  "isPrimary": true,
  "isDeleted": false
}
```

### 4. Delete Phone Number

```http
DELETE /api/users/{userId}/phones/{phoneId}
Authorization: Bearer <token>
```

**Example:** `DELETE /api/users/5/phones/3`

**Response:** `204 No Content`

---

## ??? PROFILE IMAGES

### 1. Get All User Images

```http
GET /api/users/{userId}/images
```

**Response (200 OK):**
```json
{
  "userID": 5,
  "images": [
    {
      "userImageID": 1,
      "userID": 5,
      "imageURL": "/uploads/users/5/profile_20241225.jpg",
      "uploadedAt": "2024-12-25T10:30:00Z",
      "isDeleted": false
    }
  ],
  "totalCount": 1
}
```

### 2. Upload Profile Image (File Upload)

```http
POST /api/users/{userId}/images/upload
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Form Data:**
- `file`: The image file (JPEG, PNG, GIF, WebP)

**JavaScript Example:**
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);

const response = await fetch(`/api/users/${userId}/images/upload`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});
```

**Response (201 Created):**
```json
{
  "success": true,
  "image": {
    "userImageID": 2,
    "userID": 5,
    "imageURL": "/uploads/users/5/profile_new.jpg",
    "uploadedAt": "2024-12-25T12:00:00Z",
    "isDeleted": false
  },
  "fileSizeBytes": 245678
}
```

### 3. Upload Profile Image (Base64)

```http
POST /api/users/{userId}/images/upload-base64
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "imageData": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA..."
}
```

### 4. Delete Profile Image

```http
DELETE /api/users/{userId}/images/{imageId}
Authorization: Bearer <token>
```

**Response:** `204 No Content`

---

## ?? PASSWORD CHANGE

### Change Password

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

**Response:** `204 No Content` (Success)

**Error Response (400):**
```json
{
  "title": "Invalid current password",
  "status": 400
}
```

---

## ?? FRONTEND IMPLEMENTATION GUIDE

### Profile Edit Page Structure

```
???????????????????????????????????????????????????????????????
?                    PROFILE EDIT PAGE                        ?
???????????????????????????????????????????????????????????????
?                                                             ?
?  ???????????????                                            ?
?  ?   Profile   ?  [Upload New] [Delete]                     ?
?  ?   Image     ?                                            ?
?  ???????????????                                            ?
?                                                             ?
?  ??????????????? PERSONAL INFO ???????????????              ?
?                                                             ?
?  First Name: [_______________]                              ?
?  Last Name:  [_______________]                              ?
?  Email:      [_______________]                              ?
?  Username:   [_______________]                              ?
?                                                             ?
?                    [Save Changes]                           ?
?                                                             ?
?  ??????????????? PHONE NUMBERS ???????????????              ?
?                                                             ?
?  +962791234567  ? Primary  [Edit] [Delete]                 ?
?  +962799876543             [Edit] [Delete] [Make Primary]   ?
?                                                             ?
?  [+ Add New Phone Number]                                   ?
?                                                             ?
?  ??????????????? SECURITY ???????????????                   ?
?                                                             ?
?  [Change Password]                                          ?
?                                                             ?
???????????????????????????????????????????????????????????????
```

### React Example Implementation

```jsx
// ProfilePage.jsx
import { useState, useEffect } from 'react';
import api from './api'; // Your axios instance

function ProfilePage() {
  const [profile, setProfile] = useState(null);
  const [phones, setPhones] = useState([]);
  const [images, setImages] = useState([]);
  const [loading, setLoading] = useState(true);

  // Load profile data
  useEffect(() => {
    const loadProfile = async () => {
      try {
        const [profileRes, phonesRes, imagesRes] = await Promise.all([
          api.get('/users/me'),
          api.get(`/users/${userId}/phones`),
          api.get(`/users/${userId}/images`)
        ]);
        
        setProfile(profileRes.data);
        setPhones(phonesRes.data.phoneNumbers);
        setImages(imagesRes.data.images);
      } catch (error) {
        console.error('Failed to load profile:', error);
      } finally {
        setLoading(false);
      }
    };
    
    loadProfile();
  }, []);

  // Update profile
  const handleUpdateProfile = async (formData) => {
    try {
      const response = await api.put('/users/me', {
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        username: formData.username
      });
      setProfile(response.data);
      alert('Profile updated successfully!');
    } catch (error) {
      alert('Failed to update profile');
    }
  };

  // Add phone number
  const handleAddPhone = async (phoneNumber, isPrimary = false) => {
    try {
      const response = await api.post(`/users/${profile.userID}/phones`, {
        phoneNumber,
        isPrimary
      });
      setPhones([...phones, response.data]);
      
      // If set as primary, update other phones in state
      if (isPrimary) {
        setPhones(phones.map(p => ({ ...p, isPrimary: false })).concat(response.data));
      }
    } catch (error) {
      alert('Failed to add phone number');
    }
  };

  // Delete phone number
  const handleDeletePhone = async (phoneId) => {
    if (!confirm('Are you sure you want to delete this phone number?')) return;
    
    try {
      await api.delete(`/users/${profile.userID}/phones/${phoneId}`);
      setPhones(phones.filter(p => p.phoneID !== phoneId));
    } catch (error) {
      alert('Failed to delete phone number');
    }
  };

  // Upload image
  const handleUploadImage = async (file) => {
    const formData = new FormData();
    formData.append('file', file);
    
    try {
      const response = await api.post(
        `/users/${profile.userID}/images/upload`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
      );
      setImages([...images, response.data.image]);
    } catch (error) {
      alert('Failed to upload image');
    }
  };

  // Change password
  const handleChangePassword = async (currentPassword, newPassword) => {
    try {
      await api.put(`/users/${profile.userID}/password`, {
        currentPassword,
        newPassword
      });
      alert('Password changed successfully!');
    } catch (error) {
      if (error.response?.status === 400) {
        alert('Current password is incorrect');
      } else {
        alert('Failed to change password');
      }
    }
  };

  if (loading) return <div>Loading...</div>;

  return (
    <div className="profile-page">
      {/* Profile Form */}
      <ProfileForm profile={profile} onSave={handleUpdateProfile} />
      
      {/* Phone Numbers */}
      <PhoneList 
        phones={phones} 
        onAdd={handleAddPhone}
        onDelete={handleDeletePhone}
      />
      
      {/* Images */}
      <ImageGallery 
        images={images}
        onUpload={handleUploadImage}
      />
      
      {/* Password Change */}
      <PasswordChangeForm onSubmit={handleChangePassword} />
    </div>
  );
}
```

### API Service (Axios)

```javascript
// api.js
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7064/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' }
});

// Add token to all requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 errors
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

// API Functions
export const userApi = {
  // Profile
  getMyProfile: () => api.get('/users/me'),
  updateMyProfile: (data) => api.put('/users/me', data),
  
  // Phone Numbers
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
  
  // Password
  changePassword: (userId, data) => api.put(`/users/${userId}/password`, data)
};
```

---

## ?? COMPLETE ENDPOINTS SUMMARY

### User Profile
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/users/me` | Get current user profile | ? |
| PUT | `/api/users/me` | Update current user profile | ? |
| PUT | `/api/users/{id}/password` | Change password | ? |

### Phone Numbers
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/users/{userId}/phones` | Get all phones | ? |
| GET | `/api/users/{userId}/phones/{phoneId}` | Get specific phone | ? |
| POST | `/api/users/{userId}/phones` | Add new phone | ? |
| PUT | `/api/users/{userId}/phones/{phoneId}` | Update phone | ? |
| DELETE | `/api/users/{userId}/phones/{phoneId}` | Delete phone | ? |

### Images
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/users/{userId}/images` | Get all images | ? |
| GET | `/api/users/{userId}/images/{imageId}` | Get specific image | ? |
| POST | `/api/users/{userId}/images/upload` | Upload image (file) | ? |
| POST | `/api/users/{userId}/images/upload-base64` | Upload image (base64) | ? |
| DELETE | `/api/users/{userId}/images/{imageId}` | Delete image | ? |

---

## ?? Error Handling

### Common Error Responses

**400 Bad Request - Validation Error:**
```json
{
  "errors": {
    "Email": ["Invalid email format."],
    "FirstName": ["First name is required."]
  }
}
```

**401 Unauthorized:**
```json
{
  "title": "Unauthorized",
  "status": 401
}
```

**403 Forbidden (Not owner or admin):**
```json
{
  "title": "Forbidden",
  "status": 403
}
```

**404 Not Found:**
```json
{
  "title": "User Not Found",
  "status": 404
}
```

---

## ? Testing Checklist

- [ ] Load profile page with GET `/api/users/me`
- [ ] Update profile with PUT `/api/users/me`
- [ ] Load phone numbers with GET `/api/users/{id}/phones`
- [ ] Add new phone number with POST `/api/users/{id}/phones`
- [ ] Set phone as primary
- [ ] Delete phone number
- [ ] Upload profile image (file upload)
- [ ] Delete profile image
- [ ] Change password

---

**Questions?** Check Swagger documentation at `/swagger` when running the API.
