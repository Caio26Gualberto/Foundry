export interface User {
  id: string;
  email: string;
  userName: string;
  tenantId?: string;
  roles: string[];
}

export interface Tenant {
  id: string;
  name: string;
  address: address;
}

export interface address {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  number: string;
}

export interface AuthContextType {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  selectTenant: (tenantId: string) => Promise<void>;
  refreshTokens: () => Promise<boolean>;
}

export interface TokensDto {
  token: string;
  refreshToken: string;
}

export interface LoginResponseDto {
  tokens: TokensDto | null;
}

export interface BoilerplateResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: T;
}

export interface LoginInputDto {
  email: string;
  password: string;
}

export interface RefreshTokenRequestDto {
  refreshToken: string;
}

export type UserRole = 'AdminGlobal' | 'GlobalManager' | 'TenantAdmin' | 'User';
