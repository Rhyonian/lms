import axios from "axios";
import { getStoredToken } from "./auth";

const baseURL = (import.meta.env?.VITE_API_BASE_URL as string | undefined) ?? "";

export const apiClient = axios.create({
  baseURL,
  withCredentials: true,
});

apiClient.interceptors.request.use((config) => {
  const token = getStoredToken();
  if (token) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${token}`;
  } else if (config.headers?.Authorization) {
    delete config.headers.Authorization;
  }

  return config;
});

export default apiClient;
