export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7265/api';

export const ROUTES = {
  LOGIN: '/login',
  CHANGE_PASSWORD: '/change-password',
  TENANT_SELECTION: '/tenant-selection',
  DASHBOARD: '/dashboard',
  USERS: '/dashboard/users',
  TENANT_SETTINGS: '/dashboard/tenant-settings',
  HOME: '/',
} as const;

export const USER_ROLES = {
  ADMIN_GLOBAL: 'AdminGlobal',
  GLOBAL_MANAGER: 'GlobalManager',
  TENANT_ADMIN: 'TenantAdmin',
  USER: 'User',
} as const;

export const STORAGE_KEYS = {
  TOKEN: 'Boilerplate_token',
  REFRESH_TOKEN: 'Boilerplate_refresh_token',
  IMPERSONATED_TOKEN: 'Boilerplate_impersonated_token',
  USER: 'user_data',
} as const;

export const SystemNotificationsEvents = {
  UpdateNotifications: "UpdateNotifications"
} as const;
