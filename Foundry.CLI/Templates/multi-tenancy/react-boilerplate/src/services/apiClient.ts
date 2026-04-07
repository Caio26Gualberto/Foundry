import axios, { type AxiosError, type AxiosRequestConfig, type AxiosResponse } from 'axios';
import { enqueueSnackbar } from 'notistack';
import { type RefreshTokenRequestDto, type TokensDto } from '../types';
import { API_BASE_URL, STORAGE_KEYS } from '../utils/constants';

/**
 * Contrato espelhando `BoilerplateResponse<T>` do backend (JSON em camelCase).
 * Fonte da verdade para respostas da API — usar este tipo ao interpretar payloads.
 */
export interface BoilerplateResponse<T = unknown> {
  isSuccess: boolean;
  message?: string | null;
  data?: T | null;
}

export interface ApiCallOptions extends AxiosRequestConfig {
  /** Sobrescreve a mensagem de erro exibida no Snackbar (quando não silent). */
  errorMessage?: string;
  /** Não exibe Snackbar nem para sucesso nem para erro desta requisição. */
  silent?: boolean;
}

interface ErrorResponseData {
  title?: string;
  status?: number;
  message?: string;
  errors?: Record<string, string[]>;
}

function isBoilerplatePayload(value: unknown): value is BoilerplateResponse<unknown> {
  return typeof value === 'object' && value !== null && 'isSuccess' in value;
}

/** Texto exibível no Snackbar; ignora string vazia ou só espaços. */
function snackbarText(message: string | null | undefined): string | undefined {
  if (typeof message !== 'string') return undefined;
  const t = message.trim();
  return t.length > 0 ? t : undefined;
}

/**
 * 401 em login/registro ou sem refresh token não é "sessão expirada" — não tentar renovar;
 * deixa o fluxo ir para handleError (ex.: mensagem em BoilerplateResponse).
 */
function shouldSkipTokenRefreshOn401(config: AxiosRequestConfig | undefined): boolean {
  const path = `${config?.baseURL ?? ''}${config?.url ?? ''}`.toLowerCase();
  if (path.includes('/auth/login') || path.includes('/auth/register')) {
    return true;
  }
  if (!localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)) {
    return true;
  }
  return false;
}

interface PendingRequest {
  resolve: (value: unknown) => void;
  reject: (error: unknown) => void;
  config: AxiosRequestConfig;
}

class ApiClient {
  private axiosInstance = axios.create({
    baseURL: API_BASE_URL,
    headers: {
      'Content-Type': 'application/json'
    }
  });

  private isRefreshing = false;
  private pendingRequests: PendingRequest[] = [];

  constructor() {
    this.setupInterceptors();
  }

  private setupInterceptors() {
    this.axiosInstance.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
        const impersonatedToken = localStorage.getItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
        if (impersonatedToken) {
          config.headers.Authorization = `Bearer ${impersonatedToken}`;
        }
        else if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    this.axiosInstance.interceptors.response.use(
      (response: AxiosResponse<BoilerplateResponse<unknown>>) => {
        return this.handleBoilerplateResponse(response);
      },
      async (error: AxiosError) => {
        const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

        if (error.response?.status === 401 && !originalRequest._retry) {
          if (shouldSkipTokenRefreshOn401(originalRequest)) {
            return this.handleError(error);
          }
          return this.handle401Error(originalRequest);
        }

        return this.handleError(error);
      }
    );
  }

  private handleBoilerplateResponse(response: AxiosResponse<BoilerplateResponse<unknown>>) {
    const boilerplateResponse = response.data;

    if (!isBoilerplatePayload(boilerplateResponse)) {
      return response;
    }

    const config = response.config as ApiCallOptions;
    const msg = snackbarText(boilerplateResponse.message);

    if (!boilerplateResponse.isSuccess) {
      if (!config?.silent && msg) {
        enqueueSnackbar(msg, { variant: 'error' });
      }

      return Promise.reject(new Error(msg ?? 'Erro na requisição'));
    }

    if (!config?.silent && msg) {
      enqueueSnackbar(msg, { variant: 'success' });
    }

    return {
      ...response,
      data: boilerplateResponse.data as unknown
    };
  }

  private async handle401Error(originalRequest: AxiosRequestConfig & { _retry?: boolean }) {
    if (this.isRefreshing) {
      return new Promise((resolve, reject) => {
        this.pendingRequests.push({ resolve, reject, config: originalRequest });
      });
    }

    // Verifica se existe token de impersonação para decidir o redirecionamento
      const impersonatedToken = localStorage.getItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
  
      if (impersonatedToken) {
        // Se tem token de impersonação, remove ele e redireciona para seleção de tenant
        localStorage.removeItem(STORAGE_KEYS.IMPERSONATED_TOKEN);
        enqueueSnackbar('Sessão de impersonação expirada. Retornando à seleção de tenant.', { variant: 'warning' });
        
        setTimeout(() => {    
            window.location.href = '/tenant-selection';
        }, 2000);
        return Promise.reject(new Error('Impersonation session expired'));
      }

    originalRequest._retry = true;
    this.isRefreshing = true;

    try {
      const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
      
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const refreshResponse = await this.refreshTokenRequest(refreshToken);
      
      if (refreshResponse.token) {
        localStorage.setItem(STORAGE_KEYS.TOKEN, refreshResponse.token);
      }
      
      if (refreshResponse.refreshToken) {
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshResponse.refreshToken);
      }

      this.pendingRequests.forEach(({ resolve, config }) => {
        resolve(this.axiosInstance(config));
      });
      this.pendingRequests = [];

      return this.axiosInstance(originalRequest);

    } catch (refreshError) {
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
      
      this.pendingRequests.forEach(({ reject }) => {
        reject(refreshError);
      });
      this.pendingRequests = [];

      return Promise.reject(refreshError);
    } finally {
      this.isRefreshing = false;
    }
  }

  private async refreshTokenRequest(refreshToken: string): Promise<TokensDto> {
    const refreshData: RefreshTokenRequestDto = { refreshToken };
    
    const response = await axios.post<BoilerplateResponse<TokensDto>>(
      `${API_BASE_URL}/auth/RefreshToken`,
      refreshData,
      {
        headers: {
          'Content-Type': 'application/json'
        }
      }
    );

    const boilerplateResponse = response.data;

    if (!boilerplateResponse.isSuccess) {
      throw new Error(snackbarText(boilerplateResponse.message) ?? 'Failed to refresh token');
    }

    const tokens = boilerplateResponse.data;
    if (!tokens) {
      throw new Error('Failed to refresh token');
    }

    return tokens;
  }

  private handleError(error: AxiosError) {
    const config = error.config as ApiCallOptions;
    const raw = error.response?.data;

    if (config?.silent) {
      return Promise.reject(error);
    }

    if (config?.errorMessage) {
      enqueueSnackbar(config.errorMessage, { variant: 'error' });
      return Promise.reject(error);
    }

    // Erro HTTP com corpo no formato BoilerplateResponse (ex.: middleware / controllers)
    if (isBoilerplatePayload(raw) && raw.isSuccess === false) {
      const msg = snackbarText(raw.message);
      if (msg) {
        enqueueSnackbar(msg, { variant: 'error' });
        return Promise.reject(error);
      }
    }

    const data = raw as ErrorResponseData | undefined;
    const genericMsg = snackbarText(data?.message);
    if (genericMsg) {
      enqueueSnackbar(genericMsg, { variant: 'error' });
      return Promise.reject(error);
    }

    const status = error.response?.status;
    switch (status) {
      case 400:
      case 409:
        enqueueSnackbar(snackbarText(data?.message) ?? 'Dados inválidos', { variant: 'error' });
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

  async get<T>(url: string, config?: ApiCallOptions): Promise<T> {
    const response = await this.axiosInstance.get(url, config);
    return response.data;
  }

  async post<T>(url: string, data?: unknown, config?: ApiCallOptions): Promise<T> {
    const response = await this.axiosInstance.post(url, data, config);
    return response.data;
  }

  async put<T>(url: string, data?: unknown, config?: ApiCallOptions): Promise<T> {
    const response = await this.axiosInstance.put(url, data, config);
    return response.data;
  }

  async delete<T>(url: string, config?: ApiCallOptions): Promise<T> {
    const response = await this.axiosInstance.delete(url, config);
    return response.data;
  }

  async patch<T>(url: string, data?: unknown, config?: ApiCallOptions): Promise<T> {
    const response = await this.axiosInstance.patch(url, data, config);
    return response.data;
  }
}

const apiClient = new ApiClient();
export default apiClient;