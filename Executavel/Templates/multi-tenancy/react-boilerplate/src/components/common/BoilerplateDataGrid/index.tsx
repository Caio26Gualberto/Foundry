import React from 'react';
import {
  DataGrid,
  GridActionsCellItem,
  type GridColDef,
  type GridRowsProp,
  type GridRowId,
  type GridRowParams,
  type GridSortModel,
  type GridFilterModel,
  type GridPaginationModel,
  type GridLocaleText,
} from '@mui/x-data-grid';
import {
  Box,
  Paper,
  Typography,
  Button,
  Toolbar,
  CircularProgress,
  Alert,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { translate } from '../../../i18n';

export interface BoilerplateDataGridProps {
  title: string;
  rows: GridRowsProp;
  columns: GridColDef[];
  loading?: boolean;
  error?: string;
  onAdd?: () => void;
  onEdit?: (id: GridRowId) => void;
  onDelete?: (id: GridRowId) => void;
  onRowClick?: (params: GridRowParams) => void;
  onSortModelChange?: (model: GridSortModel) => void;
  onFilterModelChange?: (model: GridFilterModel) => void;
  onPaginationModelChange?: (model: GridPaginationModel) => void;
  addButtonText?: string;
  hideAddButton?: boolean;
  pageSize?: number;
  pageSizeOptions?: number[];
  disableRowSelectionOnClick?: boolean;
  checkboxSelection?: boolean;
  height?: number | string;
  sx?: object;
  elevation?: number;
}

export const BoilerplateDataGrid = ({
  title,
  rows,
  columns,
  loading = false,
  error,
  onAdd,
  onEdit,
  onDelete,
  onRowClick,
  onSortModelChange,
  onFilterModelChange,
  onPaginationModelChange,
  addButtonText = 'Adicionar',
  hideAddButton = false,
  pageSize = 10,
  pageSizeOptions = [5, 10, 25, 50],
  disableRowSelectionOnClick = true,
  checkboxSelection = false,
  height = 600,
  sx,
  elevation = 2,
}: BoilerplateDataGridProps) => {
  // Adiciona colunas de ação se onEdit ou onDelete foram fornecidos
  const enhancedColumns: GridColDef[] = React.useMemo(() => {
    const hasActions = onEdit || onDelete;

    if (!hasActions) return columns;

    const actionsColumn: GridColDef = {
      field: 'actions',
      type: 'actions',
      headerName: translate("dataGrid.actionsColumn"),
      width: 100,
      getActions: (params: GridRowParams) => {
        const actions = [];

        if (onEdit) {
          actions.push(
            <GridActionsCellItem
              icon={<EditIcon />}
              label="Editar"
              onClick={() => onEdit(params.id)}
              key="edit"
            />
          );
        }

        if (onDelete) {
          actions.push(
            <GridActionsCellItem
              icon={<DeleteIcon />}
              label="Excluir"
              onClick={() => onDelete(params.id)}
              key="delete"
            />
          );
        }

        return actions;
      },
    };

    return [...columns, actionsColumn];
  }, [columns, onEdit, onDelete]);

  const localeText: Partial<GridLocaleText> = {
    paginationRowsPerPage: translate("dataGrid.paginationRowsPerPage"),
    paginationDisplayedRows: ({ from, to, count, estimated }) => {
      if (!estimated) {
        return `${from}–${to} ${translate("dataGrid.of")} ${count !== -1 ? count : `${translate("dataGrid.moreThan")} ${to}`}`;
      }
      const estimatedLabel = estimated && estimated > to ? `${translate("dataGrid.around")} ${estimated}` : `${translate("dataGrid.moreThan")} ${to}`;
      return `${from}–${to} ${translate("dataGrid.of")} ${count !== -1 ? count : estimatedLabel}`;
    }
  };

  if (error) {
    return (
      <Paper sx={{ p: 3, ...sx }}>
        <Alert severity="error">{error}</Alert>
      </Paper>
    );
  }

  return (
    <Paper elevation={elevation} sx={{ height: '100%', width: '100%', ...sx }}>
      <Toolbar sx={{ px: 2, py: 1, borderBottom: 1, borderColor: 'divider' }}>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          {title}
        </Typography>
        {!hideAddButton && onAdd && (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={onAdd}
            size="small"
          >
            {addButtonText}
          </Button>
        )}
      </Toolbar>

      <Box sx={{ height: height, width: '100%', position: 'relative' }}>
        {loading && (
          <Box
            sx={{
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              bottom: 0,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              backgroundColor: 'rgba(255, 255, 255, 0.7)',
              zIndex: 1,
            }}
          >
            <CircularProgress />
          </Box>
        )}

        <DataGrid
          rows={rows}
          columns={enhancedColumns}
          initialState={{
            pagination: {
              paginationModel: { pageSize, page: 0 },
            },
          }}
          localeText={localeText}
          pageSizeOptions={pageSizeOptions}
          onRowClick={onRowClick}
          onSortModelChange={onSortModelChange}
          onFilterModelChange={onFilterModelChange}
          onPaginationModelChange={onPaginationModelChange}
          disableRowSelectionOnClick={disableRowSelectionOnClick}
          checkboxSelection={checkboxSelection}
          sx={{
            border: 0,
            '& .MuiDataGrid-cell': {
              borderColor: 'divider',
            },
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: 'grey.50',
              borderColor: 'divider',
            },
          }}
        />
      </Box>
    </Paper>
  );
};

export default BoilerplateDataGrid;
