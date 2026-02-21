import axios, { type AxiosError, type AxiosRequestConfig, type AxiosResponse } from 'axios';
import { enqueueSnackbar } from 'notistack';
import { type BoilerplateResponse, type RefreshTokenRequestDto, type TokensDto } from '../types';
import { API_BASE_URL, STORAGE_KEYS } from '../utils/constants';

interface ErrorResponseData {
  title?: string;
  status?: number;
  message?: string;
  errors?: Record<string, string[]>;
}

interface ApiCallOptions extends AxiosRequestConfig {
  errorMessage?: string;
  silent?: boolean;
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
        if (token) {
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
          return this.handle401Error(originalRequest);
        }

        return this.handleError(error);
      }
    );
  }

  private handleBoilerplateResponse(response: AxiosResponse<BoilerplateResponse<unknown>>) {
    const BoilerplateResponse = response.data;
    
    if (typeof BoilerplateResponse !== 'object' || !('isSuccess' in BoilerplateResponse)) {
      return response;
    }

    const config = response.config as ApiCallOptions;

    if (!BoilerplateResponse.isSuccess) {
      if (!config?.silent && BoilerplateResponse.message) {
        enqueueSnackbar(BoilerplateResponse.message, { variant: 'error' });
      }
      
      return Promise.reject(new Error(BoilerplateResponse.message || 'Erro na requisição'));
    }

    if (!config?.silent && BoilerplateResponse.message) {
      enqueueSnackbar(BoilerplateResponse.message, { variant: 'success' });
    }

    return {
      ...response,
      data: BoilerplateResponse.data
    };
  }

  private async handle401Error(originalRequest: AxiosRequestConfig & { _retry?: boolean }) {
    if (this.isRefreshing) {
      return new Promise((resolve, reject) => {
        this.pendingRequests.push({ resolve, reject, config: originalRequest });
      });
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

    const BoilerplateResponse = response.data;
    
    if (!BoilerplateResponse.isSuccess) {
      throw new Error(BoilerplateResponse.message || 'Failed to refresh token');
    }

    return BoilerplateResponse.data;
  }

  private handleError(error: AxiosError) {
    const config = error.config as ApiCallOptions;
    const data = error.response?.data as ErrorResponseData;

    if (config?.silent) {
      return Promise.reject(error);
    }

    if (config?.errorMessage) {
      enqueueSnackbar(config.errorMessage, { variant: 'error' });
      return Promise.reject(error);
    }
    
    if (data?.message) {
      enqueueSnackbar(data.message, { variant: 'error' });
      return Promise.reject(error);
    }

    const status = error.response?.status;
    switch (status) {
      case 400:
      case 409:
        enqueueSnackbar(data?.message || 'Dados inválidos', { variant: 'error' });
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