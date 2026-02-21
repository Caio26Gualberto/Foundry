export interface User {
  id: string;
  email: string;
  userName: string;
  roles: string[];
}
export interface AuthContextType {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<{ isNeededChangePassword: boolean }>;
  logout: () => void;
  refreshTokens: () => Promise<boolean>;
  refreshUserFromToken: () => Promise<void>;
}

export interface TokensDto {
  token: string;
  refreshToken: string;
}

export interface LoginResponseDto {
  tokens: TokensDto | null;
  isNeededChangePassword: boolean;
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

export type UserRole = 'AdminGlobal' | 'GlobalManager' | 'User';

export interface ApiCallOptions {
  errorMessage?: string;
  silent?: boolean;
}
