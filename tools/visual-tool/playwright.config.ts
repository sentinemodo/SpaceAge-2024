import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  use: {
    baseURL: 'http://localhost:8787',
  },
  webServer: {
    command: 'npm start',
    cwd: '../game-host',
    port: 8787,
    reuseExistingServer: true,
    env: {
      GAME_EXE: '../Game/bin/Debug/Game.exe',
    },
  },
});
