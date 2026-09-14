import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawnGame } from './game-exe.mjs';
import { dataDir } from './paths.mjs';
import { validateOrderText } from './check-orders.mjs';

export async function parseOrdersWithEngine(orderText, factionId) {
  const tmpDir = fs.mkdtempSync(path.join(os.tmpdir(), 'sa-parse-'));
  const orderFile = path.join(tmpDir, `order.check.${factionId}.txt`);
  fs.writeFileSync(orderFile, orderText, 'utf8');
  try {
    const stdout = await spawnGame(['/data', dataDir(), '/parse-orders', orderFile]);
    const trimmed = stdout.trim();
    if (!trimmed.startsWith('{')) {
      throw new Error(trimmed || 'Game.exe returned no parse result');
    }
    return JSON.parse(trimmed);
  } finally {
    fs.rmSync(tmpDir, { recursive: true, force: true });
  }
}

export async function parseOrders(orderText, factionId, password) {
  const syntaxWarnings = validateOrderText(orderText, factionId, password);
  try {
    const engineResult = await parseOrdersWithEngine(orderText, factionId);
    const warnings = [...new Set([...(engineResult.warnings || []), ...syntaxWarnings])];
    return {
      ok: engineResult.ok && warnings.length === 0,
      errors: engineResult.errors || [],
      warnings,
    };
  } catch (err) {
    return {
      ok: false,
      errors: [String(err.message || err)],
      warnings: syntaxWarnings,
      engineUnavailable: true,
    };
  }
}
