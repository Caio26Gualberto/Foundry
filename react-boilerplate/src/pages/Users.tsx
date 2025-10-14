import React, { useState, useEffect } from "react";
import { Box, Typography, Container, Chip } from "@mui/material";
import { People } from "@mui/icons-material";
import { useSnackbar } from "notistack";
import type { GridColDef, GridRowParams, GridRowId } from "@mui/x-data-grid";
import type { UserDto } from "../types/Users";
import { useAuth } from "../contexts/Auth";
import BoilerplateDataGrid from "../components/common/BoilerplateDataGrid";
import InviteUserModal from "../components/users/InviteUserModal/InviteUserModal";
import { useConfirmation } from "../contexts/confirmationContext/ConfirmationProvider";

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
      // const usersData = await apiClient.get<UserDto[]>("/users");
      const usersData = [
        {
          id: "1",
          name: "John Doe",
          email: "john.doe@example.com",
          status: "active",
          role: "User",
        },
        {
          id: "2",
          name: "Jane Smith",
          email: "jane.smith@example.com",
          status: "active",
          role: "User",
        },
      ];
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
      // await apiClient.delete(`/users/${id}`);
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
      headerName: "Nome",
      flex: 1,
      minWidth: 200,
    },
    {
      field: "email",
      headerName: "Email",
      flex: 1,
      minWidth: 250,
    },
    {
      field: "status",
      headerName: "Status",
      width: 120,
      renderCell: (params) => {
        const status = params.value || "active";
        const color = status === "active" ? "success" : status === "pending" ? "warning" : "default";
        const label = status === "active" ? "Ativo" : status === "pending" ? "Pendente" : "Inativo";
        return <Chip label={label} color={color} size="small" />;
      },
    },
    {
      field: "role",
      headerName: "Função",
      width: 150,
      renderCell: (params) => {
        const role = params.value || "User";
        return <Chip label={role} variant="outlined" size="small" />;
      },
    },
  ];

  const rows = users.map((user) => ({
    id: user.id,
    name: user.name,
    email: user.email || "-",
    status: "active", // TODO: Adicionar status real quando disponível
    role: "User", // TODO: Adicionar role real quando disponível
  }));

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            Gerenciamento de Usuários
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Gerencie os usuários do seu tenant e envie convites para novos membros.
          </Typography>
          {user?.tenantName && (
            <Box sx={{ mt: 2 }}>
              <Chip 
                label={`Tenant: ${user.tenantName}`} 
                color="primary" 
                size="small" 
                icon={<People />}
              />
            </Box>
          )}
        </Box>

        <Box sx={{ mb: 3 }}>
          <BoilerplateDataGrid
            title="Usuários do Tenant"
            rows={rows}
            columns={columns}
            loading={loading}
            error={error}
            onAdd={handleInviteUser}
            onEdit={handleEditUser}
            onDelete={handleDeleteUser}
            onRowClick={handleRowClick}
            addButtonText="Convidar Usuário"
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
