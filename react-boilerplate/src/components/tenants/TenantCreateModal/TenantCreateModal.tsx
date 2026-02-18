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
  Stepper,
  Step,
  StepLabel,
} from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import apiClient from '../../../services/apiClient';
import type { Tenant } from '../../../types/tenants';
import { translate } from '../../../i18n';
import type { AcceptInvitationData } from '../../../types/Users';

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
  registerInput : AcceptInvitationData;
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
  const [activeStep, setActiveStep] = useState<number>(0);
  const isEdit = !!editTenant;
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
    registerInput: {
      name: '',
      email: '',
      password: '',
      confirmPassword: '',
      token: '',
      tenant: '',
      tenantId: '',
    },
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [successDialogOpen, setSuccessDialogOpen] = useState(false);
  const [createdTenant, setCreatedTenant] = useState<Tenant | null>(null);

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
        registerInput: {
          name: '',
          email: '',
          password: '',
          confirmPassword: '',
          token: '',
          tenant: '',
          tenantId: editTenant.id,
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
        registerInput: {
          name: '',
          email: '',
          password: '',
          confirmPassword: '',
          token: '',
          tenant: '',
          tenantId: '',
        },
      });
    }
    setErrors({});
    setActiveStep(0);
    setSuccessDialogOpen(false);
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

  const validateUserStep = (): boolean => {
    const newErrors: Record<string, string> = {};
    if (!formData.registerInput.name.trim()) {
      newErrors['registerInput.name'] = 'Nome do usuário é obrigatório';
    }
    if (!formData.registerInput.email.trim()) {
      newErrors['registerInput.email'] = 'Email é obrigatório';
    } else {
      const emailRegex = /[^@\s]+@[^@\s]+\.[^@\s]+/;
      if (!emailRegex.test(formData.registerInput.email)) {
        newErrors['registerInput.email'] = 'Email inválido';
      }
    }
    if (!formData.registerInput.password.trim()) {
      newErrors['registerInput.password'] = 'Gere uma senha para o usuário';
    }
    setErrors(prev => ({ ...prev, ...newErrors }));
    return Object.keys(newErrors).length === 0;
  };

  const generateSecurePassword = (length = 14) => {
    const upper = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const lower = 'abcdefghijklmnopqrstuvwxyz';
    const digits = '0123456789';
    const symbols = '!@#$%^&*()-_=+[]{};:,.<>?';
    const all = upper + lower + digits + symbols;

    const pick = (chars: string, rand: number) => chars[Math.floor(rand * chars.length)];
    const getRandomFloats = (n: number) => {
      const arr = new Uint32Array(n);
      (window.crypto || (window as any).msCrypto).getRandomValues(arr);
      return Array.from(arr, (x) => x / 2 ** 32);
    };

    const ensure = [
      pick(upper, getRandomFloats(1)[0]),
      pick(lower, getRandomFloats(1)[0]),
      pick(digits, getRandomFloats(1)[0]),
      pick(symbols, getRandomFloats(1)[0]),
    ];

    const remaining = length - ensure.length;
    const randoms = getRandomFloats(remaining);
    const rest = randoms.map((r) => pick(all, r));
    const pwd = [...ensure, ...rest];

    // shuffle using secure randomness
    const shuffleRand = getRandomFloats(pwd.length);
    for (let i = pwd.length - 1; i > 0; i--) {
      const j = Math.floor(shuffleRand[i] * (i + 1));
      [pwd[i], pwd[j]] = [pwd[j], pwd[i]];
    }

    const password = pwd.join('');
    setFormData(prev => ({
      ...prev,
      registerInput: {
        ...prev.registerInput,
        password,
        confirmPassword: password,
      },
    }));
    setErrors(prev => ({ ...prev, ['RegisterInput.password']: '' }));
  };

  const handleSubmit = async () => {
    // Edit flow: mantém comportamento original de uma etapa
    if (isEdit) {
      if (!validateForm()) return;
      setLoading(true);
      try {
        console.log(formData);
        const tenant = await apiClient.patch<Tenant>(`/tenant/${formData.registerInput.tenantId}`, formData);
        enqueueSnackbar('Tenant atualizado com sucesso!', { variant: 'success' });
        onSuccess(tenant);
        handleClose();
      } catch (error) {
        console.error('Error saving tenant:', error);
        const errorMessage = error instanceof Error ? error.message : 'Erro ao atualizar tenant';
        enqueueSnackbar(errorMessage, { variant: 'error' });
      } finally {
        setLoading(false);
      }
      return;
    }

    // Create flow em duas etapas
    if (activeStep === 0) {
      if (!validateForm()) return;
      setActiveStep(1);
      return;
    }

    if (!validateUserStep()) return;

    setLoading(true);
    try {
      const tenant = await apiClient.post<Tenant>('/tenant', { ...formData, registerInput: { ...formData.registerInput, tenantId: null, nickname: formData.registerInput.name } });
      enqueueSnackbar('Tenant criado com sucesso!', { variant: 'success' });
      // Guarda tenant e abre confirmação para copiar email/senha
      setCreatedTenant(tenant);
      setSuccessDialogOpen(true);
    } catch (error) {
      console.error('Error saving tenant:', error);
      const errorMessage = error instanceof Error ? error.message : 'Erro ao criar tenant';
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
      registerInput: {
        name: '',
        email: '',
        password: '',
        confirmPassword: '',
        token: '',
        tenant: '',
        tenantId: undefined,
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
        {!isEdit && (
          <Box sx={{ mb: 3 }}>
            <Stepper activeStep={activeStep} alternativeLabel>
              <Step>
                <StepLabel>{translate("tenantSelection.tenantModal.steps.tenant")}</StepLabel>
              </Step>
              <Step>
                <StepLabel>{translate("tenantSelection.tenantModal.steps.user")}</StepLabel>
              </Step>
            </Stepper>
          </Box>
        )}

        {activeStep === 0 || isEdit ? (
          <Stack spacing={3}>
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
        ) : (
          <Stack spacing={3}>
            <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 600 }}>
              {translate("tenantSelection.tenantModal.secondStep.title")}
            </Typography>
            <TextField
              fullWidth
              label={translate("tenantSelection.tenantModal.secondStep.name")}
              value={formData.registerInput.name}
              onChange={(e) => setFormData(prev => ({ ...prev, registerInput: { ...prev.registerInput, name: e.target.value } }))}
              error={!!errors['registerInput.name']}
              helperText={errors['registerInput.name']}
              required
            />
            <TextField
              fullWidth
              label={translate("tenantSelection.tenantModal.secondStep.email")}
              type="email"
              value={formData.registerInput.email}
              onChange={(e) => setFormData(prev => ({ ...prev, registerInput: { ...prev.registerInput, email: e.target.value } }))}
              error={!!errors['registerInput.email']}
              helperText={errors['registerInput.email']}
              required
            />
            <Stack direction="row" spacing={2} alignItems="flex-start">
              <TextField
                fullWidth
                label={translate("tenantSelection.tenantModal.secondStep.password")}
                value={formData.registerInput.password}
                InputProps={{ readOnly: true }}
                error={!!errors['registerInput.password']}
                helperText={errors['registerInput.password'] || translate("tenantSelection.tenantModal.secondStep.helpText")}
              />
              <Button variant="outlined" onClick={() => generateSecurePassword()} sx={{ whiteSpace: 'nowrap', height: 56 }}>
                {translate("tenantSelection.tenantModal.secondStep.generatePassword")}
              </Button>
            </Stack>
          </Stack>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={handleClose} disabled={loading}>
          {translate("button.cancel")}
        </Button>
        {!isEdit && activeStep === 1 && (
          <Button onClick={() => setActiveStep(0)} disabled={loading}>
            {translate("button.back")}
          </Button>
        )}
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading}
        >
          {isEdit ? translate("button.save") : activeStep === 0 ? translate("button.next") : translate("button.save")}
        </Button>
      </DialogActions>

      <Dialog open={successDialogOpen} onClose={() => { setSuccessDialogOpen(false); if (createdTenant) onSuccess(createdTenant); handleClose(); }} maxWidth="sm" fullWidth>
        <DialogTitle>{translate("tenantSelection.tenantModal.datashow.title")}</DialogTitle>
        <DialogContent dividers>
          <Stack spacing={2}>
            <Typography>{translate("tenantSelection.tenantModal.datashow.subtitle")}</Typography>
            <Stack direction="row" spacing={1} alignItems="center">
              <TextField fullWidth label={translate("tenantSelection.tenantModal.datashow.email")} value={formData.registerInput.email} InputProps={{ readOnly: true }} />
              <Button onClick={() => navigator.clipboard?.writeText(formData.registerInput.email)}>Copiar</Button>
            </Stack>
            <Stack direction="row" spacing={1} alignItems="center">
              <TextField fullWidth label={translate("tenantSelection.tenantModal.datashow.password")} value={formData.registerInput.password} InputProps={{ readOnly: true }} />
              <Button onClick={() => navigator.clipboard?.writeText(formData.registerInput.password)}>Copiar</Button>
            </Stack>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setSuccessDialogOpen(false); if (createdTenant) onSuccess(createdTenant); handleClose(); }} variant="contained">{translate("button.close")}</Button>
        </DialogActions>
      </Dialog>
    </Dialog>
  );
};

export default TenantCreateModal;
