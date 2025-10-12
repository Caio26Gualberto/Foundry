import axios, { type AxiosError } from 'axios';
import { enqueueSnackbar } from 'notistack';
import { type ApiCallOptions } from '../types';
import { API_BASE_URL, STORAGE_KEYS } from '../utils/constants';

interface ErrorResponseData {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, error => {
  return Promise.reject(error);
});

apiClient.interceptors.response.use(
  response => response,
  (error: AxiosError) => {
    const config = error.config as ApiCallOptions;
    const data = error.response?.data as ErrorResponseData;

    if (config?.silent) {
      return Promise.reject(error);
    }

    if (config?.errorMessage) {
      enqueueSnackbar(config.errorMessage, { variant: 'error' });
      return Promise.reject(error);
    }

    const status = error.response?.status;
    switch (status) {
      case 400:
      case 409:
        enqueueSnackbar(data?.detail || 'Dados inválidos', { variant: 'error' });
        break;

      case 401:
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        enqueueSnackbar('Sessão expirada. Faça login novamente.', { variant: 'warning' });
        setTimeout(() => { if (window.location.pathname !== '/login') window.location.href = '/login'; }, 2000);
        break;

      case 403:
        enqueueSnackbar('Acesso negado: você não tem permissão.', { variant: 'error' });
        break;

      case 404:
        enqueueSnackbar(data?.title || 'Recurso solicitado não encontrado.', { variant: 'error' });
        break;

      default:
        enqueueSnackbar('Erro inesperado do servidor.', { variant: 'error' });
        break;
    }

    return Promise.reject(error);
  }
);

export default apiClient;