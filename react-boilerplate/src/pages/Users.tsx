import React, { useState, useEffect } from "react";
import { Box, Typography, Container, Chip, Stack } from "@mui/material";
import { People } from "@mui/icons-material";
import { useSnackbar } from "notistack";
import type { GridColDef, GridRowParams, GridRowId } from "@mui/x-data-grid";
import type { UserDto } from "../types/Users";
import { useAuth } from "../contexts/Auth";
import BoilerplateDataGrid from "../components/common/BoilerplateDataGrid";
import InviteUserModal from "../components/users/InviteUserModal/InviteUserModal";
import { useConfirmation } from "../contexts/confirmationContext/ConfirmationProvider";
import apiClient from "../services/apiClient";
import { translate } from "../i18n";

export const Users: React.FC = () => {
  const { user } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const confirm = useConfirmation();
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [inviteModalOpen, setInviteModalOpen] = useState(false);

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const usersData = await apiClient.get<UserDto[]>("/user");
      setUsers(usersData);
      setError("");
    } catch (err) {
      console.error("Error fetching users:", err);
      const errorMessage =
        err instanceof Error ? err.message : "Erro ao carregar usuários";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleInviteUser = () => {
    setInviteModalOpen(true);
  };

  const handleInviteSuccess = () => {
    fetchUsers();
    setInviteModalOpen(false);
  };

  const handleEditUser = (id: GridRowId) => {
    // TODO: Implementar edição de usuário
    console.log("Edit user:", id);
    enqueueSnackbar("Funcionalidade de edição em desenvolvimento", {
      variant: "info",
    });
  };

  const handleDeleteUser = async (id: GridRowId) => {
    const result = await confirm({
      title: "Excluir Usuário",
      message: "Tem certeza que deseja excluir este usuário?",
    });
    if (!result) return;

    try {
      await apiClient.delete(`/user/${id}`);
      fetchUsers();
      enqueueSnackbar("Usuário excluído com sucesso", {
        variant: "success",
      });
    } catch (error) {
      console.error("Error deleting user:", error);
      enqueueSnackbar("Erro ao excluir usuário", { variant: "error" });
    }
  };

  const handleRowClick = (params: GridRowParams) => {
    // TODO: Implementar visualização de detalhes do usuário
    console.log("View user details:", params.id);
  };

  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: translate("usersManagement.usersGrid.columns.name"),
      flex: 1,
      minWidth: 200,
    },
    {
      field: "email",
      headerName: translate("usersManagement.usersGrid.columns.email"),
      flex: 1,
      minWidth: 250,
    },
{
    field: "roles",
    headerName: translate("usersManagement.usersGrid.columns.roles"),
    minWidth: 200,
    flex: 1,
    renderCell: (params) => {
      const roles: string[] = params.value || [];

      if (!roles.length) {
        return <Chip label="User" size="small" variant="outlined" />;
      }

      return (
        <Stack direction="row" spacing={0.5} mt={2} sx={{ flexWrap: "wrap" }}>
          {roles.map((role) => (
            <Chip key={role} label={role} size="small" variant="outlined" />
          ))}
        </Stack>
      );
    },
  },
  ];

  const rows = users.map((user) => ({
    id: user.id,
    name: user.name,
    email: user.email || "-",
    roles: user.roles || [],
  }));

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            {translate("usersManagement.title")}
          </Typography>
          <Typography variant="body1" color="text.secondary">
            {translate("usersManagement.description")}
          </Typography>
          {user?.tenantName && (
            <Box sx={{ mt: 2 }}>
              <Chip 
                label={translate("usersManagement.tenantName", { tenantName: user.tenantName })} 
                color="primary" 
                size="small" 
                icon={<People />}
              />
            </Box>
          )}
        </Box>

        <Box sx={{ mb: 3 }}>
          <BoilerplateDataGrid
            title={translate("usersManagement.usersGrid.title", { tenantName: user?.tenantName })}
            rows={rows}
            columns={columns}
            loading={loading}
            error={error}
            onAdd={handleInviteUser}
            onEdit={handleEditUser}
            onDelete={handleDeleteUser}
            onRowClick={handleRowClick}
            addButtonText={translate("usersManagement.usersGrid.buttonInvite")}
            height={500}
            pageSize={10}
          />
        </Box>

        <InviteUserModal
          open={inviteModalOpen}
          onClose={() => setInviteModalOpen(false)}
          onSuccess={handleInviteSuccess}
        />
      </Box>
    </Container>
  );
};

export default Users;
