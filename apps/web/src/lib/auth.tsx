import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { apiClient } from "./api";

export type AuthUser = {
  id: string;
  email: string;
  [key: string]: unknown;
};

export type AuthContextValue = {
  user: AuthUser | null;
  token: string | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<AuthUser>;
  logout: () => void;
};

const AUTH_STORAGE_KEY =
  (import.meta.env?.VITE_AUTH_STORAGE_KEY as string | undefined) ?? "lms_jwt";
const API_BASE_URL =
  (import.meta.env?.VITE_API_BASE_URL as string | undefined) ?? "";

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const isBrowser = typeof window !== "undefined";

export const getStoredToken = (): string | null => {
  if (!isBrowser) {
    return null;
  }

  try {
    return window.localStorage.getItem(AUTH_STORAGE_KEY);
  } catch (error) {
    console.warn("Failed to read auth token from storage", error);
    return null;
  }
};

const storeToken = (token: string | null) => {
  if (!isBrowser) {
    return;
  }

  try {
    if (token) {
      window.localStorage.setItem(AUTH_STORAGE_KEY, token);
    } else {
      window.localStorage.removeItem(AUTH_STORAGE_KEY);
    }
  } catch (error) {
    console.warn("Failed to persist auth token", error);
  }
};

const fetchMe = async (): Promise<AuthUser> => {
  const response = await apiClient.get("/me");
  return response.data;
};

type AuthProviderProps = {
  children?: React.ReactNode;
};

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(() => getStoredToken());
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    let isActive = true;

    const initialise = async () => {
      if (!token) {
        setLoading(false);
        setUser(null);
        return;
      }

      setLoading(true);
      try {
        const profile = await fetchMe();
        if (isActive) {
          setUser(profile);
        }
      } catch (error) {
        if (isActive) {
          setUser(null);
          setToken(null);
          storeToken(null);
        }
      } finally {
        if (isActive) {
          setLoading(false);
        }
      }
    };

    initialise();

    return () => {
      isActive = false;
    };
  }, [token]);

  const login = useCallback(async (email: string, password: string) => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Unable to login");
      }

      const payload = await response.json();
      const accessToken = payload?.accessToken as string | undefined;

      if (!accessToken) {
        throw new Error("Login response did not include an access token");
      }

      storeToken(accessToken);
      setToken(accessToken);

      const profile = await fetchMe();
      setUser(profile);
      return profile;
    } catch (error) {
      storeToken(null);
      setToken(null);
      setUser(null);
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    storeToken(null);
    setToken(null);
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ user, token, loading, login, logout }),
    [loading, login, logout, token, user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
};

export const clearStoredToken = () => storeToken(null);
export const setStoredToken = (token: string | null) => storeToken(token);
export const AUTH_TOKEN_STORAGE_KEY = AUTH_STORAGE_KEY;
