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
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import apiClient from '../../../services/apiClient';
import type { UserDto } from '../../../types/Users';
import { USER_ROLES } from '../../../utils/constants';

interface EditUserModalProps {
  open: boolean;
  user: UserDto | null;
  onClose: () => void;
  onSuccess?: () => void;
}

export const EditUserModal: React.FC<EditUserModalProps> = ({
  open,
  user,
  onClose,
  onSuccess,
}) => {
  const { enqueueSnackbar } = useSnackbar();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [roles, setRoles] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<{ name?: string; email?: string }>({});

  useEffect(() => {
    if (user) {
      setName(user.name || '');
      setEmail(user.email || '');
      setRoles(user.roles || []);
    }
  }, [user]);

  const validateEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const validate = (): boolean => {
    const newErrors: { name?: string; email?: string } = {};

    if (!name.trim()) {
      newErrors.name = 'Nome é obrigatório';
    }

    if (!email.trim()) {
      newErrors.email = 'Email é obrigatório';
    } else if (!validateEmail(email)) {
      newErrors.email = 'Email inválido';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate() || !user) return;

    setLoading(true);
    try {
      await apiClient.patch(`/User/${user.id}`, { name, email, roles });
      enqueueSnackbar('Usuário atualizado com sucesso!', { variant: 'success' });
      onSuccess?.();
      handleClose();
    } catch (error) {
      console.error('Error updating user:', error);
      const errorMessage =
        error instanceof Error ? error.message : 'Erro ao atualizar usuário';
      enqueueSnackbar(errorMessage, { variant: 'error' });
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setErrors({});
    setName('');
    setEmail('');
    setRoles([]);
    onClose();
  };

  const handleFieldChange = (field: 'name' | 'email', value: string) => {
    if (field === 'name') setName(value);
    else setEmail(value);

    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const handleRolesChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value;
    setRoles(typeof value === 'string' ? value.split(',') : value);
  };

  const availableRoles = [USER_ROLES.USER, USER_ROLES.TENANT_ADMIN];

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
          <Typography variant="h6">Editar Usuário</Typography>
          <IconButton onClick={handleClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        <Box sx={{ py: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            fullWidth
            label="Nome"
            value={name}
            onChange={(e) => handleFieldChange('name', e.target.value)}
            error={!!errors.name}
            helperText={errors.name}
            required
            autoFocus
          />
          <TextField
            fullWidth
            label="Email"
            type="email"
            value={email}
            onChange={(e) => handleFieldChange('email', e.target.value)}
            error={!!errors.email}
            helperText={errors.email}
            required
            placeholder="exemplo@email.com"
          />
          <FormControl fullWidth>
            <InputLabel id="roles-label">Roles</InputLabel>
            <Select
              labelId="roles-label"
              multiple
              value={roles}
              onChange={handleRolesChange}
              input={<OutlinedInput label="Roles" />}
              renderValue={(selected) => (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {selected.map((value) => (
                    <Chip key={value} label={value} size="small" />
                  ))}
                </Box>
              )}
            >
              {availableRoles.map((role) => (
                <MenuItem key={role} value={role}>
                  {role}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={handleClose} disabled={loading}>
          Cancelar
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading || !name.trim() || !email.trim()}
        >
          {loading ? 'Salvando...' : 'Salvar'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default EditUserModal;
