import React, { useEffect, useState, useCallback } from 'react';
import type { ReactNode } from 'react';
import { useSnackbar } from 'notistack';
import apiClient from '../../services/apiClient';
import { STORAGE_KEYS } from '../../utils/constants';
import type { AuthContextType, LoginResponseDto, TokensDto, User } from '../../types';
import { AuthContext } from './context';

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const { enqueueSnackbar } = useSnackbar();
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

    async function decodeJWT(token: string | null): Promise<User | null> {
    try {
      if (!token) return null;
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );

      const payload = JSON.parse(jsonPayload);
      console.log('JWT Payload:', payload);
      
      const userData = {
        id: payload.sub || payload.userId || payload.id || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        email: payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        userName: payload.unique_name || payload.userName || payload.name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        tenantId: payload.tenantId,
        tenantName: payload.tenantName,
        roles: Array.isArray(payload.role) ? payload.role : payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ? [payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']] : [payload.role].filter(Boolean),
        impersonatedBy: payload.impersonatedBy,
      };
      
      console.log('Decoded user data:', userData);
      return userData;
    } catch (error) {
      console.error('Error decoding JWT:', error);
      return null;
    }
  }

  const logout = useCallback(async (): Promise<void> => {
    try {
      if (token) {
        await apiClient.get('/Auth/Logout');
      }
    } catch (error) {
      console.error('Logout API call failed:', error);
      enqueueSnackbar('Erro ao fazer logout', { variant: 'error' });
    } finally {
      setUser(null);
      setToken(null);
      setRefreshToken(null);
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
      // Remove permissão temporária de acesso ao dashboard
      sessionStorage.removeItem('allowDashboardAccess');
    }
  }, [token, enqueueSnackbar]);

  const refreshTokens = useCallback(async (): Promise<boolean> => {
    try {
      if (!refreshToken) {
        await logout();
        return false;
      }

      const response = await apiClient.get<TokensDto>('/Auth/RefreshToken');
      
      if (response) {
        const { token: newAccessToken, refreshToken: newRefreshToken } = response;
        
        setToken(newAccessToken);
        setRefreshToken(newRefreshToken);
        
        const userData = await decodeJWT(newAccessToken);
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
      enqueueSnackbar('Sessão expirada. Faça login novamente.', { variant: 'warning' });
      await logout();
      return false;
    }
  }, [refreshToken, logout, enqueueSnackbar]);

  useEffect(() => {
    const initializeAuth = async () => {
      // Prioriza o token de impersonação se existir
      const impersonatedToken = localStorage.getItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
      const storedToken = localStorage.getItem(STORAGE_KEYS.TOKEN);
      const storedRefreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);

      const activeToken = impersonatedToken || storedToken;

      if (activeToken && storedRefreshToken) {
        try {
          setToken(activeToken);
          setRefreshToken(storedRefreshToken);
          
          const userData = await decodeJWT(activeToken);
          if (userData) {
            setUser(userData);
            localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
          } else if (!impersonatedToken) {
            // Token is invalid, try to refresh (only if not impersonated)
            await refreshTokens();
          }
        } catch (error) {
          console.error('Token validation failed:', error);
          if (impersonatedToken) {
            // Se o token de impersonação falhou, remove e tenta com o token original
            localStorage.removeItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
            if (storedToken) {
              const userData = await decodeJWT(storedToken);
              if (userData) {
                setToken(storedToken);
                setUser(userData);
                localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
              }
            }
          } else {
            enqueueSnackbar('Erro ao validar token. Faça login novamente.', { variant: 'error' });
            await logout();
          }
        }
      }
      
      setIsLoading(false);
    };

    initializeAuth();
  }, [enqueueSnackbar, refreshTokens, logout]);

  const login = async (email: string, password: string): Promise<{ isNeededChangePassword: boolean }> => {
    try {
      setIsLoading(true);
      const response = await apiClient.post<LoginResponseDto>('/Auth/Login', { email, password });
      
      if (response?.isNeededChangePassword) {
        return { isNeededChangePassword: true };
      }

      if (response?.tokens?.token && response?.tokens?.refreshToken) {
        const { token: accessToken, refreshToken: newRefreshToken } = response.tokens;
        setToken(accessToken);
        setRefreshToken(newRefreshToken);
        const userData = await decodeJWT(accessToken);
        if (userData) {
          setUser(userData);
          localStorage.setItem(STORAGE_KEYS.TOKEN, accessToken);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
        return { isNeededChangePassword: false };
      }

      throw new Error('Login failed - no tokens received');
    } catch (error) {
      console.error('Login failed:', error);
      const errorMessage = error instanceof Error ? error.message : 'Erro no login';
      enqueueSnackbar(errorMessage, { variant: 'error' });
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const selectTenant = async (tenantId: string): Promise<void> => {
    try {
      setIsLoading(true);
      const response = await apiClient.post<TokensDto>('/Impersonate', { tenantId });
      
      if (response?.token && response?.refreshToken) {
        const { token: accessToken, refreshToken: newRefreshToken } = response;
        
        setToken(accessToken);
        setRefreshToken(newRefreshToken);
        
        const userData = await decodeJWT(accessToken);
        if (userData) {
          setUser(userData);
          
          localStorage.setItem(STORAGE_KEYS.TOKEN, accessToken);
          localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
      } else {
        throw new Error('Tenant selection failed - no tokens received');
      }
    } catch (error) {
      console.error('Tenant selection failed:', error);
      const errorMessage = error instanceof Error ? error.message : 'Erro ao selecionar tenant';
      enqueueSnackbar(errorMessage, { variant: 'error' });
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const stopImpersonation = async (): Promise<void> => {
    try {
      setIsLoading(true);
      // Remove o token de impersonação
      localStorage.removeItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
      
      // Volta para o token original
      const originalToken = localStorage.getItem(STORAGE_KEYS.TOKEN);
      if (originalToken) {
        setToken(originalToken);
        const userData = await decodeJWT(originalToken);
        if (userData) {
          setUser(userData);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
          enqueueSnackbar('Impersonação finalizada', { variant: 'info' });
        }
      }
    } catch (error) {
      console.error('Error stopping impersonation:', error);
      enqueueSnackbar('Erro ao finalizar impersonação', { variant: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const refreshUserFromToken = async (): Promise<void> => {
    try {
      setIsLoading(true);
      // Prioriza o token de impersonação se existir
      const impersonatedToken = localStorage.getItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
      const storedToken = localStorage.getItem(STORAGE_KEYS.TOKEN);
      const activeToken = impersonatedToken || storedToken;

      if (activeToken) {
        setToken(activeToken);
        const userData = await decodeJWT(activeToken);
        if (userData) {
          console.log('User data decoded from token:', userData);
          setUser(userData);
          localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));
        }
      }
    } catch (error) {
      console.error('Error refreshing user from token:', error);
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
    stopImpersonation,
    refreshUserFromToken,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

