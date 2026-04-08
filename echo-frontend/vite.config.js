import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173, // Çalışmasını istediğin port
    strictPort: true, // Port doluysa 5174'e geçmez, hata verir ve durur
  }
})
