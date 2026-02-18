import React, { useState, useEffect } from "react";
import { 
  Box, 
  Typography, 
  Container, 
  Tabs, 
  Tab, 
  Paper,
  Button,
  Chip,
  Card,
  CardContent
} from "@mui/material";
import { 
  Settings, 
  People, 
  PersonAdd,
  CheckCircle,
  Cancel,
  Schedule,
  Notifications,
  Add
} from "@mui/icons-material";
import { useSnackbar } from "notistack";
import type { GridColDef, GridRowId } from "@mui/x-data-grid";
import { useAuth } from "../contexts/Auth";
import BoilerplateDataGrid from "../components/common/BoilerplateDataGrid";
import InviteUserModal from "../components/users/InviteUserModal/InviteUserModal";
import { useConfirmation } from "../contexts/confirmationContext/ConfirmationProvider";
import { translate } from "../i18n";
import apiClient from "../services/apiClient";
import type { InviteData } from "../types/Users";
import type { TenantNotificationDto } from "../types/systemNotifications";
import CreateNotificationModal from "../components/notifications/CreateNotificationModal/CreateNotificationModal";

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`tenant-tabpanel-${index}`}
      aria-labelledby={`tenant-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

export const TenantSettings: React.FC = () => {
  const { user } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const confirm = useConfirmation();
  const [tabValue, setTabValue] = useState(0);
  const [invites, setInvites] = useState<InviteData[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [inviteModalOpen, setInviteModalOpen] = useState(false);
  const [notifications, setNotifications] = useState<TenantNotificationDto[]>([]);
  const [notificationsLoading, setNotificationsLoading] = useState(false);
  const [notificationsError, setNotificationsError] = useState("");
  const [createNotificationModalOpen, setCreateNotificationModalOpen] = useState(false);

  useEffect(() => {
    if (tabValue === 1) {
      fetchInvites();
    }
    if (tabValue === 2) {
      fetchNotifications();
    }
  }, [tabValue]);

  const fetchInvites = async () => {
    try {
      setLoading(true);
      const invitesData = await apiClient.get<InviteData[]>("/user/GetInvites");
      setInvites(invitesData);
      setError("");
    } catch (err) {
      console.error("Error fetching invites:", err);
      const errorMessage =
        err instanceof Error ? err.message : "Erro ao carregar convites";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleInviteUser = () => {
    setInviteModalOpen(true);
  };

  const handleInviteSuccess = () => {
    fetchInvites();
    setInviteModalOpen(false);
  };

  const fetchNotifications = async () => {
    try {
      setNotificationsLoading(true);
      const data = await apiClient.get<TenantNotificationDto[]>("/systemNotification/tenant");
      setNotifications(data);
      setNotificationsError("");
    } catch (err) {
      console.error("Error fetching notifications:", err);
      const errorMessage = err instanceof Error ? err.message : "Erro ao carregar notificações";
      setNotificationsError(errorMessage);
    } finally {
      setNotificationsLoading(false);
    }
  };

  const handleCreateNotification = () => {
    setCreateNotificationModalOpen(true);
  };

  const handleCreateNotificationSuccess = () => {
    fetchNotifications();
    setCreateNotificationModalOpen(false);
  };

  const handleCancelInvite = async (_id: GridRowId) => {
    const result = await confirm({
      title: translate("tenantSettings.warnings.deleteInviteTitle"),
      message: translate("tenantSettings.warnings.deleteInvite"),
    });
    if (!result) return;
    // TODO: Implementar chamada real da API
    try {
      // await apiClient.delete(`/tenant/invites/${id}`);
      fetchInvites();
      enqueueSnackbar("Convite cancelado com sucesso", {
        variant: "success",
      });
    } catch (error) {
      console.error("Error canceling invite:", error);
      const errorMessage =
        error instanceof Error ? error.message : "Erro ao cancelar convite";
      enqueueSnackbar(errorMessage, { variant: "error" });
    }
  };

  // const handleResendInvite = async (id: GridRowId) => {
  //   try {
  //     await apiClient.post(`/tenant/invites/${id}/resend`);
  //     fetchInvites();
  //     enqueueSnackbar("Convite reenviado com sucesso", {
  //       variant: "success",
  //     });
  //   } catch (error) {
  //     console.error("Error resending invite:", error);
  //     enqueueSnackbar("Erro ao reenviar convite", { variant: "error" });
  //   }
  // };

  const getStatusChip = (status: string) => {
    const statusConfig = {
      pending: { label: translate("tenantSettings.tenantTabs.usersManagement.inviteStatus.pending"), color: "warning" as const, icon: <Schedule /> },
      accepted: { label: translate("tenantSettings.tenantTabs.usersManagement.inviteStatus.accepted"), color: "success" as const, icon: <CheckCircle /> },
      expired: { label: translate("tenantSettings.tenantTabs.usersManagement.inviteStatus.expired"), color: "error" as const, icon: <Cancel /> },
      cancelled: { label: translate("tenantSettings.tenantTabs.usersManagement.inviteStatus.cancelled"), color: "default" as const, icon: <Cancel /> },
    };

    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig.pending;
    
    return (
      <Chip 
        label={config.label} 
        color={config.color} 
        size="small" 
        icon={config.icon}
      />
    );
  };

  const notificationColumns: GridColDef[] = [
    {
      field: "title",
      headerName: translate("tenantSettings.tenantTabs.systemNotifications.columns.title"),
      flex: 1,
      minWidth: 200,
    },
    {
      field: "content",
      headerName: translate("tenantSettings.tenantTabs.systemNotifications.columns.content"),
      flex: 2,
      minWidth: 300,
    },
    {
      field: "usersCount",
      headerName: translate("tenantSettings.tenantTabs.systemNotifications.columns.usersCount"),
      width: 150,
    },
    {
      field: "createdAt",
      headerName: translate("tenantSettings.tenantTabs.systemNotifications.columns.createdAt"),
      width: 150,
      valueFormatter: (value: any) => {
        return new Date(value).toLocaleDateString('pt-BR');
      },
    },
  ];

  const inviteColumns: GridColDef[] = [
    {
      field: "email",
      headerName: translate("tenantSettings.tenantTabs.usersManagement.columns.email"),
      flex: 1,
      minWidth: 250,
    },
    {
      field: "status",
      headerName: translate("tenantSettings.tenantTabs.usersManagement.columns.status"),
      width: 130,
      renderCell: (params) => getStatusChip(params.value),
    },
    {
      field: "sentAt",
      headerName: translate("tenantSettings.tenantTabs.usersManagement.columns.sentAt"),
      width: 150,
      valueFormatter: (value: any) => {
        return new Date(value).toLocaleDateString('pt-BR');
      },
    },
    {
      field: "expiresAt",
      headerName: translate("tenantSettings.tenantTabs.usersManagement.columns.expiresAt"),
      width: 150,
      valueFormatter: (value: any) => {
        return new Date(value).toLocaleDateString('pt-BR');
      },
    },
  ];

  const inviteRows = invites.map((invite: InviteData) => ({
    id: invite.id,
    email: invite.email,
    status: invite.status,
    sentAt: invite.sendedAt,
    expiresAt: invite.expirationTime,
  }));

  const getInviteStats = () => {
    const stats = {
      total: invites.length,
      pending: invites.filter(i => i.status.toLowerCase() === 'pending').length,
      accepted: invites.filter(i => i.status.toLowerCase() === 'accepted').length,
      expired: invites.filter(i => i.status.toLowerCase() === 'expired').length,
    };
    return stats;
  };

  const stats = getInviteStats();

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            {translate("tenantSettings.title")}
          </Typography>
          <Typography variant="body1" color="text.secondary">
            {translate("tenantSettings.description")}
          </Typography>
          {user?.tenantName && (
            <Box sx={{ mt: 2 }}>
              <Chip 
                label={`Tenant: ${user.tenantName}`} 
                color="primary" 
                size="small" 
                icon={<Settings />}
              />
            </Box>
          )}
        </Box>

        <Paper sx={{ width: '100%', p: 1 }}>
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs value={tabValue} onChange={handleTabChange}>
              <Tab 
                label={translate("tenantSettings.tenantTabs.generalSettings.tabTitle")} 
                icon={<Settings />} 
                iconPosition="start"
              />
              <Tab 
                label={translate("tenantSettings.tenantTabs.usersManagement.tabTitle")} 
                icon={<People />} 
                iconPosition="start"
              />
              <Tab 
                label={translate("tenantSettings.tenantTabs.systemNotifications.tabTitle")} 
                icon={<Notifications />} 
                iconPosition="start"
              />
            </Tabs>
          </Box>

          <TabPanel value={tabValue} index={0}>
            <Typography variant="h6" gutterBottom>
              {translate("tenantSettings.tenantTabs.generalSettings.tabTitle")}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {translate("tenantSettings.tenantTabs.generalSettings.description")}
            </Typography>
          </TabPanel>

          <TabPanel value={tabValue} index={1}>
            <Box sx={{ mb: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6">
                  {translate("tenantSettings.tenantTabs.usersManagement.subtitle")}
                </Typography>
                <Button
                  variant="contained"
                  startIcon={<PersonAdd />}
                  onClick={handleInviteUser}
                >
                  {translate("tenantSettings.tenantTabs.usersManagement.inviteButton")}
                </Button>
              </Box>

              {/* Estatísticas dos convites */}
              <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
                <Card sx={{ flex: '1 1 200px', minWidth: 200 }}>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="primary">
                      {stats.total}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {translate("tenantSettings.tenantTabs.usersManagement.cards.totalInvites")}
                    </Typography>
                  </CardContent>
                </Card>
                <Card sx={{ flex: '1 1 200px', minWidth: 200 }}>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="warning.main">
                      {stats.pending}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {translate("tenantSettings.tenantTabs.usersManagement.cards.pendingInvites")}
                    </Typography>
                  </CardContent>
                </Card>
                <Card sx={{ flex: '1 1 200px', minWidth: 200 }}>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="success.main">
                      {stats.accepted}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {translate("tenantSettings.tenantTabs.usersManagement.cards.acceptedInvites")}
                    </Typography>
                  </CardContent>
                </Card>
                <Card sx={{ flex: '1 1 200px', minWidth: 200 }}>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="error.main">
                      {stats.expired}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {translate("tenantSettings.tenantTabs.usersManagement.cards.expiredInvites")}
                    </Typography>
                  </CardContent>
                </Card>
              </Box>

              <BoilerplateDataGrid
                title={translate("tenantSettings.tenantTabs.usersManagement.subtitle")}
                rows={inviteRows}
                columns={inviteColumns}
                loading={loading}
                error={error}
                onDelete={handleCancelInvite}
                height={400}
                pageSize={10}
                elevation={0}
              />
            </Box>
          </TabPanel>

          <TabPanel value={tabValue} index={2}>
            <Box sx={{ mb: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6">
                  {translate("tenantSettings.tenantTabs.systemNotifications.subtitle")}
                </Typography>
                <Button
                  variant="contained"
                  startIcon={<Add />}
                  onClick={handleCreateNotification}
                >
                  {translate("tenantSettings.tenantTabs.systemNotifications.createButton")}
                </Button>
              </Box>

              <BoilerplateDataGrid
                title={translate("tenantSettings.tenantTabs.systemNotifications.gridTitle")}
                rows={notifications.map((n) => ({
                  id: n.id,
                  title: n.title,
                  content: n.content,
                  createdAt: n.createdAt,
                  usersCount: n.usersCount,
                }))}
                columns={notificationColumns}
                loading={notificationsLoading}
                error={notificationsError}
                height={400}
                pageSize={10}
                elevation={0}
              />
            </Box>
          </TabPanel>
        </Paper>

        <InviteUserModal
          open={inviteModalOpen}
          onClose={() => setInviteModalOpen(false)}
          onSuccess={handleInviteSuccess}
        />

        <CreateNotificationModal
          open={createNotificationModalOpen}
          onClose={() => setCreateNotificationModalOpen(false)}
          onSuccess={handleCreateNotificationSuccess}
        />
      </Box>
    </Container>
  );
};

export default TenantSettings;
