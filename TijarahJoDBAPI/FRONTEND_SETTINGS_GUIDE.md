# TijarahJo - System Settings API Guide

## ?? Overview

This guide documents the System Settings API for managing platform configuration, specifically support contact information (email and WhatsApp number) from the admin dashboard.

**Base URL:** `https://localhost:7064/api`

---

## ?? Use Cases

1. **Admin Dashboard**: Configure support email and WhatsApp number
2. **Banned User Page**: Show support contact for appeals
3. **Footer/Contact Page**: Display support contact info
4. **Help Center**: Provide contact options

---

## ?? Database Schema

```sql
CREATE TABLE [dbo].[TbSystemSettings]
(
    [SettingID]       INT IDENTITY(1,1) PRIMARY KEY,
    [SettingKey]      NVARCHAR(100) NOT NULL UNIQUE,
    [SettingValue]    NVARCHAR(500) NULL,
    [SettingGroup]    NVARCHAR(50) NOT NULL DEFAULT 'General',
    [Description]     NVARCHAR(255) NULL,
    [DataType]        NVARCHAR(20) NOT NULL DEFAULT 'String',
    [IsPublic]        BIT NOT NULL DEFAULT 0,
    [CreatedAt]       DATETIME2 NOT NULL,
    [UpdatedAt]       DATETIME2 NOT NULL,
    [UpdatedByUserID] INT NULL
);
```

### Default Settings

| Key | Default Value | Group | Public |
|-----|---------------|-------|--------|
| `SupportEmail` | support@tijarahjo.com | Support | ? Yes |
| `SupportWhatsApp` | 962791234567 | Support | ? Yes |
| `SiteName` | TijarahJo | General | ? Yes |
| `MaintenanceMode` | false | General | ? Yes |
| `MaxUploadSizeMB` | 5 | Upload | ? Yes |

---

## ?? API Endpoints

### Public Endpoints (No Auth Required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/settings/support` | Get support contact info |
| GET | `/api/settings/public` | Get all public settings |
| GET | `/api/settings/public/{key}` | Get a specific public setting |

### Admin Endpoints (Admin Auth Required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/settings` | Get all settings |
| GET | `/api/settings/grouped` | Get settings grouped by category |
| GET | `/api/settings/group/{name}` | Get settings by group |
| GET | `/api/settings/{key}` | Get a specific setting |
| PUT | `/api/settings/support` | Update support contact info |
| PUT | `/api/settings/{key}` | Update a single setting |
| PUT | `/api/settings/batch` | Update multiple settings |
| POST | `/api/settings` | Create a new setting |
| DELETE | `/api/settings/{key}` | Delete a setting |

---

## ?? Public Endpoints

### 1. Get Support Contact Info

**Endpoint:** `GET /api/settings/support`

**Use this on:** Banned page, footer, contact page, help center

**Response (200 OK):**
```json
{
  "supportEmail": "support@tijarahjo.com",
  "supportWhatsApp": "962791234567",
  "whatsAppLink": "https://wa.me/962791234567",
  "emailLink": "mailto:support@tijarahjo.com",
  "isConfigured": true,
  "whatsAppSupportLink": "https://wa.me/962791234567?text=Hi%2C%20I%20need%20help%20with%20TijarahJo."
}
```

### 2. Get All Public Settings

**Endpoint:** `GET /api/settings/public`

**Response (200 OK):**
```json
{
  "settings": [
    {
      "settingID": 1,
      "settingKey": "SupportEmail",
      "settingValue": "support@tijarahjo.com",
      "settingGroup": "Support",
      "description": "Support email address displayed to users",
      "dataType": "String",
      "isPublic": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-12-25T10:30:00Z",
      "updatedByUserID": 1
    }
  ],
  "totalCount": 5,
  "groupCounts": {
    "Support": 2,
    "General": 2,
    "Upload": 1
  }
}
```

---

## ?? Admin Endpoints

### 1. Get All Settings

**Endpoint:** `GET /api/settings`

**Headers:**
```
Authorization: Bearer <admin_token>
```

**Response (200 OK):**
```json
{
  "settings": [
    {
      "settingID": 1,
      "settingKey": "SupportEmail",
      "settingValue": "support@tijarahjo.com",
      "settingGroup": "Support",
      "description": "Support email address displayed to users",
      "dataType": "String",
      "isPublic": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-12-25T10:30:00Z",
      "updatedByUserID": 1
    },
    {
      "settingID": 2,
      "settingKey": "SupportWhatsApp",
      "settingValue": "962791234567",
      "settingGroup": "Support",
      "description": "Support WhatsApp number (Jordan format)",
      "dataType": "String",
      "isPublic": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-12-25T10:30:00Z",
      "updatedByUserID": 1
    }
  ],
  "totalCount": 5,
  "groupCounts": {
    "Support": 2,
    "General": 2,
    "Upload": 1
  }
}
```

### 2. Update Support Contact Settings

**Endpoint:** `PUT /api/settings/support`

**Headers:**
```
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "supportEmail": "help@tijarahjo.com",
  "supportWhatsApp": "962791234567"
}
```

**Validation Rules:**
| Field | Required | Rules |
|-------|----------|-------|
| `supportEmail` | ? Yes | Valid email format, max 200 chars |
| `supportWhatsApp` | ? Yes | Jordan format: `9627[7/8/9]XXXXXXX` (12 digits) |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Support contact settings updated successfully.",
  "updatedAt": "2024-12-25T10:30:00Z",
  "updatedCount": 2
}
```

**Error Response (400 Bad Request):**
```json
{
  "errors": {
    "SupportEmail": ["Invalid email format."],
    "SupportWhatsApp": ["WhatsApp number must be in Jordan format: 9627[7/8/9]XXXXXXX (12 digits)"]
  }
}
```

### 3. Update Single Setting

**Endpoint:** `PUT /api/settings/{key}`

**Example:** `PUT /api/settings/SiteName`

**Request Body:**
```json
{
  "settingKey": "SiteName",
  "settingValue": "TijarahJo Marketplace"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Setting 'SiteName' updated successfully.",
  "updatedAt": "2024-12-25T10:30:00Z",
  "updatedCount": 1
}
```

### 4. Update Multiple Settings (Batch)

**Endpoint:** `PUT /api/settings/batch`

**Request Body:**
```json
{
  "settings": [
    { "settingKey": "SupportEmail", "settingValue": "support@tijarahjo.com" },
    { "settingKey": "SupportWhatsApp", "settingValue": "962791234567" },
    { "settingKey": "MaintenanceMode", "settingValue": "false" }
  ]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Successfully updated 3 settings.",
  "updatedAt": "2024-12-25T10:30:00Z",
  "updatedCount": 3
}
```

---

## ?? TypeScript Types

```typescript
// types/settings.ts

export interface SettingResponse {
  settingID: number;
  settingKey: string;
  settingValue: string | null;
  settingGroup: string;
  description: string | null;
  dataType: 'String' | 'Int' | 'Bool' | 'Json';
  isPublic: boolean;
  createdAt: string;
  updatedAt: string;
  updatedByUserID: number | null;
}

export interface AllSettingsResponse {
  settings: SettingResponse[];
  totalCount: number;
  groupCounts: Record<string, number>;
}

export interface SettingsGroupResponse {
  groupName: string;
  settings: SettingResponse[];
  count: number;
}

export interface SupportContactResponse {
  supportEmail: string | null;
  supportWhatsApp: string | null;
  whatsAppLink: string | null;
  emailLink: string | null;
  isConfigured: boolean;
  whatsAppSupportLink: string | null;
}

export interface SettingsUpdateResponse {
  success: boolean;
  message: string;
  updatedAt: string;
  updatedCount: number;
}

// Request types
export interface UpdateSupportContactRequest {
  supportEmail: string;
  supportWhatsApp: string;
}

export interface UpdateSettingRequest {
  settingKey: string;
  settingValue: string | null;
}

export interface UpdateSettingsBatchRequest {
  settings: UpdateSettingRequest[];
}
```

---

## ?? Service Layer

```typescript
// services/settingsService.ts

import api from './api';
import {
  AllSettingsResponse,
  SupportContactResponse,
  SettingsUpdateResponse,
  UpdateSupportContactRequest,
  SettingResponse
} from '../types/settings';

export const settingsService = {
  // ==================== PUBLIC ====================
  
  /**
   * Get support contact info (no auth required)
   * Use on: banned page, footer, contact page
   */
  getSupportContact: async (): Promise<SupportContactResponse> => {
    const { data } = await api.get<SupportContactResponse>('/settings/support');
    return data;
  },

  /**
   * Get all public settings (no auth required)
   */
  getPublicSettings: async (): Promise<AllSettingsResponse> => {
    const { data } = await api.get<AllSettingsResponse>('/settings/public');
    return data;
  },

  // ==================== ADMIN ====================
  
  /**
   * Get all settings (admin only)
   */
  getAllSettings: async (): Promise<AllSettingsResponse> => {
    const { data } = await api.get<AllSettingsResponse>('/settings');
    return data;
  },

  /**
   * Get settings grouped by category (admin only)
   */
  getSettingsGrouped: async (): Promise<SettingsGroupResponse[]> => {
    const { data } = await api.get<SettingsGroupResponse[]>('/settings/grouped');
    return data;
  },

  /**
   * Get a specific setting (admin only)
   */
  getSetting: async (key: string): Promise<SettingResponse> => {
    const { data } = await api.get<SettingResponse>(`/settings/${key}`);
    return data;
  },

  /**
   * Update support contact settings (admin only)
   */
  updateSupportContact: async (
    request: UpdateSupportContactRequest
  ): Promise<SettingsUpdateResponse> => {
    const { data } = await api.put<SettingsUpdateResponse>('/settings/support', request);
    return data;
  },

  /**
   * Update a single setting (admin only)
   */
  updateSetting: async (
    key: string,
    value: string | null
  ): Promise<SettingsUpdateResponse> => {
    const { data } = await api.put<SettingsUpdateResponse>(`/settings/${key}`, {
      settingKey: key,
      settingValue: value
    });
    return data;
  },

  /**
   * Update multiple settings (admin only)
   */
  updateSettingsBatch: async (
    settings: { settingKey: string; settingValue: string | null }[]
  ): Promise<SettingsUpdateResponse> => {
    const { data } = await api.put<SettingsUpdateResponse>('/settings/batch', {
      settings
    });
    return data;
  }
};

export default settingsService;
```

---

## ?? React Components

### 1. Admin Settings Page

```tsx
// pages/admin/SettingsPage.tsx

import React, { useState, useEffect } from 'react';
import { Save, RefreshCw, CheckCircle, AlertCircle, Mail, Phone } from 'lucide-react';
import settingsService from '../../services/settingsService';
import { SupportContactResponse, UpdateSupportContactRequest } from '../../types/settings';
import toast from 'react-hot-toast';

const SettingsPage: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [supportContact, setSupportContact] = useState<SupportContactResponse | null>(null);
  
  // Form state
  const [supportEmail, setSupportEmail] = useState('');
  const [supportWhatsApp, setSupportWhatsApp] = useState('');
  
  // Validation state
  const [errors, setErrors] = useState<{ email?: string; whatsApp?: string }>({});
  const [isDirty, setIsDirty] = useState(false);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const data = await settingsService.getSupportContact();
      setSupportContact(data);
      setSupportEmail(data.supportEmail || '');
      setSupportWhatsApp(data.supportWhatsApp || '');
      setIsDirty(false);
    } catch (error) {
      toast.error('Failed to load settings');
    } finally {
      setLoading(false);
    }
  };

  const validateForm = (): boolean => {
    const newErrors: { email?: string; whatsApp?: string } = {};

    // Email validation
    if (!supportEmail) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(supportEmail)) {
      newErrors.email = 'Invalid email format';
    }

    // WhatsApp validation (Jordan format: 9627[789]XXXXXXX)
    if (!supportWhatsApp) {
      newErrors.whatsApp = 'WhatsApp number is required';
    } else if (!/^9627[789]\d{7}$/.test(supportWhatsApp)) {
      newErrors.whatsApp = 'Must be Jordan format: 9627[7/8/9]XXXXXXX (12 digits)';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validateForm()) return;

    try {
      setSaving(true);
      
      const request: UpdateSupportContactRequest = {
        supportEmail,
        supportWhatsApp
      };

      const result = await settingsService.updateSupportContact(request);

      if (result.success) {
        toast.success('Settings saved successfully!');
        setIsDirty(false);
        await loadSettings(); // Refresh to show updated values
      }
    } catch (error: any) {
      const errorMsg = error.response?.data?.detail || 'Failed to save settings';
      toast.error(errorMsg);
      
      // Handle validation errors from API
      if (error.response?.data?.errors) {
        const apiErrors = error.response.data.errors;
        setErrors({
          email: apiErrors.SupportEmail?.[0],
          whatsApp: apiErrors.SupportWhatsApp?.[0]
        });
      }
    } finally {
      setSaving(false);
    }
  };

  const handleEmailChange = (value: string) => {
    setSupportEmail(value);
    setIsDirty(true);
    if (errors.email) setErrors(prev => ({ ...prev, email: undefined }));
  };

  const handleWhatsAppChange = (value: string) => {
    // Only allow digits
    const cleaned = value.replace(/\D/g, '');
    setSupportWhatsApp(cleaned);
    setIsDirty(true);
    if (errors.whatsApp) setErrors(prev => ({ ...prev, whatsApp: undefined }));
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
      </div>
    );
  }

  return (
    <div className="p-6 max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Settings</h1>
          <p className="text-gray-500 mt-1">Manage platform configuration</p>
        </div>
        <div className="flex items-center gap-2">
          {supportContact?.isConfigured && (
            <span className="flex items-center gap-1 text-sm text-green-600">
              <CheckCircle size={16} />
              System Operational
            </span>
          )}
          <button
            onClick={loadSettings}
            className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg"
            title="Refresh"
          >
            <RefreshCw size={20} />
          </button>
        </div>
      </div>

      {/* Support Contact Card */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-2">
          Support Contact Configuration
        </h2>
        <p className="text-sm text-gray-500 mb-6">
          Configure the support contact information displayed to users throughout the platform
        </p>

        <div className="space-y-6">
          {/* Support Email */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Support Email Address
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Mail size={18} className="text-gray-400" />
              </div>
              <input
                type="email"
                value={supportEmail}
                onChange={(e) => handleEmailChange(e.target.value)}
                placeholder="support@tijarahjo.com"
                className={`block w-full pl-10 pr-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  errors.email ? 'border-red-500' : 'border-gray-300'
                }`}
              />
            </div>
            {errors.email && (
              <p className="mt-1 text-sm text-red-600 flex items-center gap-1">
                <AlertCircle size={14} />
                {errors.email}
              </p>
            )}
            <p className="mt-1 text-xs text-gray-500">
              This email will be displayed on ban appeal pages and support links
            </p>
          </div>

          {/* Support WhatsApp */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              WhatsApp Number
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Phone size={18} className="text-gray-400" />
              </div>
              <input
                type="text"
                value={supportWhatsApp}
                onChange={(e) => handleWhatsAppChange(e.target.value)}
                placeholder="962791234567"
                maxLength={12}
                className={`block w-full pl-10 pr-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  errors.whatsApp ? 'border-red-500' : 'border-gray-300'
                }`}
              />
            </div>
            {errors.whatsApp && (
              <p className="mt-1 text-sm text-red-600 flex items-center gap-1">
                <AlertCircle size={14} />
                {errors.whatsApp}
              </p>
            )}
            <p className="mt-1 text-xs text-gray-500">
              Format: 9627[7/8/9]XXXXXXX - Jordan mobile only (12 digits)
            </p>
          </div>
        </div>

        {/* Save Button */}
        <div className="mt-8 flex items-center justify-between">
          <button
            onClick={handleSave}
            disabled={saving || !isDirty}
            className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            {saving ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white" />
                Saving...
              </>
            ) : (
              <>
                <Save size={18} />
                Save Support Settings
              </>
            )}
          </button>

          {isDirty && (
            <span className="text-sm text-amber-600">
              You have unsaved changes
            </span>
          )}
        </div>
      </div>

      {/* Preview Card */}
      <div className="bg-gray-50 rounded-xl border border-gray-200 p-6">
        <h3 className="text-sm font-medium text-gray-700 mb-4">Preview</h3>
        <p className="text-sm text-gray-500 mb-4">
          Users will see these contact options:
        </p>
        
        <div className="flex flex-col sm:flex-row gap-3">
          {supportEmail && (
            <a
              href={`mailto:${supportEmail}`}
              className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg text-gray-700 hover:bg-gray-50"
            >
              <Mail size={18} className="text-gray-400" />
              {supportEmail}
            </a>
          )}
          
          {supportWhatsApp && (
            <a
              href={`https://wa.me/${supportWhatsApp}`}
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-2 px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600"
            >
              <Phone size={18} />
              WhatsApp
            </a>
          )}
        </div>
      </div>
    </div>
  );
};

export default SettingsPage;
```

### 2. Support Contact Component (For Public Pages)

```tsx
// components/SupportContact.tsx

import React, { useEffect, useState } from 'react';
import { Mail, MessageCircle, Phone, ExternalLink } from 'lucide-react';
import settingsService from '../services/settingsService';
import { SupportContactResponse } from '../types/settings';

interface SupportContactProps {
  variant?: 'full' | 'compact' | 'inline';
  showTitle?: boolean;
  className?: string;
}

const SupportContact: React.FC<SupportContactProps> = ({
  variant = 'full',
  showTitle = true,
  className = ''
}) => {
  const [contact, setContact] = useState<SupportContactResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadContact();
  }, []);

  const loadContact = async () => {
    try {
      const data = await settingsService.getSupportContact();
      setContact(data);
    } catch (error) {
      console.error('Failed to load support contact:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading || !contact?.isConfigured) {
    return null;
  }

  // Inline variant (for footer)
  if (variant === 'inline') {
    return (
      <div className={`flex items-center gap-4 ${className}`}>
        {contact.supportEmail && (
          <a
            href={contact.emailLink!}
            className="text-gray-400 hover:text-white transition"
            title="Email Support"
          >
            <Mail size={20} />
          </a>
        )}
        {contact.supportWhatsApp && (
          <a
            href={contact.whatsAppLink!}
            target="_blank"
            rel="noopener noreferrer"
            className="text-gray-400 hover:text-green-500 transition"
            title="WhatsApp Support"
          >
            <MessageCircle size={20} />
          </a>
        )}
      </div>
    );
  }

  // Compact variant
  if (variant === 'compact') {
    return (
      <div className={`flex flex-col gap-2 ${className}`}>
        {contact.supportEmail && (
          <a
            href={contact.emailLink!}
            className="flex items-center gap-2 text-sm text-gray-600 hover:text-blue-600"
          >
            <Mail size={16} />
            {contact.supportEmail}
          </a>
        )}
        {contact.supportWhatsApp && (
          <a
            href={contact.whatsAppSupportLink!}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-2 text-sm text-gray-600 hover:text-green-600"
          >
            <MessageCircle size={16} />
            WhatsApp Support
            <ExternalLink size={12} />
          </a>
        )}
      </div>
    );
  }

  // Full variant (for contact page or banned page)
  return (
    <div className={`bg-white rounded-xl shadow-sm border border-gray-200 p-6 ${className}`}>
      {showTitle && (
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Contact Support
        </h3>
      )}
      
      <div className="space-y-4">
        {contact.supportEmail && (
          <a
            href={contact.emailLink!}
            className="flex items-center gap-4 p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition group"
          >
            <div className="p-3 bg-blue-100 rounded-full group-hover:bg-blue-200 transition">
              <Mail size={24} className="text-blue-600" />
            </div>
            <div>
              <p className="font-medium text-gray-900">Email Support</p>
              <p className="text-sm text-gray-500">{contact.supportEmail}</p>
            </div>
          </a>
        )}
        
        {contact.supportWhatsApp && (
          <a
            href={contact.whatsAppSupportLink!}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-4 p-4 bg-green-50 border border-green-200 rounded-lg hover:bg-green-100 transition group"
          >
            <div className="p-3 bg-green-500 rounded-full group-hover:bg-green-600 transition">
              <MessageCircle size={24} className="text-white" />
            </div>
            <div>
              <p className="font-medium text-gray-900">WhatsApp Support</p>
              <p className="text-sm text-gray-500">Chat with us instantly</p>
            </div>
            <ExternalLink size={16} className="ml-auto text-gray-400" />
          </a>
        )}
      </div>
    </div>
  );
};

export default SupportContact;
```

### 3. Usage on Banned Page

```tsx
// pages/AccountBanned.tsx

import React from 'react';
import { Ban } from 'lucide-react';
import SupportContact from '../components/SupportContact';

const AccountBanned: React.FC = () => {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-red-50 to-red-100 py-12 px-4">
      <div className="max-w-lg w-full text-center">
        {/* Icon & Title */}
        <div className="mx-auto w-24 h-24 bg-red-100 rounded-full flex items-center justify-center mb-8">
          <Ban className="h-12 w-12 text-red-600" />
        </div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">
          Account Suspended
        </h1>
        <p className="text-lg text-gray-600 mb-8">
          Your account has been suspended. Please contact support for assistance.
        </p>

        {/* Dynamic Support Contact */}
        <SupportContact variant="full" showTitle={true} />
      </div>
    </div>
  );
};

export default AccountBanned;
```

---

## ??? Setup Instructions

### 1. Run SQL Script

Execute this in SQL Server Management Studio (SSMS):

```sql
-- File: TijarahJoDBAPI\Scripts\CreateSystemSettingsTable.sql
```

This will:
- Create `TbSystemSettings` table
- Insert default settings (SupportEmail, SupportWhatsApp, etc.)
- Create all stored procedures
- Create necessary indexes

### 2. Restart API

The API will automatically include the new `SettingsController`.

### 3. Test Endpoints

**Public (no auth):**
```bash
curl https://localhost:7064/api/settings/support
```

**Admin (with token):**
```bash
curl -X PUT https://localhost:7064/api/settings/support \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{"supportEmail":"help@tijarahjo.com","supportWhatsApp":"962791234567"}'
```

---

## ?? Validation Rules

### Support Email
- Required
- Valid email format
- Max 200 characters

### Support WhatsApp
- Required
- Must be Jordan mobile format: `9627[7/8/9]XXXXXXX`
- Exactly 12 digits
- Must start with `9627` followed by `7`, `8`, or `9`

**Valid examples:**
- `962791234567` ?
- `962781234567` ?
- `962799876543` ?

**Invalid examples:**
- `0791234567` ? (missing country code)
- `+962791234567` ? (no + sign)
- `962711234567` ? (invalid carrier prefix)
- `96279123456` ? (only 11 digits)

---

## ?? Testing Checklist

### Admin Dashboard
- [ ] Settings page loads correctly
- [ ] Current values are displayed
- [ ] Email validation works
- [ ] WhatsApp validation works
- [ ] Save button disabled when no changes
- [ ] Save success shows toast
- [ ] Save error shows error message
- [ ] Preview updates in real-time

### Public Pages
- [ ] Support contact loads on banned page
- [ ] Email link works (opens mail client)
- [ ] WhatsApp link works (opens WhatsApp)
- [ ] Component handles missing data gracefully

---

## ?? API Response Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Setting created |
| 204 | Setting deleted |
| 400 | Validation error |
| 401 | Not authenticated |
| 403 | Not admin |
| 404 | Setting not found |
| 409 | Setting key already exists |
| 500 | Server error |

---

## ?? Security Notes

1. **Protected Settings**: `SupportEmail`, `SupportWhatsApp`, `SiteName` cannot be deleted
2. **Admin Only**: All write operations require Admin role
3. **Public Settings**: Only settings with `IsPublic = true` are accessible without auth
4. **Audit Trail**: `UpdatedByUserID` tracks who modified each setting

---

*Last Updated: December 2024*  
*API Version: 1.0*
