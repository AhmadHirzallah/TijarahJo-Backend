# TijarahJo - Authentication & Login Frontend Guide

## ?? Overview

This guide documents the authentication endpoints, response formats, error handling, and provides guidance for implementing login/error pages in the frontend.

**Base URL:** `https://localhost:7064/api` (Development)  
**Production URL:** `https://your-domain.com/api`

---

## ?? Authentication Endpoints

### 1. Login

**Endpoint:** `POST /api/users/login`

**Authentication:** Not required (public endpoint)

#### Request Body

```json
{
  "login": "username_or_email",
  "password": "user_password"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `login` | string | ? Yes | Username or email address |
| `password` | string | ? Yes | User's password |

---

## ? Success Response (200 OK)

When login is successful:

```json
{
  "user": {
    "userID": 5,
    "username": "ahmad_user",
    "email": "ahmad@example.com",
    "firstName": "Ahmad",
    "lastName": "Ali",
    "joinDate": "2024-01-15T10:30:00Z",
    "status": 0,
    "statusName": "Active",
    "roleID": 2,
    "isDeleted": false,
    "primaryPhone": "+962791234567",
    "fullName": "Ahmad Ali"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-28T15:30:00Z",
  "role": "User"
}
```

### User Status Values

| Code | Name | Description |
|------|------|-------------|
| 0 | Active | Normal active account |
| 1 | Verified | Email/phone verified account |
| 2 | Banned | Account has been banned |

### Role Values

| RoleID | Role Name | Description |
|--------|-----------|-------------|
| 1 | Admin | Full system access |
| 2 | User | Regular user |
| 3 | Moderator | Limited admin access |

---

## ? Error Responses

### 1. Invalid Credentials (401 Unauthorized)

**When:** Username/email or password is incorrect

```json
{
  "title": "Invalid credentials",
  "detail": "The username/email or password is incorrect.",
  "status": 401
}
```

**Frontend Action:**
- Show error message on login form
- Keep user on login page
- Clear password field

---

### 2. Account Disabled/Deleted (401 Unauthorized)

**When:** User account has been soft-deleted or deactivated

```json
{
  "title": "Account disabled",
  "detail": "This account has been deactivated. Please contact support for assistance.",
  "status": 401
}
```

**Frontend Action:**
- Show specific disabled account message
- Provide contact support link
- Clear form fields

---

### 3. Account Banned (403 Forbidden)

**When:** User account has been banned (Status = 2)

```json
{
  "title": "Account Banned",
  "detail": "Your account has been banned. Please contact support for more information.",
  "status": 403
}
```

**Frontend Action:**
- Redirect to a dedicated "Account Banned" page OR show modal
- Provide contact/appeal information
- Do NOT allow retry

---

### 4. Validation Error (400 Bad Request)

**When:** Required fields are missing or invalid

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Login": ["Login is required."],
    "Password": ["Password is required."]
  }
}
```

**Frontend Action:**
- Display field-specific validation errors
- Highlight invalid fields

---

## ?? TypeScript Types

```typescript
// types/auth.ts

// User Status enum
export enum UserStatus {
  Active = 0,
  Verified = 1,
  Banned = 2
}

// Role enum
export enum UserRole {
  Admin = 1,
  User = 2,
  Moderator = 3
}

// Login request
export interface LoginRequest {
  login: string;  // username or email
  password: string;
}

// User response
export interface UserResponse {
  userID: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string | null;
  joinDate: string;
  status: UserStatus;
  statusName: string;  // "Active" | "Verified" | "Banned"
  roleID: UserRole;
  isDeleted: boolean;
  primaryPhone: string | null;
  fullName: string;
}

// Login success response
export interface LoginResponse {
  user: UserResponse;
  token: string;
  expiresAt: string;
  role: string;  // "Admin" | "User" | "Moderator"
}

// API Error response
export interface ApiError {
  title: string;
  detail?: string;
  status: number;
  errors?: Record<string, string[]>;
}

// Login error types for handling
export type LoginErrorType = 
  | 'invalid_credentials' 
  | 'account_disabled' 
  | 'account_banned' 
  | 'validation_error'
  | 'server_error';
```

---

## ?? Service Layer Implementation

```typescript
// services/authService.ts

import api from './api';
import { LoginRequest, LoginResponse, ApiError, LoginErrorType } from '../types/auth';

export interface LoginResult {
  success: boolean;
  data?: LoginResponse;
  errorType?: LoginErrorType;
  errorMessage?: string;
  fieldErrors?: Record<string, string[]>;
}

export const authService = {
  /**
   * Authenticate user and get JWT token
   */
  login: async (credentials: LoginRequest): Promise<LoginResult> => {
    try {
      const { data } = await api.post<LoginResponse>('/users/login', credentials);
      
      // Store token
      localStorage.setItem('auth_token', data.token);
      localStorage.setItem('user', JSON.stringify(data.user));
      localStorage.setItem('token_expires', data.expiresAt);
      
      return {
        success: true,
        data
      };
    } catch (error: any) {
      return handleLoginError(error);
    }
  },

  /**
   * Logout user
   */
  logout: () => {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user');
    localStorage.removeItem('token_expires');
  },

  /**
   * Check if user is logged in
   */
  isAuthenticated: (): boolean => {
    const token = localStorage.getItem('auth_token');
    const expires = localStorage.getItem('token_expires');
    
    if (!token || !expires) return false;
    
    return new Date(expires) > new Date();
  },

  /**
   * Get current user from storage
   */
  getCurrentUser: (): UserResponse | null => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
};

/**
 * Parse login error and return structured result
 */
function handleLoginError(error: any): LoginResult {
  const status = error.response?.status;
  const data = error.response?.data as ApiError;

  switch (status) {
    case 400:
      // Validation error
      return {
        success: false,
        errorType: 'validation_error',
        errorMessage: data.title || 'Please check your input',
        fieldErrors: data.errors
      };

    case 401:
      // Check if it's disabled account or invalid credentials
      if (data.title === 'Account disabled') {
        return {
          success: false,
          errorType: 'account_disabled',
          errorMessage: data.detail || 'This account has been deactivated.'
        };
      }
      return {
        success: false,
        errorType: 'invalid_credentials',
        errorMessage: data.detail || 'Invalid username or password.'
      };

    case 403:
      // Account banned
      return {
        success: false,
        errorType: 'account_banned',
        errorMessage: data.detail || 'Your account has been banned.'
      };

    default:
      // Server error or network error
      return {
        success: false,
        errorType: 'server_error',
        errorMessage: 'An unexpected error occurred. Please try again later.'
      };
  }
}

export default authService;
```

---

## ?? React Login Page Component

```tsx
// pages/Login.tsx

import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Eye, EyeOff, Mail, Lock, AlertCircle, Ban } from 'lucide-react';
import authService, { LoginResult } from '../services/authService';
import { LoginErrorType } from '../types/auth';

const Login: React.FC = () => {
  const navigate = useNavigate();
  
  // Form state
  const [login, setLogin] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  
  // Error state
  const [errorType, setErrorType] = useState<LoginErrorType | null>(null);
  const [errorMessage, setErrorMessage] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Clear previous errors
    setErrorType(null);
    setErrorMessage('');
    setFieldErrors({});
    
    setIsLoading(true);

    const result: LoginResult = await authService.login({ login, password });

    setIsLoading(false);

    if (result.success) {
      // Redirect based on role
      const role = result.data!.role;
      if (role === 'Admin') {
        navigate('/admin/dashboard');
      } else {
        navigate('/dashboard');
      }
    } else {
      setErrorType(result.errorType!);
      setErrorMessage(result.errorMessage!);
      
      if (result.fieldErrors) {
        setFieldErrors(result.fieldErrors);
      }

      // Handle banned account - redirect to banned page
      if (result.errorType === 'account_banned') {
        navigate('/account-banned');
      }
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4">
      <div className="max-w-md w-full space-y-8">
        {/* Header */}
        <div className="text-center">
          <h2 className="text-3xl font-bold text-gray-900">Welcome Back</h2>
          <p className="mt-2 text-gray-600">Sign in to your TijarahJo account</p>
        </div>

        {/* Error Alert */}
        {errorType && errorType !== 'validation_error' && (
          <ErrorAlert type={errorType} message={errorMessage} />
        )}

        {/* Login Form */}
        <form onSubmit={handleSubmit} className="mt-8 space-y-6">
          {/* Email/Username Field */}
          <div>
            <label htmlFor="login" className="block text-sm font-medium text-gray-700">
              Email or Username
            </label>
            <div className="mt-1 relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Mail className="h-5 w-5 text-gray-400" />
              </div>
              <input
                id="login"
                name="login"
                type="text"
                required
                value={login}
                onChange={(e) => setLogin(e.target.value)}
                className={`block w-full pl-10 pr-3 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  fieldErrors.Login ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="you@example.com"
              />
            </div>
            {fieldErrors.Login && (
              <p className="mt-1 text-sm text-red-600">{fieldErrors.Login[0]}</p>
            )}
          </div>

          {/* Password Field */}
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700">
              Password
            </label>
            <div className="mt-1 relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Lock className="h-5 w-5 text-gray-400" />
              </div>
              <input
                id="password"
                name="password"
                type={showPassword ? 'text' : 'password'}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={`block w-full pl-10 pr-10 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  fieldErrors.Password ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="••••••••"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute inset-y-0 right-0 pr-3 flex items-center"
              >
                {showPassword ? (
                  <EyeOff className="h-5 w-5 text-gray-400" />
                ) : (
                  <Eye className="h-5 w-5 text-gray-400" />
                )}
              </button>
            </div>
            {fieldErrors.Password && (
              <p className="mt-1 text-sm text-red-600">{fieldErrors.Password[0]}</p>
            )}
          </div>

          {/* Remember Me & Forgot Password */}
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <input
                id="remember-me"
                name="remember-me"
                type="checkbox"
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <label htmlFor="remember-me" className="ml-2 block text-sm text-gray-700">
                Remember me
              </label>
            </div>
            <Link to="/forgot-password" className="text-sm font-medium text-blue-600 hover:text-blue-500">
              Forgot password?
            </Link>
          </div>

          {/* Submit Button */}
          <button
            type="submit"
            disabled={isLoading}
            className="w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? (
              <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
            ) : (
              'Sign In'
            )}
          </button>

          {/* Register Link */}
          <p className="text-center text-sm text-gray-600">
            Don't have an account?{' '}
            <Link to="/register" className="font-medium text-blue-600 hover:text-blue-500">
              Create one
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
};

// Error Alert Component
interface ErrorAlertProps {
  type: LoginErrorType;
  message: string;
}

const ErrorAlert: React.FC<ErrorAlertProps> = ({ type, message }) => {
  const config: Record<LoginErrorType, { bg: string; border: string; text: string; icon: React.ReactNode }> = {
    invalid_credentials: {
      bg: 'bg-red-50',
      border: 'border-red-400',
      text: 'text-red-700',
      icon: <AlertCircle className="h-5 w-5 text-red-400" />
    },
    account_disabled: {
      bg: 'bg-yellow-50',
      border: 'border-yellow-400',
      text: 'text-yellow-700',
      icon: <AlertCircle className="h-5 w-5 text-yellow-400" />
    },
    account_banned: {
      bg: 'bg-red-50',
      border: 'border-red-500',
      text: 'text-red-800',
      icon: <Ban className="h-5 w-5 text-red-500" />
    },
    validation_error: {
      bg: 'bg-orange-50',
      border: 'border-orange-400',
      text: 'text-orange-700',
      icon: <AlertCircle className="h-5 w-5 text-orange-400" />
    },
    server_error: {
      bg: 'bg-gray-50',
      border: 'border-gray-400',
      text: 'text-gray-700',
      icon: <AlertCircle className="h-5 w-5 text-gray-400" />
    }
  };

  const { bg, border, text, icon } = config[type];

  return (
    <div className={`${bg} border-l-4 ${border} p-4 rounded-r-lg`}>
      <div className="flex items-start">
        <div className="flex-shrink-0">{icon}</div>
        <div className="ml-3">
          <p className={`text-sm ${text}`}>{message}</p>
          {type === 'account_disabled' && (
            <p className={`mt-2 text-sm ${text}`}>
              <a href="mailto:support@tijarahjo.com" className="font-medium underline">
                Contact Support
              </a>
            </p>
          )}
        </div>
      </div>
    </div>
  );
};

export default Login;
```

---

## ?? Account Banned Page

Create a dedicated page for banned users:

```tsx
// pages/AccountBanned.tsx

import React from 'react';
import { Ban, Mail, Phone, ArrowLeft } from 'lucide-react';
import { Link } from 'react-router-dom';

const AccountBanned: React.FC = () => {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-red-50 to-red-100 py-12 px-4">
      <div className="max-w-lg w-full text-center">
        {/* Icon */}
        <div className="mx-auto w-24 h-24 bg-red-100 rounded-full flex items-center justify-center mb-8">
          <Ban className="h-12 w-12 text-red-600" />
        </div>

        {/* Title */}
        <h1 className="text-3xl font-bold text-gray-900 mb-4">
          Account Suspended
        </h1>

        {/* Message */}
        <p className="text-lg text-gray-600 mb-8">
          Your account has been suspended due to a violation of our 
          <Link to="/terms" className="text-blue-600 hover:underline mx-1">
            Terms of Service
          </Link>
          or
          <Link to="/community-guidelines" className="text-blue-600 hover:underline mx-1">
            Community Guidelines
          </Link>.
        </p>

        {/* Info Box */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-8 text-left">
          <h3 className="font-semibold text-gray-900 mb-4">What can you do?</h3>
          <ul className="space-y-3 text-gray-600">
            <li className="flex items-start gap-3">
              <span className="flex-shrink-0 w-6 h-6 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-sm font-medium">1</span>
              <span>Review our Terms of Service to understand the violation</span>
            </li>
            <li className="flex items-start gap-3">
              <span className="flex-shrink-0 w-6 h-6 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-sm font-medium">2</span>
              <span>Contact our support team to appeal this decision</span>
            </li>
            <li className="flex items-start gap-3">
              <span className="flex-shrink-0 w-6 h-6 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-sm font-medium">3</span>
              <span>Provide any relevant information that may help your case</span>
            </li>
          </ul>
        </div>

        {/* Contact Options */}
        <div className="bg-gray-50 rounded-xl p-6 mb-8">
          <h3 className="font-semibold text-gray-900 mb-4">Contact Support</h3>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <a
              href="mailto:support@tijarahjo.com?subject=Account%20Ban%20Appeal"
              className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
            >
              <Mail className="h-5 w-5" />
              Email Support
            </a>
            <a
              href="tel:+962791234567"
              className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition"
            >
              <Phone className="h-5 w-5" />
              Call Us
            </a>
          </div>
        </div>

        {/* Back Link */}
        <Link
          to="/login"
          className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 transition"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Login
        </Link>
      </div>
    </div>
  );
};

export default AccountBanned;
```

---

## ?? Account Disabled Page

For deactivated/deleted accounts:

```tsx
// pages/AccountDisabled.tsx

import React from 'react';
import { UserX, Mail, ArrowLeft, HelpCircle } from 'lucide-react';
import { Link } from 'react-router-dom';

const AccountDisabled: React.FC = () => {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-yellow-50 to-orange-50 py-12 px-4">
      <div className="max-w-lg w-full text-center">
        {/* Icon */}
        <div className="mx-auto w-24 h-24 bg-yellow-100 rounded-full flex items-center justify-center mb-8">
          <UserX className="h-12 w-12 text-yellow-600" />
        </div>

        {/* Title */}
        <h1 className="text-3xl font-bold text-gray-900 mb-4">
          Account Deactivated
        </h1>

        {/* Message */}
        <p className="text-lg text-gray-600 mb-8">
          This account has been deactivated. This may have happened because:
        </p>

        {/* Reasons */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-8 text-left">
          <ul className="space-y-3 text-gray-600">
            <li className="flex items-start gap-3">
              <HelpCircle className="h-5 w-5 text-yellow-500 flex-shrink-0 mt-0.5" />
              <span>You requested account deletion</span>
            </li>
            <li className="flex items-start gap-3">
              <HelpCircle className="h-5 w-5 text-yellow-500 flex-shrink-0 mt-0.5" />
              <span>The account was inactive for an extended period</span>
            </li>
            <li className="flex items-start gap-3">
              <HelpCircle className="h-5 w-5 text-yellow-500 flex-shrink-0 mt-0.5" />
              <span>An administrator deactivated the account</span>
            </li>
          </ul>
        </div>

        {/* CTA */}
        <div className="space-y-4">
          <a
            href="mailto:support@tijarahjo.com?subject=Account%20Reactivation%20Request"
            className="inline-flex items-center justify-center gap-2 px-8 py-3 bg-yellow-500 text-white rounded-lg hover:bg-yellow-600 transition w-full sm:w-auto"
          >
            <Mail className="h-5 w-5" />
            Request Reactivation
          </a>
          
          <div>
            <Link
              to="/register"
              className="text-blue-600 hover:underline"
            >
              Or create a new account
            </Link>
          </div>
        </div>

        {/* Back Link */}
        <div className="mt-8">
          <Link
            to="/login"
            className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 transition"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Login
          </Link>
        </div>
      </div>
    </div>
  );
};

export default AccountDisabled;
```

---

## ??? Router Setup

Add routes for error pages:

```tsx
// App.tsx or routes.tsx

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import AccountBanned from './pages/AccountBanned';
import AccountDisabled from './pages/AccountDisabled';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import ProtectedRoute from './components/ProtectedRoute';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/account-banned" element={<AccountBanned />} />
        <Route path="/account-disabled" element={<AccountDisabled />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />
        
        {/* Protected Routes */}
        <Route element={<ProtectedRoute />}>
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/my-posts" element={<MyPosts />} />
          {/* ... other protected routes */}
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
```

---

## ?? Protected Route Component

```tsx
// components/ProtectedRoute.tsx

import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import authService from '../services/authService';

const ProtectedRoute: React.FC = () => {
  const location = useLocation();
  
  if (!authService.isAuthenticated()) {
    // Redirect to login, preserving the attempted URL
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
```

---

## ?? Error Handling Summary

| HTTP Status | Error Type | User Message | Action |
|-------------|------------|--------------|--------|
| 400 | Validation Error | Field-specific errors | Show inline errors |
| 401 | Invalid Credentials | "Invalid username or password" | Show alert, stay on page |
| 401 | Account Disabled | "Account deactivated" | Show alert or redirect |
| 403 | Account Banned | "Account banned" | Redirect to banned page |
| 500 | Server Error | "Unexpected error" | Show alert, allow retry |

---

## ?? Testing Checklist

### Login Page
- [ ] Valid login redirects to dashboard
- [ ] Invalid credentials shows error message
- [ ] Empty fields show validation errors
- [ ] Password visibility toggle works
- [ ] Loading state during API call
- [ ] Remember me functionality

### Account Banned Page
- [ ] Shows when 403 returned
- [ ] Has contact support options
- [ ] Back to login link works
- [ ] Professional, non-accusatory design

### Account Disabled Page
- [ ] Shows when account is deactivated
- [ ] Has reactivation request option
- [ ] Link to create new account
- [ ] Back to login link works

---

## ?? Design Guidelines

### Colors

| State | Background | Border | Text |
|-------|------------|--------|------|
| Error (Invalid) | `bg-red-50` | `border-red-400` | `text-red-700` |
| Warning (Disabled) | `bg-yellow-50` | `border-yellow-400` | `text-yellow-700` |
| Banned | `bg-red-50` | `border-red-500` | `text-red-800` |
| Info | `bg-blue-50` | `border-blue-400` | `text-blue-700` |

### Icons (Lucide React)

```tsx
import { 
  AlertCircle,  // General error
  Ban,          // Banned
  UserX,        // Disabled
  Lock,         // Password
  Mail,         // Email
  Eye,          // Show password
  EyeOff        // Hide password
} from 'lucide-react';
```

---

## ?? Quick Reference

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/users/login` | Authenticate user |
| POST | `/users/register` | Create new account |
| GET | `/users/me` | Get current user |
| PUT | `/users/me` | Update profile |
| PUT | `/users/{id}/password` | Change password |

### Response Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | Success | Login successful |
| 201 | Created | Registration successful |
| 400 | Bad Request | Validation error |
| 401 | Unauthorized | Invalid credentials / Disabled |
| 403 | Forbidden | Account banned |
| 500 | Server Error | Unexpected error |

---

*Last Updated: December 2024*  
*API Version: 1.0*
