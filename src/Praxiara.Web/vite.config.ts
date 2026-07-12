import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const environment = loadEnv(mode, process.cwd(), '')
  const apiUpstream = environment.PRAXIARA_API_UPSTREAM || 'http://localhost:5176'

  return {
    plugins: [react()],
    server: {
      proxy: {
        '/api': {
          target: apiUpstream,
          changeOrigin: true,
        },
        '/hubs': {
          target: apiUpstream,
          changeOrigin: true,
          ws: true,
        },
      },
    },
  }
})
