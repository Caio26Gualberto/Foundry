import React, { useState } from 'react';
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
  Collapse,
} from '@mui/material';
import {
  Home,
  Dashboard,
  People,
  Analytics,
  Security,
  ChevronRight,
  AccountCircle,
  ExpandLess,
  ExpandMore,
} from '@mui/icons-material';
import { useAuth } from '../../contexts/Auth';
import { useNavigate } from 'react-router-dom';
import { translate } from '../../i18n';

interface SidebarProps {
  mobileOpen: boolean;
  desktopOpen: boolean;
  onMobileClose: () => void;
  onDesktopToggle: () => void;
  drawerWidth: number;
  collapsedWidth: number;
}

interface MenuItem {
  text: string;
  icon: React.ReactNode;
  path?: string;
  subItems?: MenuItem[];
  action?: () => void;
}

const getMenuItems = (): MenuItem[] => [
  { text: translate('sidebar.home'), icon: <Home />, path: '/dashboard' },
  { text: translate('sidebar.dashboard'), icon: <Dashboard />, path: '/dashboard/analytics' },
  { text: translate('sidebar.users.title'), icon: <People />, path: '/dashboard/users' },
  { text: translate('sidebar.reports'), icon: <Analytics />, path: '/dashboard/reports' },
  { text: translate('sidebar.security'), icon: <Security />, path: '/dashboard/security' },
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
  const navigate = useNavigate();
  const [openSubmenus, setOpenSubmenus] = useState<{ [key: string]: boolean }>({});

  const handleNavigation = (path: string) => {
    navigate(path);
  };

  const toggleSubmenu = (itemText: string) => {
    setOpenSubmenus(prev => ({
      ...prev,
      [itemText]: !prev[itemText]
    }));
  };

  const menuItems = getMenuItems();

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
          <ChevronRight />
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
            <React.Fragment key={item.text}>
              <ListItem disablePadding>
                <ListItemButton
                  onClick={() => {
                    if (item.subItems && !isCollapsed) {
                      toggleSubmenu(item.text);
                    } else if (item.path) {
                      handleNavigation(item.path);
                    } else if (item.action) {
                      item.action();
                    }
                  }}
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
                    <>
                      <ListItemText 
                        primary={item.text}
                        primaryTypographyProps={{
                          fontSize: '0.875rem',
                          fontWeight: 500,
                        }}
                      />
                      {item.subItems && (
                        openSubmenus[item.text] ? <ExpandLess /> : <ExpandMore />
                      )}
                    </>
                  )}
                </ListItemButton>
              </ListItem>
              
              {/* Submenu */}
              {item.subItems && !isCollapsed && (
                <Collapse in={openSubmenus[item.text]} timeout="auto" unmountOnExit>
                  <List component="div" disablePadding>
                    {item.subItems.map((subItem) => (
                      <ListItem key={subItem.text} disablePadding>
                        <ListItemButton
                          onClick={() => {
                            if (subItem.path) {
                              handleNavigation(subItem.path);
                            } else if (subItem.action) {
                              subItem.action();
                            }
                          }}
                          sx={{
                            pl: 4,
                            minHeight: 40,
                            '&:hover': {
                              backgroundColor: 'action.hover',
                            },
                          }}
                        >
                          <ListItemIcon sx={{ minWidth: 32, justifyContent: 'center' }}>
                            {subItem.icon}
                          </ListItemIcon>
                          <ListItemText 
                            primary={subItem.text}
                            primaryTypographyProps={{
                              fontSize: '0.8rem',
                              fontWeight: 400,
                            }}
                          />
                        </ListItemButton>
                      </ListItem>
                    ))}
                  </List>
                </Collapse>
              )}
            </React.Fragment>
          ))}
        </List>
      </Box>

      <Divider />

      {/* Bottom Actions */}
      <Box sx={{ p: 1 }}>
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
              primary={translate('sidebar.logout')}
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
          keepMounted: true,
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
