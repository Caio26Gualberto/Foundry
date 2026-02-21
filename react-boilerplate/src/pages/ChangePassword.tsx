import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
  Container,
  CircularProgress,
  InputAdornment,
  IconButton,
} from '@mui/material';
import { Visibility, VisibilityOff, Lock, Email } from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import { ROUTES } from '../utils/constants';
import apiClient from '../services/apiClient';
import { translate } from '../i18n';

const useQuery = () => new URLSearchParams(useLocation().search);

export const ChangePassword: React.FC = () => {
  const query = useQuery();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const prefillEmail = query.get('email') || '';

  const [email, setEmail] = useState(prefillEmail);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  const validate = () => {
    if (!email) {
      setError('Email é obrigatório');
      return false;
    }
    if (!password || password.length < 6) {
      setError('A nova senha deve ter no mínimo 6 caracteres');
      return false;
    }
    if (password !== confirmPassword) {
      setError('As senhas não coincidem');
      return false;
    }
    setError('');
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      setIsSubmitting(true);
      const ok = await apiClient.post<boolean>('/Auth/ChangePassword', {
        email,
        password
      });
      if (!ok) throw new Error('Falha ao alterar a senha');

      enqueueSnackbar('Senha alterada com sucesso. Faça login novamente.', { variant: 'success' });
      navigate(ROUTES.LOGIN);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Erro ao alterar senha';
      setError(msg);
      enqueueSnackbar(msg, { variant: 'error' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Container component="main" maxWidth="sm">
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
          py: 4,
        }}
      >
        <Card sx={{ width: '100%', maxWidth: 480 }}>
          <CardContent sx={{ p: 4 }}>
            <Box sx={{ textAlign: 'center', mb: 3 }}>
              <Typography variant="h5" component="h1" gutterBottom>
                {translate('changePassword.title')}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {translate('changePassword.description')}
              </Typography>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mb: 3 }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit} noValidate>
              <TextField
                margin="normal"
                required
                fullWidth
                id="email"
                label={translate('changePassword.fields.email')}
                name="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Email fontSize="small" />
                    </InputAdornment>
                  ),
                }}
                disabled={isSubmitting}
              />

              <TextField
                margin="normal"
                required
                fullWidth
                name="password"
                label={translate('changePassword.fields.password')}
                type={showPassword ? 'text' : 'password'}
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Lock fontSize="small" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={() => setShowPassword(v => !v)} edge="end">
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                disabled={isSubmitting}
              />

              <TextField
                margin="normal"
                required
                fullWidth
                name="confirmPassword"
                label={translate('changePassword.fields.confirmPassword')}
                type={showConfirmPassword ? 'text' : 'password'}
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Lock fontSize="small" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={() => setShowConfirmPassword(v => !v)} edge="end">
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                disabled={isSubmitting}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2, py: 1.5 }}
                disabled={isSubmitting}
              >
                {isSubmitting ? (
                  <CircularProgress size={24} color="inherit" />
                ) : (
                  translate('changePassword.save')
                )}
              </Button>

              <Button fullWidth variant="text" onClick={() => navigate(ROUTES.LOGIN)}>
                {translate('changePassword.backToLogin')}
              </Button>
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Container>
  );
};

export default ChangePassword;
