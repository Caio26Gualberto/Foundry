import React from 'react';
import {
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
  Divider,
  Avatar,
  Chip,
  IconButton,
} from '@mui/material';
import {
  Home,
  Dashboard,
  People,
  Settings,
  Business,
  Analytics,
  Security,
  ChevronLeft,
  AccountCircle,
} from '@mui/icons-material';
import { useAuth } from '../../contexts/Auth';
import { canAccessTenantSelection } from '../../utils/authHelpers';

interface SidebarProps {
  mobileOpen: boolean;
  desktopOpen: boolean;
  onMobileClose: () => void;
  onDesktopToggle: () => void;
  drawerWidth: number;
  collapsedWidth: number;
}

const menuItems = [
  { text: 'Início', icon: <Home />, path: '/dashboard' },
  { text: 'Dashboard', icon: <Dashboard />, path: '/dashboard/analytics' },
  { text: 'Usuários', icon: <People />, path: '/dashboard/users' },
  { text: 'Relatórios', icon: <Analytics />, path: '/dashboard/reports' },
  { text: 'Segurança', icon: <Security />, path: '/dashboard/security' },
];

export const Sidebar: React.FC<SidebarProps> = ({ 
  mobileOpen, 
  desktopOpen, 
  onMobileClose, 
  onDesktopToggle, 
  drawerWidth, 
  collapsedWidth 
}) => {
  const { user, logout } = useAuth();

  const handleNavigation = (path: string) => {
    // TODO: Implement navigation logic
    console.log('Navigate to:', path);
  };

  const handleTenantConfig = () => {
    // TODO: Implement tenant configuration
    console.log('Open tenant configuration');
  };

  const getDrawerContent = (isCollapsed = false) => (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Box sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        {!isCollapsed && (
          <Typography variant="h6" noWrap component="div" sx={{ fontWeight: 600 }}>
            Boilerplate
          </Typography>
        )}
        <IconButton 
          onClick={isCollapsed ? onDesktopToggle : onMobileClose} 
          sx={{ display: { sm: isCollapsed ? 'block' : 'none' } }}
        >
          <ChevronLeft />
        </IconButton>
      </Box>

      <Divider />

      {/* User Info */}
      {!isCollapsed && (
        <Box sx={{ p: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <Avatar sx={{ width: 40, height: 40, mr: 2, bgcolor: 'primary.main' }}>
              <AccountCircle />
            </Avatar>
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography variant="subtitle2" noWrap>
                {user?.userName}
              </Typography>
              <Typography variant="caption" color="text.secondary" noWrap>
                {user?.email}
              </Typography>
            </Box>
          </Box>

          {/* User Roles */}
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mb: 1 }}>
            {user?.roles.map((role) => (
              <Chip
                key={role}
                label={role}
                size="small"
                variant="outlined"
                sx={{ fontSize: '0.7rem' }}
              />
            ))}
          </Box>

          {/* Tenant Info */}
          {user?.tenantId && (
            <Box sx={{ mt: 1 }}>
              <Typography variant="caption" color="text.secondary">
                Tenant: {user.tenantId}
              </Typography>
            </Box>
          )}

          {/* Global Admin Info */}
          {canAccessTenantSelection(user) && !user?.tenantId && (
            <Chip
              label="Modo Global"
              size="small"
              color="primary"
              sx={{ mt: 1 }}
            />
          )}
        </Box>
      )}

      {/* Collapsed User Avatar */}
      {isCollapsed && (
        <Box sx={{ p: 1, display: 'flex', justifyContent: 'center' }}>
          <Avatar sx={{ width: 40, height: 40, bgcolor: 'primary.main' }}>
            <AccountCircle />
          </Avatar>
        </Box>
      )}

      <Divider />

      {/* Navigation Menu */}
      <Box sx={{ flex: 1, overflow: 'auto' }}>
        <List>
          {menuItems.map((item) => (
            <ListItem key={item.text} disablePadding>
              <ListItemButton
                onClick={() => handleNavigation(item.path)}
                sx={{
                  minHeight: 48,
                  px: 2.5,
                  '&:hover': {
                    backgroundColor: 'action.hover',
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: isCollapsed ? 'auto' : 40, justifyContent: 'center' }}>
                  {item.icon}
                </ListItemIcon>
                {!isCollapsed && (
                  <ListItemText 
                    primary={item.text}
                    primaryTypographyProps={{
                      fontSize: '0.875rem',
                      fontWeight: 500,
                    }}
                  />
                )}
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </Box>

      <Divider />

      {/* Bottom Actions */}
      <Box sx={{ p: 1 }}>
        {user?.tenantId && (
          <ListItemButton
            onClick={handleTenantConfig}
            sx={{ borderRadius: 1, mb: 1 }}
          >
            <ListItemIcon sx={{ minWidth: isCollapsed ? 'auto' : 40, justifyContent: 'center' }}>
              <Business />
            </ListItemIcon>
            {!isCollapsed && (
              <ListItemText 
                primary="Configurações do Tenant"
                primaryTypographyProps={{
                  fontSize: '0.875rem',
                }}
              />
            )}
          </ListItemButton>
        )}

        <ListItemButton
          onClick={() => handleNavigation('/dashboard/settings')}
          sx={{ borderRadius: 1, mb: 1 }}
        >
          <ListItemIcon sx={{ minWidth: isCollapsed ? 'auto' : 40, justifyContent: 'center' }}>
            <Settings />
          </ListItemIcon>
          {!isCollapsed && (
            <ListItemText 
              primary="Configurações"
              primaryTypographyProps={{
                fontSize: '0.875rem',
              }}
            />
          )}
        </ListItemButton>

        <ListItemButton
          onClick={logout}
          sx={{ 
            borderRadius: 1,
            color: 'error.main',
            '&:hover': {
              backgroundColor: 'error.light',
              color: 'error.contrastText',
            },
          }}
        >
          <ListItemIcon sx={{ minWidth: isCollapsed ? 'auto' : 40, justifyContent: 'center', color: 'inherit' }}>
            <AccountCircle />
          </ListItemIcon>
          {!isCollapsed && (
            <ListItemText 
              primary="Sair"
              primaryTypographyProps={{
                fontSize: '0.875rem',
              }}
            />
          )}
        </ListItemButton>
      </Box>
    </Box>
  );

  const currentDesktopWidth = desktopOpen ? drawerWidth : collapsedWidth;

  return (
    <Box
      component="nav"
      sx={{ width: { sm: currentDesktopWidth }, flexShrink: { sm: 0 } }}
    >
      {/* Mobile drawer */}
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={onMobileClose}
        ModalProps={{
          keepMounted: true, // Better open performance on mobile.
        }}
        sx={{
          display: { xs: 'block', sm: 'none' },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: drawerWidth,
          },
        }}
      >
        {getDrawerContent(false)}
      </Drawer>

      {/* Desktop drawer */}
      <Drawer
        variant="permanent"
        sx={{
          display: { xs: 'none', sm: 'block' },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: currentDesktopWidth,
            transition: 'width 0.3s ease',
            overflowX: 'hidden',
          },
        }}
        open
      >
        {getDrawerContent(!desktopOpen)}
      </Drawer>
    </Box>
  );
};
