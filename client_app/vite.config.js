import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5170,
    strictPort: true,
    proxy: {
      '/api': 'http://localhost:5270' // asp.net core backend port
    }
  },
  build: {
    outDir: 'dist',
    emptyOutDir: true
  }
});
