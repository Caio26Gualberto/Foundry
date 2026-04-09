import React, { useState, useRef, useCallback } from 'react';
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
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ROUTES } from '../utils/constants';
import { translate } from '../i18n';
import apiClient from '../services/apiClient';

const CODE_LENGTH = 6;

export const VerifyEmail: React.FC = () => {
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email') ?? '';

  const [digits, setDigits] = useState<string[]>(Array(CODE_LENGTH).fill(''));
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isResending, setIsResending] = useState(false);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  const focusInput = useCallback((index: number) => {
    inputRefs.current[index]?.focus();
  }, []);

  const handleDigitChange = (index: number, value: string) => {
    if (value.length > 1) {
      const pasted = value.replace(/\D/g, '').slice(0, CODE_LENGTH);
      if (pasted.length > 1) {
        const newDigits = [...digits];
        for (let i = 0; i < pasted.length && index + i < CODE_LENGTH; i++) {
          newDigits[index + i] = pasted[i];
        }
        setDigits(newDigits);
        const nextIndex = Math.min(index + pasted.length, CODE_LENGTH - 1);
        setTimeout(() => focusInput(nextIndex), 0);
        return;
      }
      value = value.slice(-1);
    }

    if (value && !/^\d$/.test(value)) return;

    const newDigits = [...digits];
    newDigits[index] = value;
    setDigits(newDigits);

    if (value && index < CODE_LENGTH - 1) {
      setTimeout(() => focusInput(index + 1), 0);
    }
  };

  const handleKeyDown = (index: number, e: React.KeyboardEvent) => {
    if (e.key === 'Backspace' && !digits[index] && index > 0) {
      const newDigits = [...digits];
      newDigits[index - 1] = '';
      setDigits(newDigits);
      setTimeout(() => focusInput(index - 1), 0);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const code = digits.join('');
    if (code.length < CODE_LENGTH) {
      const msg = translate('verifyEmail.validation.fillCode');
      setError(msg);
      enqueueSnackbar(msg, { variant: 'warning' });
      return;
    }

    try {
      setIsLoading(true);
      await apiClient.post('/Auth/VerifyEmail', { email, code }, { silent: true });
      enqueueSnackbar(translate('verifyEmail.success'), { variant: 'success' });
      navigate(ROUTES.LOGIN);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Erro ao verificar email';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const handleResend = async () => {
    try {
      setIsResending(true);
      await apiClient.post('/Auth/ResendVerificationCode', { email }, { silent: true });
      enqueueSnackbar(translate('verifyEmail.resendSuccess'), { variant: 'success' });
      setDigits(Array(CODE_LENGTH).fill(''));
      setError('');
      setTimeout(() => focusInput(0), 0);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Erro ao reenviar código';
      setError(errorMessage);
    } finally {
      setIsResending(false);
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
        <Card sx={{ width: '100%', maxWidth: 440 }}>
          <CardContent sx={{ p: 4 }}>
            <Box sx={{ textAlign: 'center', mb: 4 }}>
              <Typography variant="h4" component="h1" gutterBottom>
                Boilerplate
              </Typography>
              <Typography variant="h6" gutterBottom>
                {translate('verifyEmail.title')}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {translate('verifyEmail.description')}
              </Typography>
              {email && (
                <Typography variant="body2" fontWeight="bold" sx={{ mt: 0.5 }}>
                  {email}
                </Typography>
              )}
            </Box>

            {error && (
              <Alert severity="error" sx={{ mb: 3 }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit} noValidate>
              <Box sx={{ display: 'flex', gap: 1, justifyContent: 'center', mb: 3 }}>
                {digits.map((digit, index) => (
                  <TextField
                    key={index}
                    inputRef={(el) => { inputRefs.current[index] = el; }}
                    value={digit}
                    onChange={(e) => handleDigitChange(index, e.target.value)}
                    onKeyDown={(e) => handleKeyDown(index, e)}
                    inputProps={{
                      maxLength: CODE_LENGTH,
                      style: {
                        textAlign: 'center',
                        fontSize: '1.5rem',
                        fontWeight: 'bold',
                        padding: '12px 0',
                      },
                      inputMode: 'numeric',
                    }}
                    sx={{ width: 52 }}
                    disabled={isLoading}
                    autoFocus={index === 0}
                  />
                ))}
              </Box>

              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mb: 2, py: 1.5 }}
                disabled={isLoading}
              >
                {isLoading ? (
                  <CircularProgress size={24} color="inherit" />
                ) : (
                  translate('button.confirm')
                )}
              </Button>

              <Box sx={{ textAlign: 'center' }}>
                <Button
                  variant="text"
                  onClick={handleResend}
                  disabled={isResending}
                  size="small"
                >
                  {isResending ? (
                    <CircularProgress size={16} sx={{ mr: 1 }} />
                  ) : null}
                  {translate('verifyEmail.resend')}
                </Button>
              </Box>
            </Box>

            <Box sx={{ textAlign: 'center', mt: 3 }}>
              <Link
                component="button"
                variant="body2"
                onClick={() => navigate(ROUTES.LOGIN)}
                sx={{ cursor: 'pointer' }}
              >
                {translate('verifyEmail.backToLogin')}
              </Link>
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Container>
  );
};

export default VerifyEmail;
