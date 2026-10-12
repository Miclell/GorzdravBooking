import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => ({
  plugins: [react()],
  server: {
    host: '127.0.0.1',
    port: Number(loadEnv(mode, process.cwd(), 'PORT').PORT) || 5093,
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.API_TARGET || 'http://localhost:5090',
        changeOrigin: true,
      },
    },
  },
}))
