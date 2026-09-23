import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  globalSetup: './e2e/global-setup.ts',
  use: {
    baseURL: 'http://localhost:8787',
    channel: process.env.PLAYWRIGHT_CHANNEL || 'chrome',
  },
  webServer: {
    command: 'npm start',
    cwd: '../game-host',
    port: 8787,
    reuseExistingServer: !process.env.CI,
    timeout: 120_000,
    env: {
      GAME_EXE: '../Game/bin/Debug/Game.exe',
      GAME_HOST_GM_KEY: process.env.GAME_HOST_GM_KEY || 'dev-gm-key',
    },
  },
});
