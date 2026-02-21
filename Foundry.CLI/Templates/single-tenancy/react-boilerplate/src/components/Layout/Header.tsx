import React from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Box,
  Avatar,
} from '@mui/material';
import {
  Menu as MenuIcon,
  AccountCircle,
} from '@mui/icons-material';
import { NotificationCenter } from '../NotificationCenter';
import LanguageSwitcher from '../common/LanguageSwitcher';
import { translate } from '../../i18n';

interface HeaderProps {
  onMenuClick: () => void;
  onDesktopMenuClick?: () => void;
  drawerWidth: number;
}
export const Header: React.FC<HeaderProps> = ({ onMenuClick, onDesktopMenuClick, drawerWidth }) => {
  return (
    <AppBar
      position="fixed"
      sx={{
        width: { xs: '100%', sm: `calc(100% - ${drawerWidth}px)` },
        ml: { sm: `${drawerWidth}px` },
        bgcolor: 'background.paper',
        color: 'text.primary',
        boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
        zIndex: (theme) => theme.zIndex.drawer + 1,
        transition: 'width 0.3s ease, margin-left 0.3s ease',
      }}
    >
      <Toolbar>
        {/* Mobile menu button */}
        <IconButton
          color="inherit"
          aria-label="open drawer"
          edge="start"
          onClick={onMenuClick}
          sx={{ mr: 2, display: { sm: 'none' } }}
        >
          <MenuIcon />
        </IconButton>

        {/* Desktop menu button */}
        {onDesktopMenuClick && (
          <IconButton
            color="inherit"
            aria-label="toggle drawer"
            edge="start"
            onClick={onDesktopMenuClick}
            sx={{ mr: 2, display: { xs: 'none', sm: 'block' } }}
          >
            <MenuIcon />
          </IconButton>
        )}

        <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
          {translate('sidebar.dashboard')}
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {/* Notifications */}
          <NotificationCenter />

          {/* Language Switcher */}
          <LanguageSwitcher />

          {/* User Avatar */}
          <IconButton color="inherit">
            <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main' }}>
              <AccountCircle />
            </Avatar>
          </IconButton>
        </Box>
      </Toolbar>
    </AppBar>
  );
};
