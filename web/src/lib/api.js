import axios from "axios";

// Token storage (BACKEND_SPEC §8.9). Access token is short-lived; refresh token rotates.
const ACCESS_KEY = "kc_token";
const REFRESH_KEY = "kc_refresh";

export const tokenStore = {
  get access() { return localStorage.getItem(ACCESS_KEY); },
  get refresh() { return localStorage.getItem(REFRESH_KEY); },
  set({ token, refreshToken }) {
    if (token) localStorage.setItem(ACCESS_KEY, token);
    if (refreshToken) localStorage.setItem(REFRESH_KEY, refreshToken);
  },
  clear() {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};

// API base: an absolute URL in production (the live domain), else same-origin "/api"
// which the Vite dev server proxies to the backend (BACKEND_SPEC §5).
export const API_BASE = import.meta.env.VITE_API_BASE_URL || "/api";

const api = axios.create({
  baseURL: API_BASE,
  headers: { "Content-Type": "application/json" },
  timeout: 15000,
});

// Attach the bearer token on every request.
api.interceptors.request.use((config) => {
  const token = tokenStore.access;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let refreshPromise = null;
let onAuthFailure = null;

/** Lets the AuthProvider react when refresh ultimately fails (force logout). */
export function setAuthFailureHandler(fn) {
  onAuthFailure = fn;
}

async function refreshTokens() {
  const refreshToken = tokenStore.refresh;
  if (!refreshToken) throw new Error("No refresh token");
  // Use a bare axios call so the interceptors below don't recurse.
  const { data } = await axios.post(`${API_BASE}/auth/refresh`, { refreshToken });
  tokenStore.set(data);
  return data.token;
}

// On 401, refresh once and retry; surface everything else as the parsed body (spec §5).
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    const status = error.response?.status;
    const isAuthCall = original?.url?.includes("/auth/");

    if (status === 401 && original && !original._retry && !isAuthCall && tokenStore.refresh) {
      original._retry = true;
      try {
        refreshPromise = refreshPromise || refreshTokens();
        const newToken = await refreshPromise;
        refreshPromise = null;
        original.headers.Authorization = `Bearer ${newToken}`;
        return api(original);
      } catch (refreshErr) {
        refreshPromise = null;
        tokenStore.clear();
        if (onAuthFailure) onAuthFailure();
        return Promise.reject(refreshErr.response?.data ?? refreshErr);
      }
    }

    return Promise.reject(error.response?.data ?? error);
  }
);

export default api;
