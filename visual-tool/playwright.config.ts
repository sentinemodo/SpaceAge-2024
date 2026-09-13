import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  use: {
    baseURL: 'http://localhost:8787',
  },
  webServer: {
    command: 'npm run preview -- --port 8787 --host',
    cwd: '.',
    port: 8787,
    reuseExistingServer: true,
  },
});
