import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  IconButton,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  OutlinedInput,
  CircularProgress,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import apiClient from '../../../services/apiClient';
import type { UserDto } from '../../../types/Users';
import type { CreateNotificationDto } from '../../../types/systemNotifications';
import { translate } from '../../../i18n';

interface CreateNotificationModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}

export const CreateNotificationModal: React.FC<CreateNotificationModalProps> = ({
  open,
  onClose,
  onSuccess,
}) => {
  const { enqueueSnackbar } = useSnackbar();
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [selectedUsers, setSelectedUsers] = useState<string[]>([]);
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingUsers, setLoadingUsers] = useState(false);
  const [errors, setErrors] = useState<{ title?: string; content?: string }>({});

  useEffect(() => {
    if (open) {
      fetchUsers();
    }
  }, [open]);

  const fetchUsers = async () => {
    try {
      setLoadingUsers(true);
      const usersData = await apiClient.get<UserDto[]>('/User');
      setUsers(usersData);
    } catch (error) {
      console.error('Error fetching users:', error);
    } finally {
      setLoadingUsers(false);
    }
  };

  const validate = (): boolean => {
    const newErrors: { title?: string; content?: string } = {};

    if (!title.trim()) {
      newErrors.title = translate('notifications.errors.titleRequired');
    }

    if (!content.trim()) {
      newErrors.content = translate('notifications.errors.contentRequired');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;

    setLoading(true);
    try {
      const payload: CreateNotificationDto = {
        title,
        content,
        userIds: selectedUsers,
      };
      await apiClient.post('/SystemNotification', payload);
      enqueueSnackbar(translate('notifications.createSuccess'), { variant: 'success' });
      onSuccess?.();
      handleClose();
    } catch (error) {
      console.error('Error creating notification:', error);
      const errorMessage =
        error instanceof Error ? error.message : translate('notifications.createError');
      enqueueSnackbar(errorMessage, { variant: 'error' });
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setTitle('');
    setContent('');
    setSelectedUsers([]);
    setErrors({});
    onClose();
  };

  const handleFieldChange = (field: 'title' | 'content', value: string) => {
    if (field === 'title') setTitle(value);
    else setContent(value);

    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const handleUsersChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value;
    setSelectedUsers(typeof value === 'string' ? value.split(',') : value);
  };

  const handleSelectAll = () => {
    if (selectedUsers.length === users.length) {
      setSelectedUsers([]);
    } else {
      setSelectedUsers(users.map((u) => u.id));
    }
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 2 },
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6">
            {translate('notifications.createModal.title')}
          </Typography>
          <IconButton onClick={handleClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        <Box sx={{ py: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            fullWidth
            label={translate('notifications.createModal.titleField')}
            value={title}
            onChange={(e) => handleFieldChange('title', e.target.value)}
            error={!!errors.title}
            helperText={errors.title}
            required
            autoFocus
          />
          <TextField
            fullWidth
            label={translate('notifications.createModal.contentField')}
            value={content}
            onChange={(e) => handleFieldChange('content', e.target.value)}
            error={!!errors.content}
            helperText={errors.content}
            required
            multiline
            rows={4}
          />
          <FormControl fullWidth>
            <InputLabel id="users-label">
              {translate('notifications.createModal.usersField')}
            </InputLabel>
            <Select
              labelId="users-label"
              multiple
              value={selectedUsers}
              onChange={handleUsersChange}
              input={<OutlinedInput label={translate('notifications.createModal.usersField')} />}
              renderValue={(selected) => (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {selected.map((userId) => {
                    const user = users.find((u) => u.id === userId);
                    return (
                      <Chip key={userId} label={user?.name || userId} size="small" />
                    );
                  })}
                </Box>
              )}
              disabled={loadingUsers}
              endAdornment={loadingUsers ? <CircularProgress size={20} sx={{ mr: 2 }} /> : null}
            >
              {users.map((user) => (
                <MenuItem key={user.id} value={user.id}>
                  {user.name} {user.email && `(${user.email})`}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            size="small"
            onClick={handleSelectAll}
            disabled={loadingUsers || users.length === 0}
          >
            {selectedUsers.length === users.length
              ? translate('notifications.createModal.deselectAll')
              : translate('notifications.createModal.selectAll')}
          </Button>
          <Typography variant="caption" color="text.secondary">
            {translate('notifications.createModal.usersHint')}
          </Typography>
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={handleClose} disabled={loading}>
          {translate('button.cancel')}
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading || !title.trim() || !content.trim()}
        >
          {loading ? <CircularProgress size={20} /> : translate('button.create')}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default CreateNotificationModal;
