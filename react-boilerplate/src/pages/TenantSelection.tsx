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
import { Business, CheckCircle } from '@mui/icons-material';
import { apiService } from '../services/api';
import type { Tenant } from '../types';
import { useAuth } from '../contexts/Auth';

export const TenantSelection: React.FC = () => {
  const { user, selectTenant, isLoading: authLoading } = useAuth();
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedTenantId, setSelectedTenantId] = useState<string | null>(null);

  useEffect(() => {
    const fetchTenants = async () => {
      try {
        const tenantsData = await apiService.getTenants();
        setTenants(tenantsData.data);
      } catch (err) {
        console.error('Error fetching tenants:', err);
        setError(err instanceof Error ? err.message : 'Erro ao carregar tenants');
        
        // Mock data for testing
        setTenants([
          {
            id: '1',
            name: 'Tenant Demo 1',
            address: {
              street: 'Rua Demo 1',
              city: 'Cidade Demo 1',
              state: 'Estado Demo 1',
              zipCode: '12345-678',
              country: 'Brasil',
              number: '123',
            },
          },
          {
            id: '2',
            name: 'Tenant Demo 2',
            address: {
              street: 'Rua Demo 2',
              city: 'Cidade Demo 2',
              state: 'Estado Demo 2',
              zipCode: '12345-678',
              country: 'Brasil',
              number: '123',
            },
          },
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchTenants();
  }, []);

  const handleTenantSelect = async (tenantId: string) => {
    try {
      setSelectedTenantId(tenantId);
      await selectTenant(tenantId);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao selecionar tenant');
      setSelectedTenantId(null);
    }
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
      </Box>
    </Container>
  );
};
