// vite.config.ts
import { defineConfig } from 'vite';
import solidPlugin from 'vite-plugin-solid';

export default defineConfig({
    plugins: [solidPlugin()],
    server: {
        proxy: {
            '/ws': {
                target: 'http://localhost:5174',
                changeOrigin: true,
                secure: false,
                ws: true
            }
        }
    }
});