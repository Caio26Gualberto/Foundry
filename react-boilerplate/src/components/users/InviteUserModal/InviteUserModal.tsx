import React, { useState } from 'react';
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
} from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import apiClient from '../../../services/apiClient';
import { useAuth } from '../../../contexts/Auth';
import { translate } from '../../../i18n';

interface InviteUserModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}

export const InviteUserModal: React.FC<InviteUserModalProps> = ({
  open,
  onClose,
  onSuccess,
}) => {
  const { enqueueSnackbar } = useSnackbar();
  const { user } = useAuth();
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const validateEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleSubmit = async () => {
    if (!email.trim()) {
      setError('Email é obrigatório');
      return;
    }

    if (!validateEmail(email)) {
      setError('Email inválido');
      return;
    }

    setLoading(true);
    try {
      await apiClient.post('/tenant/invite', { email, tenantId: user?.tenantId });
      enqueueSnackbar('Convite enviado com sucesso!', { variant: 'success' });
      onSuccess?.();
      handleClose();
    } catch (error) {
      console.error('Error sending invite:', error);
      const errorMessage = error instanceof Error ? error.message : 'Erro ao enviar convite';
      enqueueSnackbar(errorMessage, { variant: 'error' });
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setEmail('');
    setError('');
    onClose();
  };

  const handleEmailChange = (value: string) => {
    setEmail(value);
    if (error) {
      setError('');
    }
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 2 }
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6">{translate('sidebar.inviteUser.title')}</Typography>
          <IconButton onClick={handleClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        <Box sx={{ py: 2 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            {translate('sidebar.inviteUser.description')}
          </Typography>
          
          <TextField
            fullWidth
            label={translate('sidebar.inviteUser.label')}
            type="email"
            value={email}
            onChange={(e) => handleEmailChange(e.target.value)}
            error={!!error}
            helperText={error}
            required
            autoFocus
            placeholder="exemplo@email.com"
          />
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={handleClose} disabled={loading}>
          Cancelar
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading || !email.trim()}
        >
          {loading ? 'Enviando...' : 'Enviar Convite'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default InviteUserModal;
