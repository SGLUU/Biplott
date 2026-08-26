export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserDto;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  displayName?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
