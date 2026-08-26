import { create } from "zustand";
import { UserDto, LoginRequest, RegisterRequest } from "@/types/auth";
import {
  apiLogin,
  apiRegister,
  apiLogout,
  apiRefreshToken,
  apiGetCurrentUser,
  setAuthToken
} from "@/lib/api";

interface AuthState {
  user: UserDto | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isInitialized: boolean;

  initAuth: () => Promise<void>;
  login: (req: LoginRequest) => Promise<void>;
  register: (req: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  accessToken: null,
  isAuthenticated: false,
  isLoading: false,
  isInitialized: false,

  initAuth: async () => {
    try {
      set({ isLoading: true });
      // Attempt silent refresh via HttpOnly cookie
      const res = await apiRefreshToken();
      if (res && res.accessToken) {
        setAuthToken(res.accessToken);
        const user = await apiGetCurrentUser();
        set({
          user,
          accessToken: res.accessToken,
          isAuthenticated: true,
          isLoading: false,
          isInitialized: true
        });
        return;
      }
    } catch {
      // Guest mode: no active session
      setAuthToken(null);
    } finally {
      set({ isLoading: false, isInitialized: true });
    }
  },

  login: async (req: LoginRequest) => {
    set({ isLoading: true });
    try {
      const res = await apiLogin(req);
      setAuthToken(res.accessToken);
      set({
        user: res.user,
        accessToken: res.accessToken,
        isAuthenticated: true,
        isLoading: false
      });
    } catch (err) {
      set({ isLoading: false });
      throw err;
    }
  },

  register: async (req: RegisterRequest) => {
    set({ isLoading: true });
    try {
      const res = await apiRegister(req);
      setAuthToken(res.accessToken);
      set({
        user: res.user,
        accessToken: res.accessToken,
        isAuthenticated: true,
        isLoading: false
      });
    } catch (err) {
      set({ isLoading: false });
      throw err;
    }
  },

  logout: async () => {
    set({ isLoading: true });
    try {
      await apiLogout();
    } finally {
      setAuthToken(null);
      set({
        user: null,
        accessToken: null,
        isAuthenticated: false,
        isLoading: false
      });
    }
  }
}));
