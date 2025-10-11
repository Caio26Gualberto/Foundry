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
  Analytics,
  Security,
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

const QuickActionCard: React.FC<{
  title: string;
  description: string;
  icon: React.ReactNode;
  action: string;
  onClick: () => void;
}> = ({ title, description, icon, action, onClick }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
        <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
          {icon}
        </Avatar>
        <Typography variant="h6" component="div">
          {title}
        </Typography>
      </Box>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2, flex: 1 }}>
        {description}
      </Typography>
      <Button variant="outlined" onClick={onClick}>
        {action}
      </Button>
    </CardContent>
  </Card>
);

export const Dashboard: React.FC = () => {
  const { user } = useAuth();

  const handleQuickAction = (action: string) => {
    console.log('Quick action:', action);
    // TODO: Implement navigation or action logic
  };

  return (
    <Box>
      {/* Welcome Section */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
          Bem-vindo, {user?.userName}!
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
          <Typography variant="body1" color="text.secondary">
            Aqui está um resumo das suas atividades
          </Typography>
          {canAccessTenantSelection(user) && !user?.tenantId && (
            <Chip label="Modo Administrador Global" color="secondary" size="small" />
          )}
          {user?.tenantId && (
            <Chip label={`Tenant: ${user.tenantId}`} color="primary" size="small" />
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

      {/* Quick Actions */}
      <Typography variant="h5" component="h2" gutterBottom sx={{ fontWeight: 600, mb: 3 }}>
        Ações Rápidas
      </Typography>

      <Grid container spacing={3}>
        <Grid sx={{xs: 12, sm: 6, md: 4}}>
          <QuickActionCard
            title="Gerenciar Usuários"
            description="Adicione, edite ou remova usuários do sistema"
            icon={<People />}
            action="Gerenciar"
            onClick={() => handleQuickAction('users')}
          />
        </Grid>

        <Grid sx={{xs: 12, sm: 6, md: 4}}>
          <QuickActionCard
            title="Relatórios"
            description="Visualize relatórios detalhados e análises"
            icon={<Analytics />}
            action="Ver Relatórios"
            onClick={() => handleQuickAction('reports')}
          />
        </Grid>

        <Grid sx={{xs: 12, sm: 6, md: 4}}>
          <QuickActionCard
            title="Configurações de Segurança"
            description="Configure políticas de segurança e permissões"
            icon={<Security />}
            action="Configurar"
            onClick={() => handleQuickAction('security')}
          />
        </Grid>

        {canAccessTenantSelection(user) && (
          <Grid sx={{xs: 12, sm: 6, md: 4}}>
            <QuickActionCard
              title="Gerenciar Tenants"
              description="Administre tenants e suas configurações"
              icon={<Business />}
              action="Gerenciar Tenants"
              onClick={() => handleQuickAction('tenants')}
            />
          </Grid>
        )}
      </Grid>
    </Box>
  );
};
