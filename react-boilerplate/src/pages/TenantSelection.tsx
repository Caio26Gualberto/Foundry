import React, { useState, useEffect } from "react";
import { Box, Typography, Container, Button, Chip } from "@mui/material";
import { Dashboard } from "@mui/icons-material";
import { useSnackbar } from "notistack";
import { useNavigate } from "react-router-dom";
import type { GridColDef, GridRowParams, GridRowId } from "@mui/x-data-grid";
import type { Tenant, address } from "../types/tenants";
import { useAuth } from "../contexts/Auth";
import { ROUTES } from "../utils/constants";
import apiClient from "../services/apiClient";
import BoilerplateDataGrid from "../components/common/BoilerplateDataGrid";
import TenantCreateModal from "../components/tenants/TenantCreateModal/TenantCreateModal";
import TenantUsersModal from "../components/tenants/TenantUsersModal/TenantUsersModal";
import { useConfirmation } from "../contexts/confirmationContext/ConfirmationProvider";

export const TenantSelection: React.FC = () => {
  const { user, isLoading: authLoading } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editingTenant, setEditingTenant] = useState<Tenant | null>(null);
  const [selectedTenant, setSelectedTenant] = useState<Tenant | null>(null);
  const [showUsers, setShowUsers] = useState(false);
  const confirm = useConfirmation();

  useEffect(() => {
    fetchTenants();
  }, []);

  const fetchTenants = async () => {
    try {
      setLoading(true);
      const tenantsData = await apiClient.get<Tenant[]>("/tenant");
      setTenants(tenantsData);
      setError("");
    } catch (err) {
      console.error("Error fetching tenants:", err);
      const errorMessage =
        err instanceof Error ? err.message : "Erro ao carregar tenants";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleSkipToDashboard = () => {
    sessionStorage.setItem("allowDashboardAccess", "true");
    enqueueSnackbar("Acessando dashboard sem selecionar tenant", {
      variant: "info",
    });
    navigate(ROUTES.DASHBOARD);
  };

  const handleRowClick = (params: GridRowParams) => {
    const tenant = tenants.find((t) => t.id === params.id);
    if (tenant) {
      setSelectedTenant(tenant);
      setShowUsers(true);
    }
  };

  const handleCreateTenant = () => {
    setCreateModalOpen(true);
  };

  const handleTenantCreated = () => {
    fetchTenants();
    setCreateModalOpen(false);
    setEditingTenant(null);
  };

  const handleModalClose = () => {
    setCreateModalOpen(false);
    setEditingTenant(null);
  };

  const handleUsersModalClose = () => {
    setShowUsers(false);
    setSelectedTenant(null);
  };

  const handleEditTenant = (id: GridRowId) => {
    const tenant = tenants.find((t) => t.id === id);
    if (tenant) {
      setEditingTenant(tenant);
      setCreateModalOpen(true);
    }
  };

  const handleDeleteTenant = async () => {
    const result = await confirm({
      title: "Excluir Tenant",
      message: "Tem certeza que deseja excluir este tenant?",
    });
    if (!result) return;
    await apiClient.delete(`/tenant`);
    fetchTenants();
    enqueueSnackbar("Tenant excluído com sucesso", {
      variant: "success",
    });
  };

  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: "Nome",
      flex: 1,
      minWidth: 200,
    },
    {
      field: "addressCity",
      headerName: "Cidade",
      flex: 1,
      minWidth: 150,
      valueGetter: (params: string) => params || "-",
    },
    {
      field: "addressState",
      headerName: "Estado",
      width: 100,
      valueGetter: (params: string) => params || "-",
    },
    {
      field: "address",
      headerName: "Endereço Completo",
      flex: 1,
      minWidth: 250,
      valueGetter: (params: address) => {
        const addr = params;
        return `${addr.street || ""}, ${addr.number || ""} - ${
          addr.city || ""
        }/${addr.state || ""}`;
      },
    },
    {
      field: "status",
      headerName: "Status",
      width: 120,
      renderCell: () => <Chip label="Ativo" color="success" size="small" />,
    },
  ];

  const rows = tenants.map((tenant) => ({
    id: tenant.id,
    name: tenant.name,
    addressCity: tenant.address.city,
    addressState: tenant.address.state,
    address: tenant.address,
  }));

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            Gerenciamento de Tenants
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Olá, {user?.userName}! Selecione um tenant para personificar ou
            gerencie os tenants existentes.
          </Typography>
        </Box>

        <Box sx={{ mb: 3 }}>
          <BoilerplateDataGrid
            title="Tenants Disponíveis"
            rows={rows}
            columns={columns}
            loading={loading}
            error={error}
            onAdd={handleCreateTenant}
            onEdit={handleEditTenant}
            onDelete={handleDeleteTenant}
            onRowClick={handleRowClick}
            addButtonText="Novo Tenant"
            height={500}
            pageSize={10}
          />
        </Box>

        <Box sx={{ textAlign: "center", mt: 4 }}>
          <Button
            variant="outlined"
            size="large"
            startIcon={<Dashboard />}
            onClick={handleSkipToDashboard}
            disabled={authLoading}
            sx={{
              minWidth: 200,
              borderColor: "primary.main",
              color: "primary.main",
              "&:hover": {
                borderColor: "primary.dark",
                backgroundColor: "primary.main",
                color: "white",
              },
            }}
          >
            Acessar Dashboard
          </Button>
          <Typography
            variant="caption"
            display="block"
            sx={{ mt: 1, color: "text.secondary" }}
          >
            Você pode selecionar um tenant mais tarde
          </Typography>
        </Box>

        <TenantCreateModal
          open={createModalOpen}
          onClose={handleModalClose}
          onSuccess={handleTenantCreated}
          editTenant={editingTenant}
        />

        <TenantUsersModal
          open={showUsers}
          onClose={handleUsersModalClose}
          tenant={selectedTenant}
        />
      </Box>
    </Container>
  );
};
