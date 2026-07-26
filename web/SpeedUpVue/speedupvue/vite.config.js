import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
    plugins: [ vue() ],
    server: {
        port: 5173,
        https: false,
        proxy: {
            // Proxy /api requests to the local SpeedUpMiniAPI
            '/api': {
                target: 'https://localhost:7110',
                secure: false,
                changeOrigin: true,
                // if your backend uses HTTPS with a self-signed cert, secure:false helps during dev
            }
        }
    },
    preview: { port: 5173 },
    resolve: {
        alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) }
    }
})
