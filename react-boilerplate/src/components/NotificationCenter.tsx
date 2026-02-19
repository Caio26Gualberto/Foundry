import React, { useEffect, useState } from 'react';
import {
  IconButton,
  Badge,
  Menu,
  MenuItem,
  Typography,
  Box,
  Divider,
  Button,
  Chip,
  Avatar,
} from '@mui/material';
import {
  Notifications as NotificationsIcon,
  NotificationsNone,
  Info,
  CheckCircle,
  Warning,
  Error,
  Clear,
  MarkEmailRead,
} from '@mui/icons-material';
import { useSignalR } from '../hooks/useSignalR';
import type { SystemNotificationDto } from '../types/systemNotifications';
import apiClient from '../services/apiClient';
import { SystemNotificationsEvents } from '../utils/constants';
import { useTranslation } from 'react-i18next';

export const NotificationCenter: React.FC = () => {
  const { connection } = useSignalR();
  const { t } = useTranslation();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [notifications, setNotifications] = useState<SystemNotificationDto[]>([]);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleMarkAsRead = async (notificationId: number, event: React.MouseEvent) => {
    event.stopPropagation();
    await apiClient.patch<boolean>(`/SystemNotification/MarkAsRead/${notificationId}`, {isRead: true});
  };

  const handleClearAll = async () => {
    await apiClient.post<boolean>('/SystemNotification/ClearAllMessages', {notificationIds: notifications.map(n => n.id)});
    handleClose();
  };

  const getNotificationIcon = (type: string) => {
    switch (type) {
      case 'success':
        return <CheckCircle color="success" />;
      case 'warning':
        return <Warning color="warning" />;
      case 'error':
        return <Error color="error" />;
      default:
        return <Info color="info" />;
    }
  };

  const getNotificationColor = (type: string) => {
    switch (type) {
      case 'success':
        return 'success.main';
      case 'warning':
        return 'warning.main';
      case 'error':
        return 'error.main';
      default:
        return 'info.main';
    }
  };

  const formatTime = (timestamp: Date) => {
    const now = new Date();
    const diff = now.getTime() - new Date(timestamp).getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return t('notifications.timeOfNotification');
    if (minutes < 60) return `${minutes}m`;
    if (hours < 24) return `${hours}h`;
    return `${days}d`;
  };

  const fetchNotifications = async () => {
    try {
      const notifications = await apiClient.get<SystemNotificationDto[]>('/SystemNotification');
      setNotifications(notifications);
    } catch (error) {
      console.error('Error fetching notifications:', error);
    }
  };

  useEffect(() => {
    fetchNotifications();
  }, []);

  useEffect(() => {
    if (!connection) return;

    const handleUpdate = () => {
      fetchNotifications();
    };

    connection.on(SystemNotificationsEvents.UpdateNotifications, handleUpdate);

    return () => {
      connection.off(SystemNotificationsEvents.UpdateNotifications, handleUpdate);
    };
  }, [connection]);

  return (
    <>
      <IconButton
        color="inherit"
        onClick={handleClick}
      >
        <Badge
          badgeContent={notifications.filter(n => !n.isRead).length}
          color="error"
          max={9}
          variant="standard"
          showZero={false} 
          invisible={notifications.filter(n => !n.isRead).length === 0}
        >
          <NotificationsIcon />
        </Badge>
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        PaperProps={{
          sx: {
            width: 360,
            maxHeight: 480,
            overflow: 'hidden',
          },
        }}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
      >
        {/* Header */}
        <Box sx={{ p: 2, pb: 1 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">
              {t('notifications.title')}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              {notifications.length > 0 && (
                <Button
                  size="small"
                  onClick={handleClearAll}
                  startIcon={<Clear />}
                >
                  {t('notifications.clearAll')}
                </Button>
              )}
            </Box>
          </Box>
        </Box>

        <Divider />

        {/* Notifications List */}
        <Box sx={{ maxHeight: 400, overflow: 'auto' }}>
          {notifications.length === 0 ? (
            <Box sx={{ p: 3, textAlign: 'center' }}>
              <NotificationsNone sx={{ fontSize: 48, color: 'text.secondary', mb: 1 }} />
              <Typography variant="body2" color="text.secondary">
                {t('notifications.noNotifications')}
              </Typography>
            </Box>
          ) : (
            notifications.map((notification) => (
              <MenuItem
                key={notification.id}
                sx={{
                  py: 2,
                  px: 2,
                  // borderLeft: `4px solid ${getNotificationColor(notification.type)}`,
                  backgroundColor: notification.isRead ? 'transparent' : 'action.hover',
                  '&:hover': {
                    backgroundColor: 'action.selected',
                  },
                  display: 'block',
                }}
              >
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  {/* Header with icon, title and time */}
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <Avatar
                      sx={{
                        width: 20,
                        height: 20,
                        bgcolor: 'transparent',
                        '& .MuiSvgIcon-root': {
                          fontSize: '1rem',
                        },
                      }}
                    >
                      {/* {getNotificationIcon(notification.type)} */}
                    </Avatar>
                    <Typography
                      variant="subtitle2"
                      sx={{
                        fontWeight: notification.isRead ? 400 : 600,
                        flex: 1,
                        lineHeight: 1.4,
                      }}
                    >
                      {notification.title}
                    </Typography>
                    <Typography 
                      variant="caption" 
                      color="text.secondary"
                      sx={{ 
                        whiteSpace: 'nowrap',
                        fontSize: '0.7rem',
                      }}
                    >
                      {formatTime(notification.createdAt)}
                    </Typography>
                  </Box>

                  {/* Message */}
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{
                      pl: 3.5,
                      display: '-webkit-box',
                      WebkitLineClamp: 2,
                      WebkitBoxOrient: 'vertical',
                      overflow: 'hidden',
                      lineHeight: 1.5,
                      fontSize: '0.875rem',
                    }}
                  >
                    {notification.content}
                  </Typography>

                  {/* Mark as read button */}
                  {!notification.isRead && (
                    <Box sx={{ pl: 3.5 }}>
                      <Button
                        size="small"
                        startIcon={<MarkEmailRead sx={{ fontSize: '0.9rem' }} />}
                        onMouseDown={(e) => e.stopPropagation()}
                        onClick={(e) => handleMarkAsRead(notification.id, e)}
                        sx={{ 
                          mt: 0.5, 
                          fontSize: '0.75rem',
                          textTransform: 'none',
                          py: 0.5,
                        }}
                      >
                        {t('notifications.markAsRead')}
                      </Button>
                    </Box>
                  )}
                </Box>
              </MenuItem>
            ))
          )}
        </Box>
      </Menu>
    </>
  );
};
