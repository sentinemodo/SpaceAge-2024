import fs from 'node:fs';
import path from 'node:path';
import { repoRoot } from './paths.mjs';

export function repoEnvFilePath() {
  return path.join(repoRoot(), '.env');
}

/** Load repo-root `.env` into process.env (.env wins over inherited shell vars). */
export function loadRepoEnv() {
  const envFile = repoEnvFilePath();
  if (!fs.existsSync(envFile)) return;
  let content = fs.readFileSync(envFile, 'utf8');
  if (content.charCodeAt(0) === 0xfeff) content = content.slice(1);
  for (const line of content.split(/\r?\n/)) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('#')) continue;
    const eq = trimmed.indexOf('=');
    if (eq <= 0) continue;
    const name = trimmed.slice(0, eq).trim().replace(/^\uFEFF/, '');
    let value = trimmed.slice(eq + 1).trim();
    if ((value.startsWith('"') && value.endsWith('"')) || (value.startsWith("'") && value.endsWith("'"))) {
      value = value.slice(1, -1);
    }
    process.env[name] = value;
  }
}

/** Re-read `.env` and return trimmed RunPod API key (empty if unset). */
export function runpodApiKey() {
  loadRepoEnv();
  return (process.env.RUNPOD_API_KEY || '').trim();
}
