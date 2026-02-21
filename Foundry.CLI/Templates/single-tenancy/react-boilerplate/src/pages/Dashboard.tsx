import React from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Avatar,
} from '@mui/material';
import {
  TrendingUp,
  People,
  Notifications,
} from '@mui/icons-material';
import { useAuth } from '../contexts/Auth';
import { translate } from '../i18n';

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
  const { user } = useAuth();

  return (
    <Box>
      {/* Welcome Section */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 600 }}>
          {translate('dashboard.title', { userName: user?.userName })}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
          <Typography variant="body1" color="text.secondary">
            {translate('dashboard.description')}
          </Typography>
        </Box>
      </Box>
      <Typography variant="h6" color="text.primary" sx={{ mb: 2 }}>
        {translate('dashboard.subtitle')}
      </Typography>

      {/* Statistics Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title="Active Users"
            value="1,234"
            icon={<People />}
            color="primary.main"
            change="+12% this month"
          />
        </Grid>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title="Revenue"
            value="$45.2K"
            icon={<TrendingUp />}
            color="success.main"
            change="+8% this month"
          />
        </Grid>
        <Grid sx={{xs: 12, sm: 6, md: 3}}>
          <StatCard
            title="Alerts"
            value="12"
            icon={<Notifications />}
            color="warning.main"
          />
        </Grid>
      </Grid>
    </Box>
  );
};
