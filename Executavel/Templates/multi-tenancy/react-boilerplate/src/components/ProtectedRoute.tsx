import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import { useAuth } from '../contexts/Auth';
import { needsTenantSelection } from '../utils/authHelpers';
import { ROUTES } from '../utils/constants';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requireAuth?: boolean;
  requireTenant?: boolean;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requireAuth = true,
  requireTenant = false,
}) => {
  const { user, token, isLoading } = useAuth();
  const location = useLocation();

  // Show loading spinner while checking authentication
  if (isLoading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '100vh',
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  // Redirect to login if authentication is required but user is not authenticated
  if (requireAuth && (!user || !token)) {
    return <Navigate to={ROUTES.LOGIN} state={{ from: location }} replace />;
  }

  // Verifica se o usuário AdminGlobal tem permissão temporária para acessar dashboard
  const hasTemporaryDashboardAccess = sessionStorage.getItem('allowDashboardAccess') === 'true';
  
  // Permite acesso ao dashboard se tiver permissão temporária
  if (location.pathname.startsWith('/dashboard') && hasTemporaryDashboardAccess && requireAuth && user && token) {
    return <>{children}</>;
  }

  // Redirect to tenant selection if user needs to select a tenant (but not if already there)
  // Mas permite acesso se estiver impersonando (tem token de impersonação)
  const hasImpersonationToken = localStorage.getItem('Boilerplate_impersonated_token');
  if (requireAuth && needsTenantSelection(user) && !hasImpersonationToken && location.pathname !== ROUTES.TENANT_SELECTION) {
    return <Navigate to={ROUTES.TENANT_SELECTION} replace />;
  }

  // Redirect to tenant selection if tenant is required but user doesn't have one
  if (requireTenant && user && (!user.tenantId || user.tenantId === '') && location.pathname !== ROUTES.TENANT_SELECTION) {
    return <Navigate to={ROUTES.TENANT_SELECTION} replace />;
  }

  return <>{children}</>;
};
