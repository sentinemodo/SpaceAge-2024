import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {
  listPersonas,
  readPersona,
  readStory,
  savePersona,
  saveStory,
} from '../lib/persona-story.mjs';

test('persona-story read/write in temp run', () => {
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'sa-persona-'));
  const prev = process.env.REPO_ROOT;
  process.env.REPO_ROOT = tmp;
  const runId = 'test-run';
  fs.mkdirSync(path.join(tmp, 'game-host', 'runs', runId, 'factions', '02'), { recursive: true });

  try {
    const personas = listPersonas(runId);
    assert.equal(personas.length, 4);
    savePersona(runId, 'military', '# Military persona\n\nTest.');
    assert.match(readPersona(runId, 'military'), /Military persona/);
    saveStory(runId, 2, '# Story\n\nTurn 2.');
    assert.match(readStory(runId, 2), /Turn 2/);
  } finally {
    if (prev === undefined) delete process.env.REPO_ROOT;
    else process.env.REPO_ROOT = prev;
    fs.rmSync(tmp, { recursive: true, force: true });
  }
});
