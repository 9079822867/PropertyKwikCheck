import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// The Axios client uses baseURL "/api" (BACKEND_SPEC §5). In dev we proxy that
// to the .NET API so the same-origin contract holds without code changes.
const API_TARGET = process.env.VITE_API_TARGET || "http://localhost:5080";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: API_TARGET,
        changeOrigin: true,
      },
    },
  },
});
