import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
  Container,
  InputAdornment,
  IconButton,
  Fade,
  Grow,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  CheckCircle,
  Email,
  Lock,
  Person,
} from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import { ROUTES } from '../utils/constants';
import apiClient from '../services/apiClient';
import type { AcceptInvitationData } from '../types/Users';
import { translate } from '../i18n';

export const AcceptInvitation: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const [formData, setFormData] = useState<AcceptInvitationData>({
    token: searchParams.get('token') || '',
    email: searchParams.get('email') || '',
    tenant: searchParams.get('tenant') || '',
    tenantId: searchParams.get('tenantId') || '',
    name: '',
    password: '',
    confirmPassword: '',
  });

  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [errors, setErrors] = useState<Partial<AcceptInvitationData>>({});
  const [tokenValid, setTokenValid] = useState<boolean | null>(null);

  useEffect(() => {
    const validateTokenOnMount = async () => {
      if (!formData.token || !formData.email || !formData.tenant || !formData.tenantId) {
        setTokenValid(false);
        return;
      }

      try {
        setLoading(true);
        const isValid = await apiClient.post<boolean>('/auth/validate-invitation-token', {
          token: formData.token,
          email: formData.email
        });

        setTokenValid(isValid);
      } catch (error) {
        console.error('Error validating token:', error);
        setTokenValid(false);
        enqueueSnackbar('Token inválido ou expirado', { variant: 'error' });
      } finally {
        setLoading(false);
      }
    };

    validateTokenOnMount();
  }, [formData.token, formData.email, formData.tenant, formData.tenantId, enqueueSnackbar]);


  const validateForm = (): boolean => {
    const newErrors: Partial<AcceptInvitationData> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Nome é obrigatório';
    }

    if (!formData.password) {
      newErrors.password = 'Senha é obrigatória';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Senha deve ter pelo menos 6 caracteres';
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Confirmação de senha é obrigatória';
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Senhas não coincidem';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) return;

    setLoading(true);
    try {
      const response = await apiClient.post<boolean>('/auth/acceptTenantInvite', {
        token: formData.token,
        email: formData.email,
        name: formData.name,
        password: formData.password,
        tenantId: formData.tenantId,
      });

      if (!response) {
        enqueueSnackbar('Erro ao aceitar convite', { variant: 'error' });
        return;
      }
      enqueueSnackbar('Convite aceito com sucesso! Redirecionando...', {
        variant: 'success'
      });

      setTimeout(() => {
        navigate(ROUTES.LOGIN);
      }, 2000);
    } catch (error) {
      console.error('Error accepting invitation:', error);
      const errorMessage = error instanceof Error ? error.message : 'Erro ao aceitar convite';
      enqueueSnackbar(errorMessage, { variant: 'error' });
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field: keyof AcceptInvitationData) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    setFormData(prev => ({
      ...prev,
      [field]: e.target.value
    }));

    // Limpar erro do campo quando usuário começar a digitar
    if (errors[field]) {
      setErrors(prev => ({
        ...prev,
        [field]: undefined
      }));
    }
  };

  if (tokenValid === null) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'linear-gradient(135deg, #718096 0%, #4a5568 100%)',
        }}
      >
        <CircularProgress size={60} sx={{ color: 'white' }} />
      </Box>
    );
  }

  if (tokenValid === false) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'linear-gradient(135deg, #718096 0%, #4a5568 100%)',
        }}
      >
        <Container maxWidth="sm">
          <Fade in timeout={1000}>
            <Paper
              elevation={24}
              sx={{
                p: 4,
                borderRadius: 3,
                textAlign: 'center',
                background: 'rgba(255, 255, 255, 0.95)',
                backdropFilter: 'blur(10px)',
              }}
            >
              <Typography variant="h4" color="error" gutterBottom>
                {translate('acceptInvitation.errors.invite')}
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
                {translate('acceptInvitation.errors.description')}
              </Typography>
              <Button
                variant="contained"
                onClick={() => navigate(ROUTES.LOGIN)}
                sx={{
                  background: 'linear-gradient(135deg, #718096 0%, #4a5568 100%)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #4a5568 0%, #2d3748 100%)',
                  },
                }}
              >
                {translate('acceptInvitation.login')}
              </Button>
            </Paper>
          </Fade>
        </Container>
      </Box>
    );
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #718096 0%, #4a5568 100%)',
        py: 4,
      }}
    >
      <Container maxWidth="sm">
        <Grow in timeout={1000}>
          <Paper
            elevation={24}
            sx={{
              p: 4,
              borderRadius: 3,
              background: 'rgba(255, 255, 255, 0.95)',
              backdropFilter: 'blur(10px)',
              border: '1px solid rgba(255, 255, 255, 0.2)',
            }}
          >
            <Box textAlign="center" sx={{ mb: 4 }}>
              <CheckCircle
                sx={{
                  fontSize: 60,
                  color: '#4a5568',
                  mb: 2,
                }}
              />
              <Typography variant="h4" gutterBottom sx={{ fontWeight: 600, color: '#2d3748' }}>
                {translate('acceptInvitation.title')}
              </Typography>
              <Typography variant="h6" gutterBottom sx={{ color: '#4a5568', mb: 2 }}>
                {translate('acceptInvitation.subtitle', { tenantName: formData.tenant })}
              </Typography>
              <Typography variant="body1" color="text.secondary">
                {translate('acceptInvitation.description')}
              </Typography>
            </Box>

            <Alert severity="info" sx={{ mb: 3, borderRadius: 2 }}>
              <Box display="flex" alignItems="center" gap={1}>
                <Email fontSize="small" />
                <Typography variant="body2">
                  {translate('acceptInvitation.invite', { email: formData.email })}
                </Typography>
              </Box>
            </Alert>

            <Box component="form" onSubmit={handleSubmit}>
              <TextField
                fullWidth
                label={translate('acceptInvitation.fields.name')}
                value={formData.name}
                onChange={handleInputChange('name')}
                error={!!errors.name}
                helperText={errors.name}
                required
                sx={{ mb: 3 }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Person color="action" />
                    </InputAdornment>
                  ),
                }}
              />

              <TextField
                fullWidth
                label={translate('acceptInvitation.fields.password')}
                type={showPassword ? 'text' : 'password'}
                value={formData.password}
                onChange={handleInputChange('password')}
                error={!!errors.password}
                helperText={errors.password}
                required
                sx={{ mb: 3 }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Lock color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              <TextField
                fullWidth
                label={translate('acceptInvitation.fields.confirmPassword')}
                type={showConfirmPassword ? 'text' : 'password'}
                value={formData.confirmPassword}
                onChange={handleInputChange('confirmPassword')}
                error={!!errors.confirmPassword}
                helperText={errors.confirmPassword}
                required
                sx={{ mb: 4 }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Lock color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        edge="end"
                      >
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={loading}
                sx={{
                  py: 1.5,
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  borderRadius: 2,
                  background: 'linear-gradient(135deg, #718096 0%, #4a5568 100%)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #4a5568 0%, #2d3748 100%)',
                    transform: 'translateY(-2px)',
                    boxShadow: '0 8px 25px rgba(74, 85, 104, 0.3)',
                  },
                  '&:disabled': {
                    background: 'rgba(0, 0, 0, 0.12)',
                  },
                  transition: 'all 0.3s ease',
                }}
              >
                {loading ? (
                  <Box display="flex" alignItems="center" gap={2}>
                    <CircularProgress size={20} color="inherit" />
                    {translate('acceptInvitation.processing')}
                  </Box>
                ) : (
                  translate('acceptInvitation.accept')
                )}
              </Button>

              <Box textAlign="center" sx={{ mt: 3 }}>
                <Button
                  variant="text"
                  onClick={() => navigate(ROUTES.LOGIN)}
                  sx={{ color: 'text.secondary' }}
                >
                  {translate('acceptInvitation.alreadyHaveAccount')}
                </Button>
              </Box>
            </Box>
          </Paper>
        </Grow>
      </Container>
    </Box>
  );
};

export default AcceptInvitation;
