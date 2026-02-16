import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from "path"
import tailwindcss from "@tailwindcss/vite"

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    proxy: {
      // Proxy API calls to the app service
      '/api': {
            target: process.env.SERVER_HTTPS || process.env.SERVER_HTTP,
        secure: false,
        changeOrigin: true
      }
    }
  }
})
