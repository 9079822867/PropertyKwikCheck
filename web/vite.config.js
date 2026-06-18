import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

// The Axios client calls same-origin "/api" (BACKEND_SPEC §5); the dev server proxies
// that to the API. Default target is the LIVE API; override with VITE_API_TARGET in
// web/.env (e.g. http://localhost:5107) to develop against a local backend.
// Proxying (rather than a direct cross-origin call) avoids CORS entirely.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const API_TARGET = env.VITE_API_TARGET || "https://property.kwikcheck.in";

  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: {
        "/api": {
          target: API_TARGET,
          changeOrigin: true,
          secure: true,
        },
      },
    },
  };
});
