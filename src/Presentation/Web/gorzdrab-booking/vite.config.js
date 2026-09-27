import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: '127.0.0.1',
    port: Number(process.env.PORT) || 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.API_TARGET || 'http://localhost:5023',
        changeOrigin: true,
      },
    },
  },
})
