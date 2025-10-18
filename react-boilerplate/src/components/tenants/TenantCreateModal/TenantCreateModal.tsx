import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Stack,
  Box,
  IconButton,
  Typography,
} from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import apiClient from '../../../services/apiClient';
import type { Tenant } from '../../../types/tenants';
import { translate } from '../../../i18n';

interface TenantCreateDto {
  name: string;
  address: {
    street: string;
    city: string;
    state: string;
    zipCode: string;
    country: string;
    number: string;
  };
}

interface TenantCreateModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: (tenant: Tenant) => void;
  editTenant?: Tenant | null;
}

export const TenantCreateModal: React.FC<TenantCreateModalProps> = ({
  open,
  onClose,
  onSuccess,
  editTenant,
}) => {
  const { enqueueSnackbar } = useSnackbar();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<TenantCreateDto>({
    name: '',
    address: {
      street: '',
      city: '',
      state: '',
      zipCode: '',
      country: 'Brasil',
      number: '',
    },
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  // Preenche o formulário quando um tenant é passado para edição
  useEffect(() => {
    if (editTenant) {
      setFormData({
        name: editTenant.name,
        address: {
          street: editTenant.address.street,
          city: editTenant.address.city,
          state: editTenant.address.state,
          zipCode: editTenant.address.zipCode,
          country: editTenant.address.country,
          number: editTenant.address.number,
        },
      });
    } else {
      // Reset form when not editing
      setFormData({
        name: '',
        address: {
          street: '',
          city: '',
          state: '',
          zipCode: '',
          country: 'Brasil',
          number: '',
        },
      });
    }
    setErrors({});
  }, [editTenant, open]);

  const handleInputChange = (field: string, value: string) => {
    if (field.startsWith('address.')) {
      const addressField = field.replace('address.', '');
      setFormData(prev => ({
        ...prev,
        address: {
          ...prev.address,
          [addressField]: value,
        },
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [field]: value,
      }));
    }

    // Limpa erro do campo quando o usuário começa a digitar
    if (errors[field]) {
      setErrors(prev => ({
        ...prev,
        [field]: '',
      }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Nome é obrigatório';
    }

    if (!formData.address.street.trim()) {
      newErrors['address.street'] = 'Rua é obrigatória';
    }

    if (!formData.address.number.trim()) {
      newErrors['address.number'] = 'Número é obrigatório';
    }

    if (!formData.address.city.trim()) {
      newErrors['address.city'] = 'Cidade é obrigatória';
    }

    if (!formData.address.state.trim()) {
      newErrors['address.state'] = 'Estado é obrigatório';
    }

    if (!formData.address.zipCode.trim()) {
      newErrors['address.zipCode'] = 'CEP é obrigatório';
    }

    if (!formData.address.country.trim()) {
      newErrors['address.country'] = 'País é obrigatório';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      let tenant: Tenant;
      if (editTenant) {
        tenant = await apiClient.put<Tenant>(`/tenant`, formData);
        enqueueSnackbar('Tenant atualizado com sucesso!', { variant: 'success' });
      } else {
        tenant = await apiClient.post<Tenant>('/tenant', formData);
        enqueueSnackbar('Tenant criado com sucesso!', { variant: 'success' });
      }
      onSuccess(tenant);
      handleClose();
    } catch (error) {
      console.error('Error saving tenant:', error);
      const errorMessage = error instanceof Error ? error.message : 
        editTenant ? 'Erro ao atualizar tenant' : 'Erro ao criar tenant';
      enqueueSnackbar(errorMessage, { variant: 'error' });
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setFormData({
      name: '',
      address: {
        street: '',
        city: '',
        state: '',
        zipCode: '',
        country: 'Brasil',
        number: '',
      },
    });
    setErrors({});
    onClose();
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 2 }
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6">{editTenant ? translate("tenantSelection.tenantModal.editTenant") : translate("tenantSelection.tenantModal.createTenant")}</Typography>
          <IconButton onClick={handleClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        <Stack spacing={3}>
          {/* Informações do Tenant */}
          <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 600 }}>
            {translate("tenantSelection.tenantModal.infoTenant")}
          </Typography>
          
          <TextField
            fullWidth
            label={translate("tenantSelection.tenantModal.nameTenant")}
            value={formData.name}
            onChange={(e) => handleInputChange('name', e.target.value)}
            error={!!errors.name}
            helperText={errors.name}
            required
          />

          {/* Endereço */}
          <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 600, mt: 2 }}>
            {translate("tenantSelection.tenantModal.address")}
          </Typography>

          <Stack direction="row" spacing={2}>
            <TextField
              fullWidth
              label={translate("tenantSelection.tenantModal.street")}
              value={formData.address.street}
              onChange={(e) => handleInputChange('address.street', e.target.value)}
              error={!!errors['address.street']}
              helperText={errors['address.street']}
              required
              sx={{ flex: 2 }}
            />
            <TextField
              label={translate("tenantSelection.tenantModal.number")}
              value={formData.address.number}
              onChange={(e) => handleInputChange('address.number', e.target.value)}
              error={!!errors['address.number']}
              helperText={errors['address.number']}
              required
              sx={{ flex: 1 }}
            />
          </Stack>

          <Stack direction="row" spacing={2}>
            <TextField
              fullWidth
              label={translate("tenantSelection.tenantModal.city")}
              value={formData.address.city}
              onChange={(e) => handleInputChange('address.city', e.target.value)}
              error={!!errors['address.city']}
              helperText={errors['address.city']}
              required
            />
            <TextField
              label={translate("tenantSelection.tenantModal.state")}
              value={formData.address.state}
              onChange={(e) => handleInputChange('address.state', e.target.value)}
              error={!!errors['address.state']}
              helperText={errors['address.state']}
              required
              sx={{ minWidth: 120 }}
            />
            <TextField
              label={translate("tenantSelection.tenantModal.zipCode")}
              value={formData.address.zipCode}
              onChange={(e) => handleInputChange('address.zipCode', e.target.value)}
              error={!!errors['address.zipCode']}
              helperText={errors['address.zipCode']}
              required
              sx={{ minWidth: 120 }}
            />
          </Stack>

          <TextField
            fullWidth
            label={translate("tenantSelection.tenantModal.country")}
            value={formData.address.country}
            onChange={(e) => handleInputChange('address.country', e.target.value)}
            error={!!errors['address.country']}
            helperText={errors['address.country']}
            required
          />
        </Stack>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={handleClose} disabled={loading}>
          {translate("button.cancel")}
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading}
        >
          {loading ? (editTenant ? translate("button.save") : translate("button.create")) : (editTenant ? translate("button.save") : translate("button.create"))}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default TenantCreateModal;
