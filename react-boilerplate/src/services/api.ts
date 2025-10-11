import { API_BASE_URL, STORAGE_KEYS } from '../utils/constants';
import type { 
  LoginResponseDto, 
  User, 
  Tenant, 
  BoilerplateResponse, 
  LoginInputDto, 
  TokensDto, 
  RefreshTokenRequestDto 
} from '../types';

class ApiService {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  private async handleResponse<T>(response: Response): Promise<BoilerplateResponse<T>> {
    const result = await response.json();
    
    if (!response.ok) {
      throw new Error(result.message || `HTTP error! status: ${response.status}`);
    }
    
    return result;
  }

  async login(email: string, password: string): Promise<BoilerplateResponse<LoginResponseDto>> {
    const loginData: LoginInputDto = { email, password };
    
    const response = await fetch(`${API_BASE_URL}/auth/Login`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(loginData),
    });

    return this.handleResponse<LoginResponseDto>(response);
  }

  async logout(): Promise<BoilerplateResponse<boolean>> {
    const response = await fetch(`${API_BASE_URL}/auth/Logout`, {
      method: 'GET',
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<boolean>(response);
  }

  async refreshToken(refreshToken: string): Promise<BoilerplateResponse<TokensDto>> {
    const refreshData: RefreshTokenRequestDto = { refreshToken };
    
    const response = await fetch(`${API_BASE_URL}/auth/RefreshToken`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(refreshData),
    });

    return this.handleResponse<TokensDto>(response);
  }

  // Método para decodificar JWT e extrair dados do usuário
  private decodeJWT(token: string): User | null {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );

      const payload = JSON.parse(jsonPayload);
      
      return {
        id: payload.sub || payload.userId || payload.id,
        email: payload.email,
        userName: payload.unique_name || payload.userName || payload.name,
        tenantId: payload.tenantId,
        roles: Array.isArray(payload.role) ? payload.role : [payload.role].filter(Boolean),
      };
    } catch (error) {
      console.error('Error decoding JWT:', error);
      return null;
    }
  }

  getUserFromToken(token: string): User | null {
    return this.decodeJWT(token);
  }

  async getTenants(): Promise<BoilerplateResponse<Tenant[]>> {
    const response = await fetch(`${API_BASE_URL}/tenant`, {
      headers: this.getAuthHeaders(),
      method: 'POST',  
    });

    const result = await this.handleResponse<Tenant[]>(response);
    return result;
  }

  async impersonateTenant(tenantId: string): Promise<BoilerplateResponse<LoginResponseDto>> {
    const response = await fetch(`${API_BASE_URL}/auth/impersonate-tenant`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ tenantId }),
    });

    return this.handleResponse<LoginResponseDto>(response);
  }
}

export const apiService = new ApiService();
