import React, { useEffect, useState, useCallback } from 'react';
import type { ReactNode } from 'react';
import { apiService } from '../../services/api';
import { STORAGE_KEYS } from '../../utils/constants';
import type { AuthContextType, User } from '../../types';
import { AuthContext } from './context';

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      const storedToken = localStorage.getItem(STORAGE_KEYS.TOKEN);
      const storedRefreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);

      if (storedToken && storedRefreshToken) {
        try {
          setToken(storedToken);
          setRefreshToken(storedRefreshToken);
          
          // Extract user data from JWT token
          const userData = apiService.getUserFromToken(storedToken);
          if (userData) {
            setUser(userData);
            localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
          } else {
            // Token is invalid, try to refresh
            await refreshTokens();
          }
        } catch (error) {
          console.error('Token validation failed:', error);
          await logout();
        }
      }
      
      setIsLoading(false);
    };

    initializeAuth();
  }, []);

  const login = async (email: string, password: string): Promise<void> => {
    try {
      setIsLoading(true);
      const response = await apiService.login(email, password);
      
      if (response.isSuccess && response.data?.tokens) {
        const { token: accessToken, refreshToken: newRefreshToken } = response.data.tokens;
        
        setToken(accessToken);
        setRefreshToken(newRefreshToken);
        
        // Extract user data from JWT
        const userData = apiService.getUserFromToken(accessToken);
        if (userData) {
          setUser(userData);
          
          localStorage.setItem(STORAGE_KEYS.TOKEN, accessToken);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
      } else {
        throw new Error(response.message || 'Login failed');
      }
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = useCallback(async (): Promise<void> => {
    try {
      if (token) {
        await apiService.logout();
      }
    } catch (error) {
      console.error('Logout API call failed:', error);
    } finally {
      setUser(null);
      setToken(null);
      setRefreshToken(null);
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
    }
  }, [token]);

  const refreshTokens = useCallback(async (): Promise<boolean> => {
    try {
      if (!refreshToken) {
        await logout();
        return false;
      }

      const response = await apiService.refreshToken(refreshToken);
      
      if (response.isSuccess && response.data) {
        const { token: newAccessToken, refreshToken: newRefreshToken } = response.data;
        
        setToken(newAccessToken);
        setRefreshToken(newRefreshToken);
        
        const userData = apiService.getUserFromToken(newAccessToken);
        if (userData) {
          setUser(userData);
          
          localStorage.setItem(STORAGE_KEYS.TOKEN, newAccessToken);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
        
        return true;
      } else {
        await logout();
        return false;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
      await logout();
      return false;
    }
  }, [refreshToken, logout]);

  const selectTenant = async (tenantId: string): Promise<void> => {
    try {
      setIsLoading(true);
      const response = await apiService.impersonateTenant(tenantId);
      
      if (response.isSuccess && response.data?.tokens) {
        const { token: accessToken, refreshToken: newRefreshToken } = response.data.tokens;
        
        setToken(accessToken);
        setRefreshToken(newRefreshToken);
        
        const userData = apiService.getUserFromToken(accessToken);
        if (userData) {
          setUser(userData);
          
          localStorage.setItem(STORAGE_KEYS.TOKEN, accessToken);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
      } else {
        throw new Error(response.message || 'Tenant selection failed');
      }
    } catch (error) {
      console.error('Tenant selection failed:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const value: AuthContextType = {
    user,
    token,
    refreshToken,
    isLoading,
    login,
    logout,
    selectTenant,
    refreshTokens,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

