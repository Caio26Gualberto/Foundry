import React from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Button,
  Avatar,
  Chip,
} from '@mui/material';
import {
  TrendingUp,
  People,
  Business,
  Notifications,
} from '@mui/icons-material';
import { useAuth } from '../contexts/Auth';
import { canAccessTenantSelection } from '../utils/authHelpers';

const StatCard: React.FC<{
  title: string;
  value: string;
  icon: React.ReactNode;
  color: string;
  change?: string;
}> = ({ title, value, icon, color, change }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box>
          <Typography color="text.secondary" gutterBottom variant="overline">
            {title}
          </Typography>
          <Typography variant="h4" component="div" sx={{ fontWeight: 600 }}>
            {value}
          </Typography>
          {change && (
            <Typography variant="body2" sx={{ color: 'success.main', mt: 1 }}>
              {change}
            </Typography>
          )}
        </Box>
        <Avatar sx={{ bgcolor: color, width: 56, height: 56 }}>
          {icon}
        </Avatar>
      </Box>
    </CardContent>
  </Card>
);

export const Dashboard: React.FC = () => {
  const { user, stopImpersonation } = useAuth();

  return (
    <Box>
      {/* Welcome Section */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
          Bem-vindo, {user?.userName}!
          {user?.impersonatedBy && (
            <Typography variant="body2" color="warning.main" sx={{ fontWeight: 400, mt: 0.5 }}>
              (Impersonado por usuário ID: {user.impersonatedBy})
            </Typography>
          )}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
          <Typography variant="body1" color="text.secondary">
            Aqui está um resumo das suas atividades
          </Typography>
          {canAccessTenantSelection(user) && !user?.tenantId && (
            <Chip label="Modo Administrador Global" color="secondary" size="small" />
          )}
          {user?.tenantId && (
            <Chip 
              label={`Tenant ID: ${user.tenantId}`} 
              color="primary" 
              size="small" 
            />
          )}
          {localStorage.getItem('Boilerplate_impersonated_token') && (
            <>
              <Chip label="Modo Impersonação" color="warning" size="small" />
              <Button 
                variant="outlined" 
                size="small" 
                color="warning"
                onClick={stopImpersonation}
                sx={{ ml: 1 }}
              >
                Parar Impersonação
              </Button>
            </>
          )}
        </Box>
      </Box>

      {/* Statistics Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title="Usuários Ativos"
            value="1,234"
            icon={<People />}
            color="primary.main"
            change="+12% este mês"
          />
        </Grid>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title="Receita"
            value="R$ 45.2K"
            icon={<TrendingUp />}
            color="success.main"
            change="+8% este mês"
          />
        </Grid>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title="Tenants"
            value="56"
            icon={<Business />}
            color="info.main"
            change="+3 novos"
          />
        </Grid>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title="Alertas"
            value="12"
            icon={<Notifications />}
            color="warning.main"
          />
        </Grid>
      </Grid>
    </Box>
  );
};
