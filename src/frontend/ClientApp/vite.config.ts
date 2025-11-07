import { defineConfig } from 'vite'
import { configDefaults } from 'vitest/config'
import react from '@vitejs/plugin-react'

// Build UI into the API's wwwroot so ASP.NET serves it.
export default defineConfig({
  plugins: [react()],
  root: '.',
  build: {
    // Output directory can be overridden with env var for Docker builds
    // Default to writing into the API's wwwroot for local runs and E2E
    outDir: process.env.VITE_OUT_DIR || '../wwwroot',
    // When writing directly into ../wwwroot (local dev/E2E), avoid wiping server assets like /img
    // In Docker builds (VITE_OUT_DIR=dist), it's safe to clean the dist folder
    emptyOutDir: !!process.env.VITE_OUT_DIR
  },
  test: {
    // Use happy-dom to avoid jsdom/parse5 ESM interop issues in Vitest
    environment: 'happy-dom',
    globals: true,
    setupFiles: ['./vitest.setup.ts'],
    css: true,
    // Extend Vitest default excludes and also exclude our Playwright e2e directory
    exclude: [...configDefaults.exclude, 'tests/e2e/**']
  },
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5240',
      '/hubs': 'http://localhost:5240'
    }
  }
})
