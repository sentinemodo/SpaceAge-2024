import { request } from '@playwright/test';

const BASE = process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:8787';
const GM_KEY = process.env.GAME_HOST_GM_KEY || 'dev-gm-key';

async function waitForHealth(): Promise<void> {
  const ctx = await request.newContext();
  for (let i = 0; i < 60; i += 1) {
    try {
      const res = await ctx.get(`${BASE}/health`);
      if (res.ok()) {
        await ctx.dispose();
        return;
      }
    } catch {
      // server still starting
    }
    await new Promise((r) => setTimeout(r, 1000));
  }
  await ctx.dispose();
  throw new Error(`Game host not ready at ${BASE}/health`);
}

export default async function globalSetup(): Promise<void> {
  await waitForHealth();
  const ctx = await request.newContext();
  const headers = { 'X-GM-Key': GM_KEY };
  const init = await ctx.post(`${BASE}/api/gm/init`, { headers });
  if (!init.ok() && init.status() !== 409) {
    throw new Error(`gm/init failed: ${init.status()} ${await init.text()}`);
  }
  const reports = await ctx.post(`${BASE}/api/gm/reports`, { headers });
  if (!reports.ok()) {
    throw new Error(`gm/reports failed: ${reports.status()} ${await reports.text()}`);
  }
  await ctx.dispose();
}
