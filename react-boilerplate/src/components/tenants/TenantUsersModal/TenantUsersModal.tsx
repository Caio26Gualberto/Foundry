import React, { useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  IconButton,
  Typography,
} from "@mui/material";
import {
  Close as CloseIcon,
  ArrowBack as ArrowBackIcon,
} from "@mui/icons-material";
import { useSnackbar } from "notistack";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../../contexts/Auth";
import type { GridColDef, GridRowParams } from "@mui/x-data-grid";
import type { Tenant } from "../../../types/tenants";
import type { UserDto } from "../../../types/Users";
import type { TokensDto } from "../../../types";
import { ROUTES, STORAGE_KEYS } from "../../../utils/constants";
import apiClient from "../../../services/apiClient";
import BoilerplateDataGrid from "../../common/BoilerplateDataGrid";
import { translate } from "../../../i18n";

interface TenantUsersModalProps {
  open: boolean;
  onClose: () => void;
  tenant: Tenant | null;
}

export const TenantUsersModal: React.FC<TenantUsersModalProps> = ({
  open,
  onClose,
  tenant,
}) => {
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const auth = useAuth();
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (tenant && open) {
      fetchUsers();
    }
  }, [tenant, open]); // eslint-disable-line react-hooks/exhaustive-deps

  const fetchUsers = async () => {
    if (!tenant) return;

    try {
      setLoading(true);
      setError("");
      if (tenant.users && tenant.users.length > 0) {
        setUsers(tenant.users);
      }
    } catch (err) {
      console.error("Error fetching users:", err);
      const errorMessage =
        err instanceof Error ? err.message : "Erro ao carregar usuários";
      setError(errorMessage);
      setUsers([]);
    } finally {
      setLoading(false);
    }
  };

  const handleUserClick = async (params: GridRowParams) => {
    if (!tenant) return;

    try {
      localStorage.removeItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
      const tokenDto = await apiClient.post<TokensDto>(
        "/tenant/impersonate",
        { tenantId: tenant.id, userId: params.id },
        { silent: false }
      );
      localStorage.setItem(STORAGE_KEYS.IMPERSONATED_TOKEN, tokenDto.token);
      
      // Atualiza o contexto de autenticação com o novo token
      await auth.refreshUserFromToken();
      
      enqueueSnackbar(`Impersonando usuário: ${params.row.name}`, {
        variant: "success",
      });
      navigate(ROUTES.DASHBOARD);
    } catch (error) {
      console.error("Error impersonating user:", error);
      enqueueSnackbar("Erro ao impersonar usuário", { variant: "error" });
    }
  };

  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: translate("tenantSelection.usersGrid.columns.name"),
      flex: 1,
      minWidth: 200,
    },
    {
      field: "id",
      headerName: translate("tenantSelection.usersGrid.columns.id"),
      width: 100,
    },
  ];

  const rows = users.map((user) => ({
    id: user.id,
    name: user.name,
  }));

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 2 },
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Box display="flex" alignItems="center" gap={1}>
            <IconButton onClick={onClose} size="small">
              <ArrowBackIcon />
            </IconButton>
            <Typography variant="h6">
              {translate("tenantSelection.usersGrid.title", { tenantName: tenant?.name })}
            </Typography>
          </Box>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        {users.length > 0 && <Box sx={{ height: 400 }}>
          <BoilerplateDataGrid
            title={translate("tenantSelection.usersGrid.selectUser")}
            rows={rows}
            columns={columns}
            loading={loading}
            error={error}
            onRowClick={handleUserClick}
            height={350}
            pageSize={10}
          />
        </Box>}

        {users.length === 0 && !loading && !error && (
          <Box sx={{ textAlign: "center", py: 4 }}>
            <Typography variant="body1" color="text.secondary">
              Nenhum usuário encontrado para este tenant.
            </Typography>
          </Box>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose}>{translate("button.close")}</Button>
      </DialogActions>
    </Dialog>
  );
};

export default TenantUsersModal;
