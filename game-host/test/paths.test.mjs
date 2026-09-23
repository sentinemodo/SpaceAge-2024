import test from 'node:test';
import assert from 'node:assert/strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
process.env.REPO_ROOT = path.resolve(__dirname, '..', '..');

const { runsRoot, runRoot, dataDir, turnDir, factionsDir } = await import('../lib/paths.mjs');

test('runsRoot resolves to play/runs under repo root', () => {
  const root = runsRoot();
  assert.match(root.replace(/\\/g, '/'), /play\/runs$/);
});

test('run layout paths sit under play/runs/<runId>', () => {
  process.env.GAME_HOST_RUN_ID = 'beta-1';
  assert.match(runRoot().replace(/\\/g, '/'), /play\/runs\/beta-1$/);
  assert.match(dataDir().replace(/\\/g, '/'), /play\/runs\/beta-1\/data$/);
  assert.match(turnDir().replace(/\\/g, '/'), /play\/runs\/beta-1\/turn$/);
  assert.match(factionsDir().replace(/\\/g, '/'), /play\/runs\/beta-1\/factions$/);
});
