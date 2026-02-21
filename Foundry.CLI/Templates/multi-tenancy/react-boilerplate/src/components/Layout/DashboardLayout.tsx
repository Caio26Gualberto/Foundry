import React, { useState } from 'react';
import { Box, Toolbar, Alert, Button } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import { ROUTES } from '../../utils/constants';

interface DashboardLayoutProps {
  children: React.ReactNode;
}

const DRAWER_WIDTH = 280;
const DRAWER_WIDTH_COLLAPSED = 64;

export const DashboardLayout: React.FC<DashboardLayoutProps> = ({ children }) => {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [desktopOpen, setDesktopOpen] = useState(true);
  const navigate = useNavigate();
  
  // Verifica se o usuário está usando acesso temporário
  const hasTemporaryAccess = sessionStorage.getItem('allowDashboardAccess') === 'true';

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleDesktopDrawerToggle = () => {
    setDesktopOpen(!desktopOpen);
  };

  const handleGoToTenantSelection = () => {
    sessionStorage.removeItem('allowDashboardAccess');
    navigate(ROUTES.TENANT_SELECTION);
  };

  const currentDrawerWidth = desktopOpen ? DRAWER_WIDTH : DRAWER_WIDTH_COLLAPSED;

  return (
    <Box sx={{ display: 'flex', width: '100%', minHeight: '100vh' }}>
      <Header 
        onMenuClick={handleDrawerToggle} 
        onDesktopMenuClick={handleDesktopDrawerToggle}
        drawerWidth={currentDrawerWidth} 
      />
      
      <Sidebar
        mobileOpen={mobileOpen}
        desktopOpen={desktopOpen}
        onMobileClose={handleDrawerToggle}
        onDesktopToggle={handleDesktopDrawerToggle}
        drawerWidth={DRAWER_WIDTH}
        collapsedWidth={DRAWER_WIDTH_COLLAPSED}
      />

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { xs: '100%', sm: `calc(100% - ${currentDrawerWidth}px)` },
          minHeight: '100vh',
          bgcolor: 'background.default',
          overflow: 'auto',
          transition: 'width 0.3s ease, margin-left 0.3s ease, padding 0.3s ease',
        }}
      >
        <Toolbar />
        
        {/* Banner informativo para acesso temporário */}
        {hasTemporaryAccess && (
          <Alert 
            severity="info" 
            sx={{ mb: 2 }}
            action={
              <Button 
                color="inherit" 
                size="small" 
                onClick={handleGoToTenantSelection}
              >
                Selecionar Tenant
              </Button>
            }
          >
            Você está acessando o dashboard temporariamente sem selecionar um tenant. 
            Ao fazer logout, precisará clicar novamente no botão para acessar.
          </Alert>
        )}
        
        {children}
      </Box>
    </Box>
  );
};
