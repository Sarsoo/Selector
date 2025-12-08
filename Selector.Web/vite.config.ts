import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
    plugins: [ vue() ],
    build: {
        outDir: "wwwroot/js",
        cssCodeSplit: true,
        sourcemap: true,
        rollupOptions: {
            input: {
                now: './scripts/now.ts',
                past: './scripts/past.ts'
            }
        }
    }
})