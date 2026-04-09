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
import { useAuth } from '../contexts/Auth';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../utils/constants';
import { translate } from '../i18n';
import apiClient from '../services/apiClient';

export const Login: React.FC = () => {
  const { login, isLoading } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!email || !password) {
      const errorMessage = 'Por favor, preencha todos os campos';
      setError(errorMessage);
      enqueueSnackbar(errorMessage, { variant: 'warning' });
      return;
    }

    try {
      const { isNeededChangePassword } = await login(email, password);
      if (isNeededChangePassword) {
        navigate(`${ROUTES.CHANGE_PASSWORD}?email=${encodeURIComponent(email)}`);
      }
    } catch (err: unknown) {
      const axiosResponse = (err as { response?: { data?: { message?: string } } })?.response?.data;
      const errorMessage = axiosResponse?.message
        || (err instanceof Error ? err.message : 'Erro ao fazer login');

      if (errorMessage.toLowerCase().includes('email not confirmed')) {
        await apiClient.post('/Auth/ResendVerificationCode', { email }, { silent: true }).catch(() => {});
        navigate(`${ROUTES.VERIFY_EMAIL}?email=${encodeURIComponent(email)}`);
        return;
      }

      setError(errorMessage);
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
                {translate('login.title')}
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
                label={translate('login.fields.email')}
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
                name="password"
                label={translate('login.fields.password')}
                type="password"
                id="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
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
                  translate('button.enter')
                )}
              </Button>
            </Box>

            <Box sx={{ textAlign: 'center', mt: 2 }}>
              <Typography variant="body2" color="text.secondary">
                {translate('login.noAccount')}{' '}
                <Link
                  component="button"
                  variant="body2"
                  onClick={() => navigate(ROUTES.REGISTER)}
                  sx={{ cursor: 'pointer' }}
                >
                  {translate('login.register')}
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
