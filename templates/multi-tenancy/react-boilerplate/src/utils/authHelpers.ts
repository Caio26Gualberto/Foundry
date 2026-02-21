import { USER_ROLES } from './constants';
import type { User } from '../types';

// Helper functions
export const isAdminGlobal = (user: User | null): boolean => {
  return user?.roles.includes(USER_ROLES.ADMIN_GLOBAL) ?? false;
};

export const isGlobalManager = (user: User | null): boolean => {
  return user?.roles.includes(USER_ROLES.GLOBAL_MANAGER) ?? false;
};

export const canAccessTenantSelection = (user: User | null): boolean => {
  return isAdminGlobal(user) || isGlobalManager(user);
};

export const needsTenantSelection = (user: User | null): boolean => {
  return canAccessTenantSelection(user) && (!user?.tenantId || user.tenantId === '');
};
