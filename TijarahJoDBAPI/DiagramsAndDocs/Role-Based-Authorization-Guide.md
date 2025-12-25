# TijarahJo - Role-Based Authorization Guide

## Overview

This document explains the JWT role-based authorization system implemented for TijarahJo.

---

## 1. JWT Token Structure

The JWT token now contains these claims:

```json
{
  "nameid": "123",           // User ID (ClaimTypes.NameIdentifier)
  "role": "Admin",           // User Role (ClaimTypes.Role)
  "unique_name": "john_doe", // Username (ClaimTypes.Name)
  "jti": "guid",             // Unique token ID
  "iat": 1234567890,         // Issued at timestamp
  "exp": 1234567890,         // Expiration timestamp
  "iss": "TijarahJoAPI",     // Issuer
  "aud": "TijarahJoClients"  // Audience
}
```

---

## 2. Role Hierarchy

| RoleID | Role Name | Description |
|--------|-----------|-------------|
| 1 | Admin | Full system access, can manage all resources |
| 2 | Moderator | Can moderate content, limited admin access |
| 3+ | User | Regular user, can only manage own resources |

---

## 3. Authorization Levels

### Public (No Auth Required)
- `POST /api/users/register` - User registration
- `POST /api/users/login` - User login

### Authenticated (Any Valid Token)
- `GET /api/users/me` - Get current user profile
- `GET /api/users/{id}` - Get user by ID (own profile or admin)
- `PUT /api/users/{id}` - Update user (own profile or admin)
- `DELETE /api/users/{id}` - Delete user (own account or admin)
- All Post/Image/Review endpoints (with ownership checks)

### Admin Only
- `GET /api/admin/dashboard` - Dashboard statistics
- `GET /api/admin/users` - List all users
- `PUT /api/admin/users/{id}/role` - Change user role
- `PUT /api/admin/users/{id}/status` - Ban/activate user
- `DELETE /api/admin/users/{id}/permanent` - Hard delete user
- `POST /api/admin/users/{id}/restore` - Restore deleted user
- `GET /api/admin/posts` - List all posts
- `PUT /api/admin/posts/{id}/status` - Approve/reject posts
- All category/role management

---

## 4. Login Response

```json
{
  "user": {
    "userID": 123,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "joinDate": "2025-01-20T00:00:00Z",
    "status": 0,
    "roleID": 1,
    "isDeleted": false,
    "fullName": "John Doe"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-20T01:00:00Z",
  "role": "Admin"
}
```

---

## 5. Frontend Authorization

### Decode Token (JavaScript)
```javascript
function decodeToken(token) {
  const payload = token.split('.')[1];
  return JSON.parse(atob(payload));
}

const claims = decodeToken(token);
const userId = claims.nameid;
const role = claims.role;
const isAdmin = role === 'Admin';
```

### Store & Use Token
```javascript
// After login
localStorage.setItem('token', response.token);
localStorage.setItem('role', response.role);
localStorage.setItem('userId', response.user.userID);

// On API calls
const headers = {
  'Authorization': `Bearer ${localStorage.getItem('token')}`,
  'Content-Type': 'application/json'
};

// Check admin access in UI
const isAdmin = localStorage.getItem('role') === 'Admin';
if (isAdmin) {
  showAdminPanel();
}
```

### Handle Token Expiration
```javascript
// Check response headers
if (response.headers.get('Token-Expired') === 'true') {
  // Token expired, redirect to login
  localStorage.clear();
  window.location.href = '/login';
}
```

---

## 6. Controller Usage

### Access User Claims
```csharp
// In any controller
using TijarahJoDBAPI.Extensions;

[Authorize]
public IActionResult MyEndpoint()
{
    // Get current user ID from token (no frontend input needed!)
    int userId = User.GetUserIdRequired();
    
    // Get role
    string? role = User.GetRole();
    
    // Check if admin
    bool isAdmin = User.IsAdmin();
    
    // Check ownership or admin
    if (!User.IsOwnerOrAdmin(resourceOwnerId))
        return Forbid();
}
```

### Apply Authorization
```csharp
// Require authentication
[Authorize]
public IActionResult AuthenticatedEndpoint() { }

// Require Admin role
[Authorize(Roles = "Admin")]
public IActionResult AdminOnlyEndpoint() { }

// Require Admin or Moderator
[Authorize(Roles = "Admin,Moderator")]
public IActionResult ModeratorEndpoint() { }

// Use policy
[Authorize(Policy = "AdminOnly")]
public IActionResult PolicyEndpoint() { }
```

---

## 7. Security Best Practices

### Why JWT Roles vs Database Flag?

| Approach | Pros | Cons |
|----------|------|------|
| **JWT Roles** | Fast (no DB hit), stateless, scalable | Can't revoke instantly, size limit |
| **DB Flag (IsAdmin)** | Real-time revocation, flexible | DB hit every request, slower |

**Recommendation**: Use JWT roles for most cases. For critical operations (e.g., payment), add a DB check.

### Implemented Security Measures

1. **Token Expiration** - Configurable lifetime (default 60 min)
2. **Unique Token ID** - Each token has `jti` claim
3. **No User ID from Frontend** - Always extract from token
4. **Ownership Validation** - Users can only access own resources
5. **Self-Protection** - Admins cannot demote/ban themselves
6. **Custom Error Responses** - Clear 401/403 messages

---

## 8. Admin Panel API Endpoints

### Dashboard
```
GET /api/admin/dashboard
? Returns user/post/category counts
```

### User Management
```
GET    /api/admin/users                    ? List all users
GET    /api/admin/users/{id}               ? Get user details
PUT    /api/admin/users/{id}/role          ? Change role
PUT    /api/admin/users/{id}/status        ? Ban/activate
DELETE /api/admin/users/{id}/permanent     ? Hard delete
POST   /api/admin/users/{id}/restore       ? Restore deleted
```

### Post Management
```
GET    /api/admin/posts                    ? List all posts
PUT    /api/admin/posts/{id}/status        ? Approve/reject
DELETE /api/admin/posts/{id}               ? Delete any post
```

### Other
```
GET /api/admin/categories   ? List categories
GET /api/admin/roles        ? List roles
```

---

## 9. Testing with Swagger

1. Login with admin credentials
2. Copy the token from response
3. Click "Authorize" button in Swagger
4. Paste token (without "Bearer " prefix)
5. All admin endpoints now accessible

---

## 10. Error Responses

### 401 Unauthorized
```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "You are not authorized to access this resource. Please provide a valid JWT token."
}
```

### 403 Forbidden
```json
{
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to access this resource."
}
```

---

## Files Created/Modified

| File | Purpose |
|------|---------|
| `TokenService.cs` | Updated to include role in JWT |
| `Extensions/ClaimsPrincipalExtensions.cs` | Helper methods for accessing claims |
| `Controllers/AdminController.cs` | Complete admin panel controller |
| `Controllers/UsersController.cs` | Added authorization + /me endpoint |
| `DTOs/Responses/AdminResponses.cs` | Admin-specific response DTOs |
| `DTOs/Requests/AdminRequests.cs` | Admin-specific request DTOs |
| `DTOs/Responses/LoginResponse.cs` | Added role + expiration |
| `Program.cs` | JWT events + authorization policies |
| `JwtOptions.cs` | Added required modifiers |
