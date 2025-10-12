export interface User {
  id: string;
  email: string;
  nome: string;
  userName: string;
  dataCriacao: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SigninResponse {
  token: string;
  user: User;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
  isLoading: boolean;
  error: string | null;
}
