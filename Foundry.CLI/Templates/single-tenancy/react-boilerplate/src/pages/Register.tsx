import React, { useState } from 'react';
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
  Link,
} from '@mui/material';
import { useSnackbar } from 'notistack';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../utils/constants';
import { translate } from '../i18n';
import apiClient from '../services/apiClient';
import type { RegisterResponseDto } from '../types';

export const Register: React.FC = () => {
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [nickname, setNickname] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!email || !nickname || !password || !confirmPassword) {
      const msg = translate('register.validation.fillAll');
      setError(msg);
      enqueueSnackbar(msg, { variant: 'warning' });
      return;
    }

    if (password.length < 6) {
      const msg = translate('register.validation.passwordMin');
      setError(msg);
      enqueueSnackbar(msg, { variant: 'warning' });
      return;
    }

    if (password !== confirmPassword) {
      const msg = translate('register.validation.passwordMismatch');
      setError(msg);
      enqueueSnackbar(msg, { variant: 'warning' });
      return;
    }

    try {
      setIsLoading(true);
      await apiClient.post<RegisterResponseDto>('/Auth/Register', {
        email,
        nickname,
        password,
      }, { silent: true });

      enqueueSnackbar(translate('register.success'), { variant: 'success' });
      navigate(`${ROUTES.VERIFY_EMAIL}?email=${encodeURIComponent(email)}`);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Erro ao criar conta';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
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
        <Card sx={{ width: '100%', maxWidth: 400 }}>
          <CardContent sx={{ p: 4 }}>
            <Box sx={{ textAlign: 'center', mb: 4 }}>
              <Typography variant="h4" component="h1" gutterBottom>
                Boilerplate
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {translate('register.title')}
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
                label={translate('register.fields.email')}
                name="email"
                autoComplete="email"
                autoFocus
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={isLoading}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                id="nickname"
                label={translate('register.fields.nickname')}
                name="nickname"
                autoComplete="name"
                value={nickname}
                onChange={(e) => setNickname(e.target.value)}
                disabled={isLoading}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                name="password"
                label={translate('register.fields.password')}
                type="password"
                id="password"
                autoComplete="new-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={isLoading}
              />
              <TextField
                margin="normal"
                required
                fullWidth
                name="confirmPassword"
                label={translate('register.fields.confirmPassword')}
                type="password"
                id="confirmPassword"
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                disabled={isLoading}
              />
              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2, py: 1.5 }}
                disabled={isLoading}
              >
                {isLoading ? (
                  <CircularProgress size={24} color="inherit" />
                ) : (
                  translate('button.create')
                )}
              </Button>
            </Box>

            <Box sx={{ textAlign: 'center', mt: 2 }}>
              <Typography variant="body2" color="text.secondary">
                {translate('register.hasAccount')}{' '}
                <Link
                  component="button"
                  variant="body2"
                  onClick={() => navigate(ROUTES.LOGIN)}
                  sx={{ cursor: 'pointer' }}
                >
                  {translate('register.login')}
                </Link>
              </Typography>
            </Box>
          </CardContent>
        </Card>

        <Typography variant="body2" color="text.secondary" sx={{ mt: 4 }}>
          {translate('login.rights')}
        </Typography>
      </Box>
    </Container>
  );
};

export default Register;
