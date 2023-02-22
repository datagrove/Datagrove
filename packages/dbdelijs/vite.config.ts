// vite.config.ts
import { defineConfig } from 'vite';
import solidPlugin from 'vite-plugin-solid';

export default defineConfig({
    plugins: [solidPlugin()],
    build: {
        outDir: '../../cmd/dbdeli/dist'
    },
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