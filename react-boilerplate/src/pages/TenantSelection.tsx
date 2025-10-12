import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Container,
  Grid,
  Button,
  Chip,
  CircularProgress,
  Alert,
  Avatar,
} from '@mui/material';
import { Business, CheckCircle, Dashboard } from '@mui/icons-material';
import { useSnackbar } from 'notistack';
import { useNavigate } from 'react-router-dom';
import type { Tenant } from '../types';
import { useAuth } from '../contexts/Auth';
import { ROUTES } from '../utils/constants';
import apiClient from '../services/apiClient';

export const TenantSelection: React.FC = () => {
  const { user, selectTenant, isLoading: authLoading } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedTenantId, setSelectedTenantId] = useState<string | null>(null);

  useEffect(() => {
    const fetchTenants = async () => {
      try {
        const tenantsData = await apiClient.get<Tenant[]>('/tenant', { silent: false });
        setTenants(tenantsData);
      } catch (err) {
        console.error('Error fetching tenants:', err);
        const errorMessage = err instanceof Error ? err.message : 'Erro ao carregar tenants';
        setError(errorMessage);
        enqueueSnackbar(errorMessage, { variant: 'error' });
      } finally {
        setLoading(false);
      }
    };

    fetchTenants();
  }, [enqueueSnackbar]);

  const handleTenantSelect = async (tenantId: string) => {
    try {
      setSelectedTenantId(tenantId);
      await selectTenant(tenantId);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao selecionar tenant');
      setSelectedTenantId(null);
    }
  };

  const handleSkipToDashboard = () => {
    // Armazena temporariamente a permissão para acessar dashboard sem tenant
    sessionStorage.setItem('allowDashboardAccess', 'true');
    enqueueSnackbar('Acessando dashboard sem selecionar tenant', { variant: 'info' });
    navigate(ROUTES.DASHBOARD);
  };

  if (loading) {
    return (
      <Container maxWidth="md">
        <Box
          sx={{
            minHeight: '100vh',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
          }}
        >
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="md">
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          py: 4,
        }}
      >
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            Selecionar Tenant
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Olá, {user?.userName}! Selecione um tenant para personificar:
          </Typography>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        <Grid container spacing={3}>
          {tenants.map((tenant) => (
            <Grid sx={{xs: 12, sm: 6, md: 4}} key={tenant.id}>
              <Card
                sx={{
                  cursor: 'pointer',
                  transition: 'all 0.2s',
                  '&:hover': {
                    transform: 'translateY(-2px)',
                    boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
                  },
                }}
                onClick={() => handleTenantSelect(tenant.id)}
              >
                <CardContent sx={{ textAlign: 'center', p: 3 }}>
                  <Avatar
                    sx={{
                      width: 64,
                      height: 64,
                      mx: 'auto',
                      mb: 2,
                      bgcolor: 'primary.main',
                    }}
                  >
                    <Business fontSize="large" />
                  </Avatar>

                  <Typography variant="h6" gutterBottom>
                    {tenant.name}
                  </Typography>

                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    {tenant.address.city}
                  </Typography>

                  <Box sx={{ mt: 2, mb: 2 }}>
                    <Chip
                      label="Ativo"
                      color="success"
                      size="small"
                    />
                  </Box>

                  {(
                    <Button
                      variant="contained"
                      fullWidth
                      disabled={authLoading || selectedTenantId === tenant.id}
                      startIcon={
                        selectedTenantId === tenant.id ? (
                          <CircularProgress size={16} color="inherit" />
                        ) : (
                          <CheckCircle />
                        )
                      }
                    >
                      {selectedTenantId === tenant.id ? 'Selecionando...' : 'Selecionar'}
                    </Button>
                  )}
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        {tenants.length === 0 && (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Typography variant="body1" color="text.secondary">
              Nenhum tenant disponível
            </Typography>
          </Box>
        )}

        {/* Botão para acessar dashboard sem selecionar tenant */}
        <Box sx={{ textAlign: 'center', mt: 4 }}>
          <Button
            variant="outlined"
            size="large"
            startIcon={<Dashboard />}
            onClick={handleSkipToDashboard}
            disabled={authLoading}
            sx={{
              minWidth: 200,
              borderColor: 'primary.main',
              color: 'primary.main',
              '&:hover': {
                borderColor: 'primary.dark',
                backgroundColor: 'primary.main',
                color: 'white',
              },
            }}
          >
            Acessar Dashboard
          </Button>
          <Typography variant="caption" display="block" sx={{ mt: 1, color: 'text.secondary' }}>
            Você pode selecionar um tenant mais tarde
          </Typography>
        </Box>
      </Box>
    </Container>
  );
};
