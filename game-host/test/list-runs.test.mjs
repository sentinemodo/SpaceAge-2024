import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';

const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'sa-runs-'));
process.env.REPO_ROOT = tmp;

const { listRuns } = await import('../lib/paths.mjs');

function writeDataXml(runId, mtimeMs) {
  const dir = path.join(tmp, 'play', 'runs', runId, 'data');
  fs.mkdirSync(dir, { recursive: true });
  const file = path.join(dir, 'data.xml');
  fs.writeFileSync(file, '<data/>');
  const when = mtimeMs / 1000;
  fs.utimesSync(file, when, when);
}

test('player-visible campaign is GAME_HOST_RUN_ID (default beta-1), not newest backup', () => {
  process.env.GAME_HOST_RUN_ID = 'beta-1';
  writeDataXml('beta-1', Date.parse('2026-01-01T00:00:00Z'));
  writeDataXml('beta-1-backup-turn2-20261012-2126', Date.parse('2026-06-01T00:00:00Z'));
  fs.mkdirSync(path.join(tmp, 'play', 'runs', 'empty'), { recursive: true });

  const runs = listRuns();
  const visible = runs.filter((run) => run.playerVisible);

  assert.equal(visible.length, 1);
  assert.equal(visible[0].id, 'beta-1');
  assert.equal(runs.find((run) => run.id === 'beta-1-backup-turn2-20261012-2126')?.playerVisible, false);
  assert.equal(runs.find((run) => run.id === 'empty')?.playerVisible, false);
});
