import { USER_ROLES } from './constants';
import type { User } from '../types';

export const isAdminGlobal = (user: User | null): boolean => {
  return user?.roles.includes(USER_ROLES.ADMIN_GLOBAL) ?? false;
};

export const isGlobalManager = (user: User | null): boolean => {
  return user?.roles.includes(USER_ROLES.GLOBAL_MANAGER) ?? false;
};
